using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace No
{
    [Serializable]
    public class PasswordEntry
    {
        public string Password { get; set; }
        public string Name { get; set; }
        
        public PasswordEntry()
        {

        }
    }
}
