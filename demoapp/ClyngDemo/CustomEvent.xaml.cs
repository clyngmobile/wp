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
using ClyngMobile;
using Microsoft.Phone.Controls;

namespace ClyngDemo
{
    public partial class CustomEvent : PhoneApplicationPage
    {
        public String EventName { get; set; }
        public String Key1 { get; set; }
        public String Value1 { get; set; }
        public String Key2 { get; set; }
        public String Value2 { get; set; }
        public String Key3 { get; set; }
        public String Value3 { get; set; }


        public CustomEvent()
        {
            InitializeComponent();

            DataContext = this;
            //EventName = Key1 = Key2 = Key3 = Value1 = Value2 = Value3 = String.Empty;
        }

        private void Send_Click(object sender, RoutedEventArgs e)
        {
            Dictionary<String, Object> data = new Dictionary<String, Object>();
            if (!String.IsNullOrEmpty(Key1) && !String.IsNullOrEmpty(Value1))
            {
                data[Key1] = Value1;
            }
            if (!String.IsNullOrEmpty(Key2) && !String.IsNullOrEmpty(Value2))
            {
                data[Key2] = Value2;
            }
            if (!String.IsNullOrEmpty(Key3) && !String.IsNullOrEmpty(Value3))
            {
                data[Key3] = Value3;
            }


            CMClient.Instance().SendEvent(EventName, data, null);
            ((PhoneApplicationFrame) Application.Current.RootVisual).GoBack();
        }
    }
}