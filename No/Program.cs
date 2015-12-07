#define COLOR // Linux might have some color issues, so comment this line if you don't want color.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using System.Threading;

namespace No
{
    class Program
    {
        static string DataPath = Utilities.GetDataRoot();
        static PasswordList List;
        static Dictionary<string, Action> Actions = new Dictionary<string, Action>()
        {
            { "Create a keylist", CreateNewList },
            { "Unlock keylist", UnlockList }
        };

        static byte[] Key;

        static void Main(string[] args)
        {
            if (!Directory.Exists(DataPath))
            {
                if (Directory.Exists("./nodata/") &&
                    PromptConfirm(string.Format("It appears that you have a keylist in the old data directory(\"./nodata/\"). Would you like to migrate that to the new data directory({0})?", DataPath)))
                {
                    Utilities.ProperMove("./nodata/", DataPath);
                }
                else
                    Directory.CreateDirectory(DataPath);
            }

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

        static void ListPasswords()
        {
            foreach(var entry in List.Passwords)
            {
                Console.Write("Name: ");
                SetConsoleColor(ConsoleColor.Green);
                Console.Write(entry.Name);
                SetConsoleColor(ConsoleColor.Gray);
                Console.Write(", password: ");
                SetConsoleColor(ConsoleColor.Red);
                Console.WriteLine(entry.Password);
                SetConsoleColor(ConsoleColor.Gray);
            }

            for(int i = 5; i > 0; i--)
            {
                if(i != 5)
                    Console.CursorTop--;

                Console.WriteLine("Clearing in {0}...", i);
                Thread.Sleep(1000);
            }

            Console.Clear();
        }

        static void GeneratePassword()
        {
            string name = Prompt("Service name");

            while (List.Contains(name))
            {
                if (!PromptConfirm("A password with that name already exists. Overwrite?"))
                    name = Prompt("Service name");
                else
                    break;
            }

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

            if (!List.Contains(name))
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
                Actions.Add("List all passwords", ListPasswords);
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
            if(File.Exists(GetAbsolutePath("list")))
            {
                if (!PromptConfirm("You already have a keylist. Overwrite?"))
                    return;
            }

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
            Actions.Add("List all passwords", ListPasswords);
        }

        static bool PromptConfirm(string prompt)
        {
            Console.Write("{0} [y/n]", prompt);

            var key = Console.ReadKey(true);
            Console.WriteLine();
            string character = key.KeyChar.ToString().ToLower();

            switch(character)
            {
                case "y":
                    return true;
                case "n":
                    return false;
                default:
                    return PromptConfirm(prompt);
            }
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
