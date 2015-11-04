#define COLOR // Linux might have some color issues, so comment this line if you don't want color.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;

namespace No
{
    class Program
    {
        static string DataPath = "./nodata/";
        static PasswordList List;
        static Dictionary<string, Action> Actions = new Dictionary<string, Action>()
        {
            { "Create a keylist", CreateNewList },
            { "Unlock keylist", UnlockList }
        };

        static byte[] Key;

        static void Main(string[] args)
        {
            while(true)
            {
                EnumerateActions();
                InvokeAction();
            }
        }

        static void EnumerateActions()
        {
            Console.WriteLine();
            int count = 1;

            foreach(var pair in Actions)
            {
                Console.WriteLine("{0}) {1}", count++, pair.Key);
            }
        }

        static void InvokeAction()
        {
            string str = Console.ReadKey(true).KeyChar.ToString();
            int selection = -1;

            if (!int.TryParse(str, out selection))
                return;

            if (selection < 1 || selection > Actions.Count)
                return;

            Console.Clear();

            var pair = GetAction(selection);

            Console.Write("Selected ");
            SetConsoleColor(ConsoleColor.White);
            Console.Write(pair.Key);
            SetConsoleColor(ConsoleColor.Gray);
            Console.WriteLine(".");

            try
            {
                pair.Value();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        static KeyValuePair<string, Action> GetAction(int index)
        {
            int i = 1;

            foreach(var pair in Actions)
            {
                if (index == i++)
                    return pair;
            }

            return new KeyValuePair<string, Action>("", delegate { });
        }

        static void GeneratePassword()
        {
            string name = Prompt("Service name");
            string password = List.Generate(name);

            Console.Write("Password for ");
            SetConsoleColor(ConsoleColor.Green);
            Console.Write(name);
            SetConsoleColor(ConsoleColor.Gray);
            Console.Write(" is ");
            SetConsoleColor(ConsoleColor.Red);
            Console.Write(password);
            SetConsoleColor(ConsoleColor.Gray);
            Console.WriteLine(".");

            Utilities.SetClipboard(password);
            Console.WriteLine("Copied password to clipboard.");

            Save();
        }

        static void SetConsoleColor(ConsoleColor color)
        {
#if COLOR
            Console.ForegroundColor = color;
#endif
        }

        static void RetrievePassword()
        {
            string name = Prompt("Service name");

            if (string.IsNullOrWhiteSpace(List.Retrieve(name)))
                Console.WriteLine("No such service.");
            else
            {
                string password = List.Retrieve(name);

                Console.Write("Password for ");
                SetConsoleColor(ConsoleColor.Green);
                Console.Write(name);
                SetConsoleColor(ConsoleColor.Gray);
                Console.Write(" is ");
                SetConsoleColor(ConsoleColor.Red);
                Console.Write(password);
                SetConsoleColor(ConsoleColor.Gray);
                Console.WriteLine(".");

                Utilities.SetClipboard(password);
                Console.WriteLine("Copied password to clipboard.");
            }
        }

        static void UnlockList()
        {
            try
            {
                if (!File.Exists(GetAbsolutePath("list")))
                {
                    Console.WriteLine("Keylist doesn't exist!");
                    return;
                }
                if (!File.Exists(GetAbsolutePath("salt")))
                {
                    Console.WriteLine("Salt doesn't exist!");
                    return;
                }

                string password = DiscretePrompt("Enter password");
                byte[] salt = File.ReadAllBytes(GetAbsolutePath("salt"));

                File.WriteAllBytes(GetAbsolutePath("salt"), salt);

                Console.Write("Generating key...");
                byte[] key = CryptoBox.GenerateKeyFromPassword(password, salt, CryptoBox.DefaultKeySize);
                Key = key;
                Console.WriteLine("done.");

                Console.Write("Decrypting keylist...");
                List = CryptoBox.SafeDeserialize<PasswordList>(key, GetAbsolutePath("list"));
                Console.WriteLine("done.");

                Actions.Clear();

                Actions.Add("Retrieve password", RetrievePassword);
                Actions.Add("Generate password", GeneratePassword);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed!");
                Console.WriteLine(ex);
            }
        }

        static void Save()
        {
            CryptoBox.SafeSerialize(List, Key, GetAbsolutePath("list"));
        }

        static string GetAbsolutePath(string path)
        {
            return Path.Combine(DataPath, path);
        }

        static void CreateNewList()
        {
            string password = DiscretePrompt("Enter password");
            byte[] salt = CryptoBox.GetRandomBytes(16);

            File.WriteAllBytes(GetAbsolutePath("salt"), salt);

            Console.Write("Generating key...");
            byte[] key = CryptoBox.GenerateKeyFromPassword(password, salt, CryptoBox.DefaultKeySize);
            Key = key;
            Console.WriteLine("done.");

            PasswordList list = new PasswordList();

            CryptoBox.SafeSerialize(list, key, GetAbsolutePath("list"));

            List = list;

            Console.WriteLine("Created new encrypted keylist.");

            Actions.Clear();

            Actions.Add("Retrieve password", RetrievePassword);
            Actions.Add("Generate password", GeneratePassword);
        }

        static string Prompt(string prompt)
        {
            Console.Write("{0}: ", prompt);
            return Console.ReadLine();
        }

        static string DiscretePrompt(string prompt)
        {
            Console.Write("{0}: ", prompt);

            StringBuilder sb = new StringBuilder();

            int x = Console.CursorLeft;

            int index = -1;
            int length = 0;
            bool finished = false;

            while(true)
            {
                var key = Console.ReadKey(true);

                switch(key.Key)
                {
                    case ConsoleKey.LeftArrow:
                        if (index > 0)
                        {
                            index--;
                            Console.CursorLeft--;
                        }
                        break;
                    case ConsoleKey.RightArrow:
                        if (index != sb.Length - 1)
                        {
                            index++;
                            Console.CursorLeft++;
                        }
                        break;
                    case ConsoleKey.Backspace:
                        if (index >= 0)
                        {
                            sb.Remove(index, 1);
                            index--;
                            length--;
                        }
                        break;
                    case ConsoleKey.Enter:
                        finished = true;
                        break;
                }

                if (finished)
                    break;

                if(!char.IsControl(key.KeyChar))
                {
                    index++;
                    length++;

                    sb.Insert(index, key.KeyChar);
                }

                Console.CursorLeft = x;
                Console.Write(new string('█', length));
                Console.Write(new string(' ', (Console.BufferWidth - Console.CursorLeft) - 1));
                Console.CursorLeft = x + index + 1;
            }

            Console.WriteLine();
            return sb.ToString();
        }
    }
}
