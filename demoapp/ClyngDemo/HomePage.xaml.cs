using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using ClyngMobile;
using Coding4Fun.Phone.Controls;
using Microsoft.Phone.Controls;

namespace ClyngDemo
{
    public partial class MainPage : PhoneApplicationPage
    {
        private Timer _waitForTokenTimer;

        public bool Fullscreen
        {
            get { return CMClient.Instance().Fullscreen; }
            set { CMClient.Instance().Fullscreen = value; }
        }

        // Constructor
        public MainPage()
        {
            InitializeComponent();

            txtServer.Text = CMClient.Instance().ServerUrl;

            _waitForTokenTimer = new Timer(state => Dispatcher.BeginInvoke(UpdateDeviceToken), null, 0, 1000);

            DataContext = this;
        }

        private void UpdateDeviceToken()
        {
            String token = CMClient.Instance().Token;
            if(!String.IsNullOrEmpty(token))
            {
                if(_waitForTokenTimer != null)
                {
                    _waitForTokenTimer.Dispose();
                    _waitForTokenTimer = null;
                }
            }
        }

        private void ApplyServer_Click(object sender, RoutedEventArgs e)
        {
            CMClient.Instance().ServerUrl = txtServer.Text;
            CMClient.Instance().RegisterUser(null);
            AppSettings.Save(CMClient.CmRgServerUrl, CMClient.Instance().ServerUrl);
        }

        private void SignIn_Click(object sender, RoutedEventArgs e)
        {
            CMClient.Instance().SendEvent("sign-in", null, null);
        }

        private void SignOut_Click(object sender, RoutedEventArgs e)
        {
            CMClient.Instance().SendEvent("sign-out", null, null);
        }

        private void Share_Click(object sender, RoutedEventArgs e)
        {
            InputPrompt prompt = new InputPrompt();
            prompt.Title = "Share with";
            prompt.Show();
            prompt.Completed += (s, args) =>
                {
                    if(args.PopUpResult == PopUpResult.Ok)
                    {
                        Dictionary<String,Object> data = new Dictionary<String, Object>();
                        data.Add("shared-with", args.Result);
                        CMClient.Instance().SendEvent("share", data, null);
                    }
                };
        }

        private void Custom_Click(object sender, RoutedEventArgs e)
        {
            PhoneApplicationFrame mainFrame = ((PhoneApplicationFrame)Application.Current.RootVisual);
            mainFrame.Navigate(new Uri("/CustomEvent.xaml", UriKind.Relative));
        }

        private void Pending_Click(object sender, RoutedEventArgs e)
        {
            CMClient.Instance().GetPendingMessages(null);
        }

        private void SetValue_Click(object sender, RoutedEventArgs e)
        {
            PhoneApplicationFrame mainFrame = ((PhoneApplicationFrame)Application.Current.RootVisual);
            mainFrame.Navigate(new Uri("/SetValuePage.xaml", UriKind.Relative));
        }

        private void ChangeUser_Click(object sender, RoutedEventArgs e)
        {
            PhoneApplicationFrame mainFrame = ((PhoneApplicationFrame)Application.Current.RootVisual);
            mainFrame.Navigate(new Uri("/ChangeUserPage.xaml", UriKind.Relative));
        }

        private void Unregister_Click(object sender, RoutedEventArgs e)
        {
            CMClient.Instance().UnregisterUser(null/*(data, ex) => { Handle Exception here System.Diagnostics.Debug.WriteLine("Unregister_Click event: {0}", ex); }*/);
        }
    }
}