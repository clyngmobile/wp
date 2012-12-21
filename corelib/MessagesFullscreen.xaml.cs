using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;

namespace ClyngMobile
{
    public partial class MessagesFullscreen : PhoneApplicationPage
    {
        private readonly List<Message> _messages;
        private int _currentMessage;

        public MessagesFullscreen()
        {
            _messages = CMClient.Instance().LastDisplayContext.Messages;

            Message message = _messages.FirstOrDefault(item => item.Id == CMClient.Instance().LastDisplayContext.InitialMessageId);
            if (message != null)
            {
                _currentMessage = _messages.IndexOf(message);
            }

            DataContext = this;
            InitializeComponent();

            ShowMessage();
        }

        public Message CurrentMessage
        {
            get { return _messages[_currentMessage]; }
        }

        private void ShowMessage()
        {
            webBrowser.Base = CMClient.Instance().ServerUrl;
            webBrowser.NavigateToString(EncondigUtil.ConvertExtendedAscii(CurrentMessage.Html));

            CMClient.Instance().NotifyMessageOpened(CurrentMessage);
        }

        public bool HasPrev
        {
            get { return _currentMessage > 0; }
        }

        public bool HasNext
        {
            get { return _currentMessage < _messages.Count() - 1; }
        }

        private void btnRemove_Click(object sender, RoutedEventArgs e)
        {
            CMClient.Instance().RemoveMessage(CurrentMessage);
            _messages.Remove(CurrentMessage);
            if (_messages.Count == 0)
            {
                ((PhoneApplicationFrame)Application.Current.RootVisual).GoBack();
                return;
            }

            if (_currentMessage >= _messages.Count)
            {
                --_currentMessage;
            }

            ShowMessage();
        }

        private void GestureListener_Flick(object sender, FlickGestureEventArgs e)
        {
            if(e.Direction == System.Windows.Controls.Orientation.Horizontal)
            {
                if(e.HorizontalVelocity < -800 && HasNext)
                {
                    ++_currentMessage;
                    ShowMessage();
                }

                if (e.HorizontalVelocity > 800 && HasPrev)
                {
                    --_currentMessage;
                    ShowMessage();
                }
            }
        }
    }
}