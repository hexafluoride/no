using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace No
{
    [Serializable]
    public class PasswordList
    {
        public List<PasswordEntry> Passwords = new List<PasswordEntry>();

        public PasswordList()
        {

        }

        public void Add(string name, string password)
        {
            if (Passwords.Any(p => p.Name == name))
                throw new Exception("Password with same name already exists");

            Passwords.Add(new PasswordEntry() { Name = name, Password = password });
        }

        public string Generate(string name, PasswordGenerator generator = null)
        {
            if (generator == null)
                generator = new PasswordGenerator();

            Add(name, generator.Generate());

            return Retrieve(name);
        }

        public string Retrieve(string name)
        {
            if (!Passwords.Any(p => p.Name == name))
                return "";

            return Passwords.First(p => p.Name == name).Password;
        }
    }
}
