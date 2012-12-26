using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using ClyngMobile;
using Microsoft.Phone.Controls;

namespace ClyngDemo
{
    public partial class Login : PhoneApplicationPage
    {
        public Login()
        {
            InitializeComponent();

            CMClient.Init(new Dictionary<String, Object>()
                              {
                                  { CMClient.CmCustomerKey, AppSettings.Get(CMClient.CmCustomerKey, "your private apiKey here") },
                                  { CMClient.CmRgServerUrl, AppSettings.Get(CMClient.CmRgServerUrl, "https://go.clyng.com") },  
                                  { CMClient.CmUserId, AppSettings.Get(CMClient.CmUserId, "mr@smith.com")},
                                  { CMClient.CmUseGpsLocation, true}
                              });
            ClyngMobile.CMClientListener clientListener = new CMClientListenerImpl();
            ClyngMobile.CMClient.Instance().setCMClientListener(clientListener);

            DataContext = this;
        }

        private void Login_Click(object sender, RoutedEventArgs e)
        {
            CMClient.Instance().RegisterUser(null);
            PhoneApplicationFrame mainFrame = ((PhoneApplicationFrame)Application.Current.RootVisual);
            mainFrame.Navigate(new Uri("/HomePage.xaml", UriKind.Relative));
        }

        public String Username
        {
            get { return CMClient.Instance().UserId; }
            set
            {
                CMClient.Instance().UserId = value;
                AppSettings.Save(CMClient.CmUserId, value);
            }
        }

        public String Password { get; set; }

        public String Serial
        {
            get { return CMClient.Instance().ApiKey; }
            set
            {
                CMClient.Instance().ApiKey = value;
                AppSettings.Save(CMClient.CmCustomerKey, value);
            }
        }

        public String Version
        {
            get
            {
                Assembly assembly = AppDomain.CurrentDomain.GetAssemblies().First(item => item.FullName.Contains("ClyngDemo"));
                AssemblyName assemblyName = new AssemblyName(assembly.FullName);
                return "Version: " + assemblyName.Version;
            }
        }
    }
}
