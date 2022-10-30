using NexteLite.Interfaces;
using NexteLite.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NexteLite.Services
{
    public class MinecraftService : IMinecraftService
    {
       
        public async Task Play(Profile profile)
        {
            await StartMinecraft();
        }

        private async Task StartMinecraft()
        {

        }

        private void GetArgs()
        {

        }
        private void GetJvm()
        {

        }
    }
}
