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
using ClyngMobile;

namespace ClyngDemo
{
    public partial class ChangeUserPage : PhoneApplicationPage
    {
        public ChangeUserPage()
        {
            InitializeComponent();
        }

        partial class ChangeUserIdCallBack : CMClientListenerImpl
        {
            private string NewUserId;

            public ChangeUserIdCallBack(string newUserId)
            {
                this.NewUserId = newUserId;
            }

            void onSuccess()
            {
                CMClient.Instance().UserId = NewUserId;
                AppSettings.Save(CMClient.CmUserId, NewUserId);
            }
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string newUserId = textBoxUserId.Text;
                CMClientListenerImpl callback = new ChangeUserIdCallBack(newUserId);
                if (textBoxUserId.Text.Length > 0)
                    ClyngMobile.CMClient.Instance().changeUserId(textBoxUserId.Text, callback/*(str) => {
                        if (String.IsNullOrEmpty(str) == false)
                            MessageBox.Show(str);
                        else
                        {
                            CMClient.Instance().UserId = textBoxUserId.Text;
                            AppSettings.Save(CMClient.CmUserId, textBoxUserId.Text);
                        }
                    }*/);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }

            PhoneApplicationFrame mainFrame = ((PhoneApplicationFrame)Application.Current.RootVisual);
            mainFrame.GoBack();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            PhoneApplicationFrame mainFrame = ((PhoneApplicationFrame)Application.Current.RootVisual);
            mainFrame.GoBack();
        }

        public String NewUserId { get; set; }
    }
}