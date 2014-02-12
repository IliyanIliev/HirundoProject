using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace Hirundo.Models
{
    [DataContract]
    public class UserModel
    {
        [DataMember(Name = "username")]
        public string Username { get; set; }

        [DataMember(Name = "password")]
        public string Password { get; set; }
    }
}