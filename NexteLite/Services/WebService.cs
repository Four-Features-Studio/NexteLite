using Newtonsoft.Json;
using NexteLite.Interfaces;
using NexteLite.Models;
using NexteLite.Models.Minecraft;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net.Http;
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
                Uuid = "9b15dea6606e47a4a241420251703c59",
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
                    Title = "test1",
                    Server = new Server() { Ip = "188.225.47.71", Port = 25565 }
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
