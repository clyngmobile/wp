using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    public partial class Messages : PhoneApplicationPage, INotifyPropertyChanged
    {
        private readonly List<Message> _messages;
        private int _currentMessage;
        
        public Messages()
        {
            _messages = CMClient.Instance().LastDisplayContext.Messages;

            Message message = _messages.FirstOrDefault(item => item.Id == CMClient.Instance().LastDisplayContext.InitialMessageId);
            if(message != null)
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
            OnPropertyChanged("HasPrev");
            OnPropertyChanged("HasNext");
            OnPropertyChanged("CounterText");
            if (_messages.Count == 1 && _currentMessage + 1 == 1)
            {
                this.btnPrev.Visibility = System.Windows.Visibility.Collapsed;
                this.btnNext.Visibility = System.Windows.Visibility.Collapsed;
            }

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

        public String CounterText
        {
            get {
                if (_currentMessage + 1 == 1 && _messages.Count == 1)
                {
                    return "";
                }
                return String.Format("{0} of {1}", (_currentMessage + 1), _messages.Count); 
            }
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            ((PhoneApplicationFrame)Application.Current.RootVisual).GoBack();
        }

        private void btnNext_Click(object sender, RoutedEventArgs e)
        {
            ++_currentMessage;
            ShowMessage();
        }

        private void btnPrev_Click(object sender, RoutedEventArgs e)
        {
            --_currentMessage;
            ShowMessage();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }

        private void btnRemove_Click(object sender, RoutedEventArgs e)
        {
            CMClient.Instance().RemoveMessage(CurrentMessage);
            _messages.Remove(CurrentMessage);
            if(_messages.Count == 0)
            {
                ((PhoneApplicationFrame)Application.Current.RootVisual).GoBack();
                return;
            }

            if(_currentMessage >= _messages.Count)
            {
                --_currentMessage;
            }

            ShowMessage();
        }
    }
}