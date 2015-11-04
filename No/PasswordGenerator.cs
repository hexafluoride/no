using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace No
{
    public class PasswordGenerator
    {
        public int Length = 32;
        public Characters Characters = Characters.Letters | Characters.LettersUppercase | Characters.Numbers | Characters.SymbolsBasic;

        private string _rangeCache = "";
        private Characters _previousRange = 0;

        public PasswordGenerator()
        {

        }

        public string Generate()
        {
            string range = GetRangeFromCharacters(Characters);
            byte[] random = CryptoBox.GetRandomBytes(Length);
            StringBuilder sb = new StringBuilder();

            for(int i = 0; i < random.Length; i++)
            {
                byte b = random[i];
                b = (byte)(b * ((double)range.Length / 256d));

                sb.Append(range[b]);
            }

            return sb.ToString();
        }

        private string GetRangeFromCharacters(Characters range)
        {
            if (range == _previousRange)
                return _rangeCache;

            StringBuilder sb = new StringBuilder();

            if (range.HasFlag(Characters.Letters))
                sb.Append("abcdefghijklmnopqrstuvwxyz");

            if (range.HasFlag(Characters.LettersUppercase))
                sb.Append("ABCDEFGHIJKLMNOPQRSTUVWXYZ");

            if (range.HasFlag(Characters.Numbers))
                sb.Append("0123456789");

            if (range.HasFlag(Characters.SymbolsBasic))
                sb.Append("!-_.+");

            if (range.HasFlag(Characters.SymbolsAdvanced))
                sb.Append("'^%&/()=?~;,{[]}");

            string ret = sb.ToString();

            _previousRange = range;
            _rangeCache = ret;

            return ret;
        }
    }
    
    public enum Characters
    {
        Letters = 0x1,
        LettersUppercase = 0x2,
        Numbers = 0x4,
        SymbolsBasic = 0x8,
        SymbolsAdvanced = 0x8 | 0x16
    }
}
