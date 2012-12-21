using System;
using System.IO.IsolatedStorage;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace ClyngDemo
{
    public class AppSettings
    {
        public static void Save(String key, String value)
        {
            if(IsolatedStorageSettings.ApplicationSettings.Contains(key))
            {
                IsolatedStorageSettings.ApplicationSettings.Remove(key);
            }
            
            if(value != null)
            {
                IsolatedStorageSettings.ApplicationSettings.Add(key, value);
            }

            IsolatedStorageSettings.ApplicationSettings.Save();
        }

        public static String Get(String key, String @default)
        {
            if (!IsolatedStorageSettings.ApplicationSettings.Contains(key))
            {
                return @default;
            }

            return (String)IsolatedStorageSettings.ApplicationSettings[key];
        }
    }
}
