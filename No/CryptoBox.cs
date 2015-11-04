using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Security.Cryptography;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace No
{
    public class CryptoBox
    {
        static RNGCryptoServiceProvider Random = new RNGCryptoServiceProvider();

        public static int Iterations = 1000000;
        public static int DefaultKeySize = 20;

        public static byte[] GenerateKeyFromPassword(string password, byte[] salt, int length)
        {
            Rfc2898DeriveBytes pbkdf = new Rfc2898DeriveBytes(password, salt, Iterations);
            return pbkdf.GetBytes(length);
        }

        public static byte[] GetRandomBytes(int length)
        {
            byte[] ret = new byte[length];
            Random.GetBytes(ret);
            return ret;
        }

        public static void SafeSerialize<T>(T obj, byte[] key, Stream stream)
        {
            IFormatter formatter = new BinaryFormatter();
            AesCryptoServiceProvider aes = new AesCryptoServiceProvider();
            aes.KeySize = 256;
            aes.Key = Pad(key, aes.KeySize / 8);
            stream.Write(aes.IV, 0, aes.IV.Length);

            Utilities.LogMessage("Serializing, new IV is {0}", Utilities.GetString(aes.IV));

            var transform = aes.CreateEncryptor();

            CryptoStream cs = new CryptoStream(stream, transform, CryptoStreamMode.Write);
            
            formatter.Serialize(cs, obj);

            cs.Close();
        }

        public static T SafeDeserialize<T>(byte[] key, Stream stream)
        {
            BinaryReader br = new BinaryReader(stream);

            AesCryptoServiceProvider aes = new AesCryptoServiceProvider();
            aes.KeySize = 256;
            aes.Key = Pad(key, aes.KeySize / 8);

            byte[] iv = new byte[aes.IV.Length];
            stream.Read(iv, 0, iv.Length);
            aes.IV = iv;

            Utilities.LogMessage("Read IV as {0}, {1}", Utilities.GetString(iv), Utilities.GetString(aes.IV));

            var transform = aes.CreateDecryptor();

            IFormatter formatter = new BinaryFormatter();

            CryptoStream cs = new CryptoStream(stream, transform, CryptoStreamMode.Read);

            T ret = (T)formatter.Deserialize(cs);

            cs.Close();

            return ret;
        }

        public static T SafeDeserialize<T>(byte[] key, string filename)
        {
            return SafeDeserialize<T>(key, new FileStream(filename, FileMode.Open));
        }

        public static void SafeSerialize<T>(T obj, byte[] key, string filename)
        {
            SafeSerialize<T>(obj, key, new FileStream(filename, FileMode.Create));
        }

        public static byte[] Pad(byte[] data, int length)
        {
            byte[] ret = new byte[length];

            data.CopyTo(ret, 0);

            for (int i = data.Length; i < length; i++)
            {
                ret[i] = 0;
            }

            return ret;
        }
    }
}
