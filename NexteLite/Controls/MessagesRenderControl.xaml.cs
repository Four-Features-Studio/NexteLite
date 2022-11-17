using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace NexteLite.Controls
{
    /// <summary>
    /// Логика взаимодействия для MessageControl.xaml
    /// </summary>
    public partial class MessagesRenderControl : UserControl
    {
        Storyboard _Show;

        Queue<Message> _ShowQueue = new Queue<Message>();

        Message _CurrentMessage;

        public MessagesRenderControl()
        {
            InitializeComponent();

            _Show = this.FindResource("Show") as Storyboard;
            if (_Show is null)
                throw new ArgumentNullException("Failed to find animation for notifications");

            _Show.Completed += _Show_Completed;
        }

        private void _Show_Completed(object? sender, EventArgs e)
        {
            DrawNext();
        }

        void DrawNext()
        {
            _CurrentMessage = null;
            Container.Children.Clear();
            if (_ShowQueue.TryDequeue(out var next))
            {
                Container.Children.Add(next);
                _Show.Begin();
            }
        }

        public void SendMessage(string message)
        {
            Container.Children.Clear();

            var messageControl = new Message();
            messageControl.MessageText = message;

            Container.Children.Add(messageControl);

            if(_CurrentMessage is null)
            {
                _CurrentMessage = messageControl;
                _Show.Begin();
            }
            else
            {
                _ShowQueue.Enqueue(messageControl);
            }
        }
    }
}
