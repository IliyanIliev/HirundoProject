using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Hirundo.Models
{
    public class User
    {
        [BsonId]
        public string Id { get; set; }

        public string Username { get; set; }

        public string Email { get; set; }

        public string Password { get; set; }

        public byte Verified { get; set; }

        public string SessionKey { get; set; }

        public string RegisterDate { get; set; }

        public virtual ICollection<User> Following { get; set; }

        public User()
        {
            Following = new HashSet<User>();
        }
    }
}