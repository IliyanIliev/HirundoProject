using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Hirundo.Models
{
    public class Message
    {
        [BsonId]
        public string Id { get; set; }

        public string Content { get; set; }

        public string Author { get; set; }

        public string Place { get; set; }

        public string Date { get; set; }
    }
}