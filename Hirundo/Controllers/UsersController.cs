using Hirundo.Models;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using MongoDB.Driver.Linq;
using MongoDB.Bson;
using System.Text;
using System.Web.Security;
using MongoDB.Driver.Builders;

namespace Hirundo.Controllers
{
    public class UsersController : ApiController
    {
        private const int MinUsernameLength = 6;
        private const int MaxUsernameLength = 30;

        private const string ValidUsernameCharacters =
            "qwertyuioplkjhgfdsazxcvbnmQWERTYUIOPLKJHGFDSAZXCVBNM1234567890_.";

        private const string SessionKeyChars =
            "qwertyuioplkjhgfdsazxcvbnmQWERTYUIOPLKJHGFDSAZXCVBNM";
        private static readonly Random rand = new Random();

        private const int SessionKeyLength = 50;

        private const int Sha1Length = 40;

        readonly MongoDatabase mongoDatabase;

        public UsersController()
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

        public bool checkUsername(string username)
        {
            bool hasSuchUser = false;
            var usersList = mongoDatabase.GetCollection<User>("Users");
            var query = usersList.AsQueryable<User>().Where(u => u.Username == username);
            foreach (var user in query)
            {
                hasSuchUser = true;
            }
            return hasSuchUser;
        }

        public User checkUserAndPass(string username,string password)
        {
            var returnedUser=new User();
            bool hasSuchUser = false;
            var usersList = mongoDatabase.GetCollection<User>("Users");
            string sha1Pass = FormsAuthentication.HashPasswordForStoringInConfigFile(password, "SHA1");
            var query = usersList.AsQueryable<User>().Where(u => u.Username == username && u.Password==sha1Pass);
            foreach (var user in query)
            {
                returnedUser = user;
            }
            return returnedUser;
        }

        [ActionName("register")]
        public User Register(User user)
        {
            var usersList = mongoDatabase.GetCollection("Users");
            WriteConcernResult result;
            bool hasError = false;
            if (string.IsNullOrEmpty(user.Id))
            {
                if (checkUsername(user.Username) == false)
                {
                    user.Id = ObjectId.GenerateNewId().ToString();
                    string password = FormsAuthentication.HashPasswordForStoringInConfigFile(user.Password, "SHA1");
                    user.Password = password;
                    user.RegisterDate = (DateTime.Now.Subtract(new DateTime(1970, 1, 1))).TotalSeconds.ToString();
                    result = usersList.Insert<User>(user);
                    hasError = result.HasLastErrorMessage;
                }
                else
                {
                    throw new InvalidOperationException("Users exists");
                }
            }
            if (!hasError)
            {
                return user;
            }
            else
            {
                throw new HttpResponseException(HttpStatusCode.InternalServerError);
            }
        }

        [ActionName("login")]
        public LoggedUserModel Login(UserModel user)
        {
            var usersList = mongoDatabase.GetCollection("Users");
            var returnedUser = checkUserAndPass(user.Username,user.Password);
            if (!String.IsNullOrEmpty(returnedUser.Username))
            {
                if (returnedUser.SessionKey == null)
                {
                    returnedUser.SessionKey = this.GenerateSessionKey(returnedUser.Id);
                }
                IMongoQuery query = Query.EQ("_id", returnedUser.Id);
                IMongoUpdate update = Update
                .Set("SessionKey", returnedUser.SessionKey);
                var result = usersList.Update(query, update);
                return new LoggedUserModel { Username = returnedUser.Username, SessionKey = returnedUser.SessionKey };
            }
            else
            {
                throw new InvalidOperationException("Invalid username or password");
            }
        }

        private void ValidateUsername(string username)
        {
            if (username == null)
            {
                throw new ArgumentNullException("Username cannot be null");
            }
            else if (username.Length < MinUsernameLength)
            {
                throw new ArgumentOutOfRangeException(
                    string.Format("Username must be at least {0} characters long",
                    MinUsernameLength));
            }
            else if (username.Length > MaxUsernameLength)
            {
                throw new ArgumentOutOfRangeException(
                    string.Format("Username must be less than {0} characters long",
                    MaxUsernameLength));
            }
            else if (username.Any(ch => !ValidUsernameCharacters.Contains(ch)))
            {
                throw new ArgumentOutOfRangeException(
                    "Username must contain only Latin letters, digits .,_");
            }
        }

        private string GenerateSessionKey(string userId)
        {
            StringBuilder skeyBuilder = new StringBuilder(SessionKeyLength);
            skeyBuilder.Append(userId);
            while (skeyBuilder.Length < SessionKeyLength)
            {
                var index = rand.Next(SessionKeyChars.Length);
                skeyBuilder.Append(SessionKeyChars[index]);
            }
            return skeyBuilder.ToString();
        }

        private void ValidateAuthCode(string authCode)
        {
            if (authCode == null || authCode.Length != Sha1Length)
            {
                throw new ArgumentOutOfRangeException("Password should be encrypted");
            }
        }
    }
}
