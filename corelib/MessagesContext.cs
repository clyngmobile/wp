using System;
using System.Collections.Generic;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace ClyngMobile
{
    internal class MessagesContext
    {
        public List<Message> Messages { get; set; }
        public int InitialMessageId { get; set; }
    }
}
