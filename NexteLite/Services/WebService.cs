using NexteLite.Interfaces;
using NexteLite.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NexteLite.Services
{
    public class WebService : IWebService
    {
        public bool Auth(string username, string password, out Profile profile, ref string message)
        {
            //message = "Неверный логин или пароль";
            //profile = null;

            //return false;

            message = string.Empty;

            profile = new Profile
            {
                Username = username,
                Uuid = "test",
                AccessToken = "test",
                ServerToken = "test",
                Avatar = "/avatar/placeholder.png"
            };
            return true;
        }

        public void GetFiles()
        {
            throw new NotImplementedException();
        }

        public List<ServerProfile> GetServerProfiles()
        {
            return new List<ServerProfile>
            {
                new ServerProfile
                {
                    NID = Guid.NewGuid().ToString(),
                    Title = "test1"
                },
                new ServerProfile
                {
                    NID = Guid.NewGuid().ToString(),
                    Title = "test2"
                },                                
                new ServerProfile
                {
                    NID = Guid.NewGuid().ToString(),
                    Title = "test3"
                }
            };
        }

        public void Logout()
        {
            throw new NotImplementedException();
        }
    }
}
