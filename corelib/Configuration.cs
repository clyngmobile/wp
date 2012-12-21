using System;
using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;
using System.Net;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Xml.Linq;
using System.Linq;

namespace ClyngMobile
{
    internal class Configuration
    {
        private readonly IDictionary<String, Object> _settings;

        internal Configuration (IDictionary<String,Object> properties)
        {
            _settings = properties ?? new Dictionary<String, Object>();
        }

        internal T Get<T>(String key)
        {
            Object value = _settings.ContainsKey(key) ? _settings[key] : null;
            if (value == null)
            {
                return default(T);
            }

            return (T) Convert.ChangeType(value, typeof(T), null);
        }

    }
}
