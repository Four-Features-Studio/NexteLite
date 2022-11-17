using Microsoft.Extensions.Logging;
using NexteLite.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NexteLite.Services
{
    public class MessageService : IMessageService
    {
        IMainWindow _MainWindow;
        public MessageService(IMainWindow mainWindow)
        {
            _MainWindow = mainWindow;
        }

        public void SendInfo(string message)
        {
            _MainWindow.SendMessage(message);
        }
    }
}
