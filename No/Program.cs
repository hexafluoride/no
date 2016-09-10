using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using System.Threading;

using No.UI;
using No.UI.Controls;

namespace No
{
    class Program
    {
        static int AutoClearDelay = 10000;
        static string DataPath = Utilities.GetDataRoot();
        static PasswordList List;
        static Dictionary<string, Action> Actions = new Dictionary<string, Action>()
        {
            { "Create a keylist", CreateNewList },
            { "Unlock keylist", UnlockList }
        };

        static ManualResetEvent ActionTrigger = new ManualResetEvent(false);

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

        static void AfterUnlock()
        {
            Actions.Clear();

            Actions.Add("Retrieve password", RetrievePassword);
            Actions.Add("Generate password", GeneratePassword);
            Actions.Add("List all passwords", ListPasswords);
            Actions.Add("Add password", AddPassword);
        }

        static void AddPassword()
        {
            string name = Prompt("Enter a service name");

            while (List.Contains(name))
            {
                if (!PromptConfirm("A password with that name already exists. Overwrite?"))
                    name = Prompt("Enter a service name");
                else
                    break;
            }

            string password = "";

            while (true)
            {
                password = DiscreetPrompt("Password");
                if (DiscreetPrompt("Confirm password") != password)
                    Console.WriteLine("Confirmation doesn't match password. Try again.");
                else
                    break;
            }

            List.Add(name, password);

            Utilities.PrintColoredLine("Added password for %a{0}%7.", name);
        }

        static void InvokeAction()
        {
            string str = Console.ReadKey(true).KeyChar.ToString();
            int selection = -1;

            if (!int.TryParse(str, out selection))
                return;

            if (selection < 1 || selection > Actions.Count)
                return;

            ActionTrigger.Set();
            Console.Clear();

            var pair = GetAction(selection);
            
            Utilities.PrintColoredLine("Selected %f{0}%7.", pair.Key);

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
                Utilities.PrintColoredLine("Name: %a{0}%7, password: %c{1}", entry.Name, entry.Password);
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

            string len = Prompt("Password length", "16");
            string chars = Prompt("Characters to use", "lusd");

            Dictionary<char, Characters> mappings = new Dictionary<char, Characters>()
            {
                {'l', Characters.Letters },
                {'u', Characters.LettersUppercase },
                {'s', Characters.SymbolsBasic },
                {'d', Characters.Numbers },
                {'a', Characters.SymbolsAdvanced }
            };

            int length = int.Parse(len);
            Characters c = (Characters)0;

            foreach (var pair in mappings)
                if (chars.Contains(pair.Key))
                    c |= pair.Value;

            string password = List.Generate(name, new PasswordGenerator() { Characters = c, Length = length });

            Utilities.PrintColored("Password for %a{0}%7 is ", name);

            int pass_x = Console.CursorLeft;
            int pass_y = Console.CursorTop;

            Utilities.PrintColoredLine("%c{0}%7.", password);

            Utilities.SetClipboard(password);
            Console.WriteLine("Copied password to clipboard.");

            Console.Write("Auto-clearing in {0} seconds...", AutoClearDelay / 1000);

            int progress_x = Console.CursorLeft;
            int progress_y = Console.CursorTop;

            Console.WriteLine();

            Task.Factory.StartNew(
                delegate {
                    ActionTrigger.Reset();

                    bool result = ActionTrigger.WaitOne(AutoClearDelay);

                    if (!result)
                    {
                        int temp_x = Console.CursorLeft;
                        int temp_y = Console.CursorTop;

                        Console.SetCursorPosition(pass_x, pass_y);
                        Utilities.PrintColored("%c" + new string('█', length) + "%7");
                        Console.SetCursorPosition(progress_x, progress_y);
                        Console.Write("cleared.");
                        Console.SetCursorPosition(temp_x, temp_y);
                    }
                });

            Save();
        }

        static void RetrievePassword()
        {
            string name = Prompt("Service name");

            if (!List.Contains(name))
                Console.WriteLine("No such service.");
            else
            {
                string password = List.Retrieve(name);
                Utilities.PrintColored("Password for %a{0}%7 is ", name);

                int pass_x = Console.CursorLeft;
                int pass_y = Console.CursorTop;

                Utilities.PrintColoredLine("%c{0}%7.", password);

                Utilities.SetClipboard(password);
                Console.WriteLine("Copied password to clipboard.");

                Console.Write("Auto-clearing in {0} seconds...", AutoClearDelay / 1000);

                int progress_x = Console.CursorLeft;
                int progress_y = Console.CursorTop;

                Console.WriteLine();

                Task.Factory.StartNew(
                    delegate {
                        ActionTrigger.Reset();

                        bool result = ActionTrigger.WaitOne(AutoClearDelay);

                        if (!result)
                        {
                            int temp_x = Console.CursorLeft;
                            int temp_y = Console.CursorTop;

                            Console.SetCursorPosition(pass_x, pass_y);
                            Utilities.PrintColored("%c" + new string('█', password.Length) + "%7");
                            Console.SetCursorPosition(progress_x, progress_y);
                            Console.Write("cleared.");
                            Console.SetCursorPosition(temp_x, temp_y);
                        }
                    });
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

                string password = DiscreetPrompt("Enter password");
                byte[] salt = File.ReadAllBytes(GetAbsolutePath("salt"));

                File.WriteAllBytes(GetAbsolutePath("salt"), salt);

                Console.Write("Generating key...");
                byte[] key = CryptoBox.GenerateKeyFromPassword(password, salt, CryptoBox.DefaultKeySize);
                Key = key;
                Console.WriteLine("done.");

                Console.Write("Decrypting keylist...");
                List = CryptoBox.SafeDeserialize<PasswordList>(key, GetAbsolutePath("list"));
                Console.WriteLine("done.");

                AfterUnlock();
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

            string password = DiscreetPrompt("Enter password");
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

            AfterUnlock();
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

        static string Prompt(string prompt, string def = "")
        {
            Console.Write("{0}{1}: ", prompt, def != "" ? " [" + def + "]" : "");

            string line = Console.ReadLine();

            if (line == "" && def != "")
                return def;

            return line;
        }

        static string DiscreetPrompt(string prompt)
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
