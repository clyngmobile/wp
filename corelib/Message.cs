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
    public class Message
    {
        public Message()
        {
        }

        public bool IsPush { get; set; }

        [JsonProperty("customer_id", Required = Required.AllowNull)]
        public int CustomerId { get; set; }
        [JsonProperty("display_w", Required = Required.AllowNull)]
        public int DisplayWidth { get; set; }
        [JsonProperty("display_h", Required = Required.AllowNull)]
        public int DispalyHeight { get; set; }
        [JsonProperty("name", Required = Required.AllowNull)]
        public String Name { get; set; }
        [JsonProperty("unique", Required = Required.AllowNull)]
        public bool Unique { get; set; }
        [JsonProperty("expiration", Required = Required.AllowNull)]
        public int Expiration { get; set; }
        [JsonProperty("filter", Required = Required.AllowNull)]
        public int Filter { get; set; }
        [JsonProperty("htmlMessageId", Required = Required.Always)]
        public int htmlMessageId { get; set; }
        [JsonProperty("messageId", Required = Required.Always)]
        public int messageId { get; set; }

        public int campaignId { get; set; }

        [JsonProperty("id", Required = Required.Always)]
        public int Id { get; set; }
        

        //[JsonProperty("embed_tag", Required = Required.Always)]
        //public EmbededElement EmbTag { get; set; }

        [JsonProperty("htmlMessage", Required = Required.Always)]
        public HtmlMessage htmlMessage { get; set; }

        [JsonProperty("isPhone", Required = Required.Always)]
        public Boolean isPhone { get; set; }

        [JsonProperty("isTablet", Required = Required.Always)]
        public Boolean isTablet { get; set; }

        public String Html { get; set; }
        public bool Viewed { get; set; }

        public void setCorrectHtml( String deviceType )
        {
            if (deviceType == "phone")
            {
                if (this.isPhone)
                    this.Html = htmlMessage.phoneHtml;
            }
            else
            {
                if (this.isPhone == false)
                    this.Html = htmlMessage.tabletHtml;
                else
                    this.Html = htmlMessage.phoneHtml;
            }
        }

        public void fillFromPushMessage(PushMessage m)
        {
            this.Html = m.phoneHtml;
            this.htmlMessageId = m.htmlMessageId;
            this.messageId = m.messageId;
            this.CustomerId = m.customerId;
            //this.UserId = m.userId;
            this.campaignId = m.campaignId;
            //this.pendingAdId = m.pendingAdId;
            this.IsPush = true;
            this.Filter = 1;
        }
    }

    public class HtmlMessage
    {
        [JsonProperty("customerId", Required = Required.Always)]
        public int customerId { get; set; }
        [JsonProperty("html", Required = Required.Always)]
        public String html { get; set; }
        [JsonProperty("phoneHtml", Required = Required.Always)]
        public String phoneHtml { get; set; }
        [JsonProperty("tabletHtml", Required = Required.Always)]
        public String tabletHtml { get; set; }
    }

    [JsonObject(MemberSerialization.OptIn)]
    public class PushMessage
    {
        [JsonProperty("htmlMessageId", Required = Required.Always)]
        public int htmlMessageId { get; set; }
        [JsonProperty("messageId", Required = Required.Always)]
        public int messageId { get; set; }
        [JsonProperty("customerId", Required = Required.Always)]
        public int customerId { get; set; }

        [JsonProperty("userId", Required = Required.Always)]
        public int userId { get; set; }

        [JsonProperty("campaignId", Required = Required.Always)]
        public int campaignId { get; set; }
        [JsonProperty("html", Required = Required.Always)]
        public String html { get; set; }

        [JsonProperty("phoneHtml", Required = Required.Always)]
        public String phoneHtml { get; set; }

        [JsonProperty("pendingAdId", Required = Required.Always)]
        public int pendingAdId { get; set; }

        [JsonProperty("tabletHtml", Required = Required.Always)]
        public String tabletHtml { get; set; }

        [JsonProperty("adId", Required = Required.Always)]
        public int adId { get; set; }

        [JsonProperty("id", Required = Required.Always)]
        public int id { get; set; }
    }
}
