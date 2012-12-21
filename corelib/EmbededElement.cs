using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Newtonsoft.Json;

namespace ClyngMobile
{
    [JsonObject(MemberSerialization.OptIn)]
    internal class EmbededElement
    {
        public EmbededElement()
        {
        }

        [JsonProperty("disptype", Required = Required.AllowNull)]
        public String Display { get; set; }
        [JsonProperty("removeact", Required = Required.AllowNull)]
        public String RemoveAct { get; set; }
        [JsonProperty("width", Required = Required.AllowNull)]
        public int Width { get; set; }
        [JsonProperty("height", Required = Required.AllowNull)]
        public int Height { get; set; }
        [JsonProperty("clickClose", Required = Required.AllowNull)]
        public bool clickClose { get; set; }
        [JsonProperty("embedtag", Required = Required.AllowNull)]
        public String embeddedTag { get; set; }
    }
}
