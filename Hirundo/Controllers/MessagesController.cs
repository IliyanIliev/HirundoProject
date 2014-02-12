using Hirundo.Models;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using MongoDB.Driver.Builders;
using MongoDB.Driver.Linq;
using MongoDB.Bson;

namespace Hirundo.Controllers
{
    public class MessagesController : ApiController
    {
        readonly MongoDatabase mongoDatabase;

        public MessagesController()
        {
            mongoDatabase = RetreiveMongohqDb();
        }

        private MongoDatabase RetreiveMongohqDb()
        {
            MongoClient mongoClient = new MongoClient(
                new MongoUrl(ConfigurationManager.ConnectionStrings
                 ["MongoHQ"].ConnectionString));
            MongoServer server = mongoClient.GetServer();
            return mongoClient.GetServer().GetDatabase("HirundoDb");
        }

        [ActionName("write")]
        [HttpPost]
        public void WriteMessage(Message msg, string sessionKey)
        {

            var msgList = mongoDatabase.GetCollection("Messages");
            WriteConcernResult result;
            bool hasError = false;
            string username = checkSessionKey(sessionKey);
            if (username == "") { throw new InvalidOperationException("You are not logged in!"); }

            if (string.IsNullOrEmpty(msg.Id))
            {
                msg.Id = ObjectId.GenerateNewId().ToString();
                msg.Author = username;
                msg.Date = (DateTime.Now.Subtract(new DateTime(1970, 1, 1))).TotalSeconds.ToString();
                result = msgList.Insert<Message>(msg);
                hasError = result.HasLastErrorMessage;
            }
            else
            {
                throw new InvalidOperationException("Users exists");
            }

            if (hasError)
            {
                throw new HttpResponseException(HttpStatusCode.InternalServerError);
            }
        }

        public string checkSessionKey(string sessionKey)
        {
            string username = "";
            var usersList = mongoDatabase.GetCollection<User>("Users");
            var query = usersList.AsQueryable<User>().Where(u => u.SessionKey == sessionKey);
            foreach (var user in query)
            {
                username = user.Username;
            }
            return username;
        }


    }
}
