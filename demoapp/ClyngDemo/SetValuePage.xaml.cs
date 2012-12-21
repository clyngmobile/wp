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
using System.Globalization;

namespace ClyngDemo
{
    public partial class SetValuePage : PhoneApplicationPage
    {
        public SetValuePage()
        {
            InitializeComponent();
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            
            try
            {
                if (textBoxStringName.Text.Length > 0 && textBoxStringValue.Text.Length > 0)
                    ClyngMobile.CMClient.Instance().setValue(textBoxStringName.Text, textBoxStringValue.Text, null/*(str) => {
                        if (String.IsNullOrEmpty(str) == false)
                            MessageBox.Show(str);
                    }*/);
                if (textBoxDoubleName.Text.Length > 0 && textBoxDoubleValue.Text.Length > 0)
                    ClyngMobile.CMClient.Instance().setValue(textBoxDoubleName.Text, Convert.ToDouble(textBoxDoubleValue.Text), null/*(str) =>
                    {
                        if (String.IsNullOrEmpty(str) == false)
                            MessageBox.Show(str);
                    }*/);

                if (textBoxBooleanName.Text.Length > 0 && textBoxBooleanValue.Text.Length > 0)
                    ClyngMobile.CMClient.Instance().setValue(textBoxBooleanName.Text, Convert.ToBoolean(textBoxBooleanValue.Text), null/*(str) =>
                    {
                        if (String.IsNullOrEmpty(str) == false)
                            MessageBox.Show(str);
                    }*/);

                if (textBoxDateName.Text.Length > 0 && textBoxDateValue.Text.Length > 0)
                {
                    DateTime dt;
                    DateTime.TryParseExact(textBoxDateValue.Text, "yyyy-MM-dd", null, DateTimeStyles.None, out dt);
                    ClyngMobile.CMClient.Instance().setValue(textBoxDateName.Text, dt, null/*(str) =>
                    {
                        if( String.IsNullOrEmpty(str) == false )
                            MessageBox.Show(str);
                    }*/);
                }
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
    }

    public struct StringPair
    {
        public string name { get; set; }
        public string value { get; set; }
    }

    struct DoublePair
    {
        string name { get; set; }
        string value { get; set; }
    }

    struct BooleanPair
    {
        string name { get; set; }
        string value { get; set; }
    }

    struct DatePair
    {
        string name { get; set; }
        string value { get; set; }
    }
}