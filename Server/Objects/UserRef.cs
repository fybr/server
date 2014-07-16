using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fybr.Server.Objects
{
    public class Credentials
    {
        public string Email { get; set; }
        public string Password { get; set; }

        public Credentials()
        {
            
        }
    }

    public class UserRef : Credentials
    {
        public UserRef()
        {
            
        }

        public UserRef(Credentials credentials)
            : this(credentials.Email, credentials.Password)
        {
            
        }

        public UserRef(string email, string password)
        {
            Email = email;
            Password = password;
            User = Guid.NewGuid().ToString();
        }

        public string User { get; set; }
    }
}
