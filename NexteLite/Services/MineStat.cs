using NexteLite.Interfaces;
using NexteLite.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Documents;
using Time = System.Timers.Timer;

namespace NexteLite.Services
{
    public class MineStat : IMineStat
    {
        const ushort dataSize = 512;
        const ushort numFields = 6;
        const int defaultTimeout = 5;

        public string Address { get; set; }
        public ushort Port { get; set; }
        public int Timeout { get; set; }
        public string Motd { get; set; }
        public string Version { get; set; }
        public string CurrentPlayers { get; set; }
        public string MaximumPlayers { get; set; }
        public bool ServerUp { get; set; }
        public long Latency { get; set; }

        ConcurrentBag<IMineStatSubscriber> subscribers = new ConcurrentBag<IMineStatSubscriber>();

        Time timer;
        public MineStat()
        {
            timer = new Time();
            timer.AutoReset = true;
            timer.Interval = (5 * 60) * 1000; // min * seconds * ms = total ms
            timer.Elapsed += Timer_Elapsed;
        }

        private async void Timer_Elapsed(object? sender, ElapsedEventArgs e)
        {
            foreach (var subscriber in subscribers)
            {
                var state = await CheckOnline(subscriber.Ip, subscriber.Port).ConfigureAwait(true);
                subscriber.UpdateInfo(state);
            }
        }

        public async Task<ServerState> CheckOnline(string ip, int port)
        {
            var rawServerData = new byte[dataSize];

            if (string.IsNullOrEmpty(ip))
                return null;

            Address = ip;
            Port = port == 0? (ushort)25565 : (ushort)port;
            Timeout = 5 * 1000;

            try
            {
                var stopWatch = new Stopwatch();
                var tcpclient = new TcpClient();
                tcpclient.ReceiveTimeout = Timeout;
                stopWatch.Start();
                tcpclient.Connect(Address, Port);
                stopWatch.Stop();
                Latency = stopWatch.ElapsedMilliseconds;
                var stream = tcpclient.GetStream();
                var payload = new byte[] { 0xFE, 0x01 };
                await stream.WriteAsync(payload, 0, payload.Length);
                await stream.ReadAsync(rawServerData, 0, dataSize);
                tcpclient.Close();
            }
            catch (Exception)
            {
                ServerUp = false;
                return null;
            }

            if (rawServerData == null || rawServerData.Length == 0)
            {
                ServerUp = false;
            }
            else
            {
                var serverData = Encoding.Unicode.GetString(rawServerData).Split("\u0000\u0000\u0000".ToCharArray());
                if (serverData != null && serverData.Length >= numFields)
                {
                    ServerUp = true;
                    Version = serverData[2];
                    Motd = serverData[3];
                    CurrentPlayers = serverData[4];
                    MaximumPlayers = serverData[5];
                }
                else
                {
                    ServerUp = false;
                }
            }

            
            if(int.TryParse(CurrentPlayers, out var players) && 
            int.TryParse(MaximumPlayers, out var maxplayers))
            {
                return new ServerState(maxplayers, players, ServerUp);
            }

            return null;
        }

        public async void Subscribe(IMineStatSubscriber subscriber)
        {
            if(!timer.Enabled)
                timer.Start();

            subscribers.Add(subscriber);

            var state = await CheckOnline(subscriber.Ip, subscriber.Port).ConfigureAwait(true);
            subscriber.UpdateInfo(state);

        }

    }
}
