using System;
using System.Collections.Generic;
using System.Device.Location;
using System.Diagnostics;
using System.IO;
using System.IO.IsolatedStorage;
using System.Net;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Linq;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Notification;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ClyngMobile
{
    public class CMClient
    {
        public const String CmRgServerUrl = "cmRgServerUrl";
        public const String CmCustomerKey = "cmCustomerKey";
        public const String CmUseGpsLocation = "cmUseGpsLocation";
        public const String CmUserId = "cmUserId";
        public const String CmEmail = "cmEmail";
        public const String CmLocale = "cmLocale";

        public const String ApiKey_Key = "apiKey";
        public const String UserId_Key = "userId";
        public const String Identifier_Key = "identifier";
        public const String HTMLMessageId_Key = "htmlMessageId";
        public const String MessageId_Key = "messageId";
        public const String CampaignId_Key = "campaignId";
        public const String EventName_Key = "eventName";
        public const String Email_Key = "email";
        public const String Locale_Key = "locale";
        public const String MobileDeviceToken_Key = "mobileDeviceToken";
        public const String MobileDevicePlatform_Key = "mobileDevicePlatform";
        public const String MobileDeviceType_Key = "mobileDeviceType";
        public const String Latitude_Key = "latitude";
        public const String Longitude_Key = "longitude";
        public const String Name_Key = "name";
        public const String Value_Key = "value";
        public const String NewUserId_Key = "newUserId";


        private static SynchronizationContext _dispatcher;
        private static readonly Object _lock = new Object();

        private static CMClient _instance { get; set; }
        private CMClientListener listener;

        private static readonly String PushChannelName = "ClyngPushChnl";
        private static readonly int MessageFilter = 1;

        /// <summary>
        /// Init library with empty settings. Can be called only once
        /// </summary>
        public static void Init()
        {
            Init(new Configuration(null));
        }

        /*public static void Init(String configName)
        {
            try
            {
                Init(Configuration.FromFile(DefaultConfigName));
            }
            catch (IOException)
            {
                Init(new Dictionary<String, Object>());
            }
        }*/

        /// <summary>
        /// Init library with settings. Can be called only once
        /// </summary>
        /// <param name="properties">settings</param>
        public static void Init(IDictionary<string, object> properties)
        {
            Init(new Configuration(properties));
        }

        /// <summary>
        /// Get instance of client. Init should be called first
        /// </summary>
        /// <returns></returns>
        public static CMClient Instance()
        {
            if(_instance == null)
            {
                throw new InvalidOperationException("Init method should be called first");
            }
            return _instance;
        }

        /// <summary>
        /// Private. Init library
        /// </summary>
        /// <param name="configuration"></param>
        private static void Init(Configuration configuration)
        {
            lock (_lock)
            {
                if (_instance != null)
                {
                    throw new InvalidOperationException("CMClient already initialized");
                }

                _instance = new CMClient();
                _instance.ServerUrl = configuration.Get<String>(CmRgServerUrl);
                _instance.ApiKey = configuration.Get<String>(CmCustomerKey);
                _instance.UserId = configuration.Get<String>(CmUserId);
                _instance.Email = configuration.Get<String>(CmEmail);
                _instance.Locale = configuration.Get<String>(CmLocale);
                _instance.UseGps = configuration.Get<Boolean>(CmUseGpsLocation);
                _dispatcher = SynchronizationContext.Current;
                _instance.GetPushedMessages(new CMClientListenerImpl());
            }
        }

        class CMClientListenerImpl : CMClientListener
        {
            void CMClientListener.onError(Exception error) { }
            void CMClientListener.onSuccess() { }
        }

        /// <summary>
        /// Server URL
        /// </summary>
        public String ServerUrl { get; set; }
        /// <summary>
        /// Customer Key
        /// </summary>
        public String ApiKey { get; set; }
        /// <summary>
        /// UserId
        /// </summary>
        public String UserId { get; set; }
        /// <summary>
        /// User email
        /// </summary>
        public String Email { get; set; }
        /// <summary>
        /// Device PUSH token
        /// </summary>
        public string Token { get; private set; }
        /// <summary>
        /// Viewer mode
        /// </summary>
        public bool Fullscreen { get; set; }

        /// <summary>
        /// Device Platform
        /// </summary>
        public String DevicePlatform
        {
            get { return "WP"; }
        }

        /// <summary>
        /// Device type
        /// </summary>
        public String DeviceType
        {
            get { return "phone"; }
        }

        private String _locale;
        /// <summary>
        /// User locale
        /// </summary>
        private String Locale
        {
            get { return _locale ?? Thread.CurrentThread.CurrentUICulture.TwoLetterISOLanguageName; }
            set { _locale = value; }
        }

        private bool _useGps;
        /// <summary>
        /// Use GPS to detect device location
        /// </summary>
        public bool UseGps
        {
            get { return _useGps; }
            set
            {
                _useGps = value;
                if (value)
                {
                    StartDetermineLocation();
                }
                else
                {
                    StopDetermineLocation();
                }

            }
        }

        /// <summary>
        /// Use this locations to send to server if UseGps = false
        /// </summary>
        public GeoCoordinate Coordinates { get; set; }
        private GeoCoordinate _detectedCoordinates;

        private GeoCoordinateWatcher _geoWatcher;
        private Timer _geoWatcherTimer;

        internal MessagesContext LastDisplayContext { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        private CMClient()
        {
            HttpNotificationChannel pushChannel = HttpNotificationChannel.Find(PushChannelName);
            if(pushChannel == null)
            {
                pushChannel = new HttpNotificationChannel(PushChannelName);
                pushChannel.ChannelUriUpdated += (sender, e) => Token = (e.ChannelUri.ToString());
                pushChannel.ShellToastNotificationReceived += ((sender,e) => HandleHttpNotification(e));
                pushChannel.Open();
                pushChannel.BindToShellToast();
            }
            else
            {
                Token = pushChannel.ChannelUri != null ? pushChannel.ChannelUri.ToString() : null;
                pushChannel.ChannelUriUpdated += (sender, e) => Token = (e.ChannelUri.ToString());
                pushChannel.ShellToastNotificationReceived += ((sender, e) => HandleHttpNotification(e));
            }
        }

        public void setCMClientListener(CMClientListener clientListener)
        {
            listener = clientListener;
        }

        /// <summary>
        /// Try to determine device locations
        /// </summary>
        private void StartDetermineLocation()
        {
            if(_geoWatcher == null)
            {
                _geoWatcher = new GeoCoordinateWatcher(GeoPositionAccuracy.Default);
                _geoWatcher.PositionChanged += (sender, e) => GeoWatcher_PositionChanged(e);
            }

            if(_geoWatcherTimer != null)
            {
                _geoWatcherTimer.Dispose();
            }

            _geoWatcher.Start();
            //stop geowatcher after 10 seconds
            _geoWatcherTimer = new Timer(s1 => _dispatcher.Send(s2 => StopDetermineLocation(), null), null, 10 * 1000, Timeout.Infinite);
        }

        /// <summary>
        /// PositionChanged listener
        /// </summary>
        /// <param name="e"></param>
        private void GeoWatcher_PositionChanged(GeoPositionChangedEventArgs<GeoCoordinate> e)
        {
            if(e.Position != null && e.Position.Location != null)
            {
                _detectedCoordinates = e.Position.Location;
            }
        }
        
        /// <summary>
        /// Stop determine locations
        /// </summary>
        private void StopDetermineLocation()
        {
            if(_geoWatcherTimer != null)
            {
                _geoWatcherTimer.Dispose();
                _geoWatcherTimer = null;
            }

            if(_geoWatcher != null)
            {
                _geoWatcher.Stop();
            }
        }

        /// <summary>
        /// Handle PUSH notification
        /// </summary>
        /// <param name="httpNotification"></param>
        private void HandleHttpNotification(NotificationEventArgs httpNotification)
        {
            if(!httpNotification.Collection.ContainsKey("wp:htmlMessageId"))
            {
                return;
            }

            int htmlMessageId_ = Int32.Parse(httpNotification.Collection["wp:htmlMessageId"]);
            int messageId_ = Int32.Parse(httpNotification.Collection["wp:messageId"]);
            int campaignId_ = Int32.Parse(httpNotification.Collection["wp:campaignId"]);
            GetMessagesHtml(new[] { new Message { messageId = messageId_, htmlMessageId = htmlMessageId_, campaignId = campaignId_, IsPush = true } }, 0, messages =>
                {
                    if (!String.IsNullOrWhiteSpace(messages[0].Html))
                    {
                        DisplayMessages(messages.ToList(), messageId_);    
                    }
                });
        }


        /*public void RegisterUser()
        {
            RegisterUser((data, ex) => { });
        }*/

        /*public void UnregisterUser()
        {
            UnregisterUser((data, ex) => { });
        }*/


        private void sendErrorResponce(CMClientListener clientListener, Exception error) {
            if(clientListener != null) {
                clientListener.onError(error);
            } else if(listener != null) {
                listener.onError(error);
            }
        }

        private void sendSuccessResponce(CMClientListener clientListener)
        {
            if (clientListener != null)
            {
                clientListener.onSuccess();
            }
            else if (listener != null)
            {
                listener.onSuccess();
            }
        }

        private void sendResponse(CMClientListener clientListener, Exception error)
        {
            if (error != null)
            {
                sendErrorResponce(clientListener, error);
            }
            else
            {
                sendSuccessResponce(clientListener);
            }
        }

        /// <summary>
        /// Call server to check pending messages
        /// </summary>
        public void GetPendingMessages(/*Action<Exception> callback*/CMClientListener clientListener)
        {
            GetPendingMessages((data, ex) => 
                {
                    sendResponse(clientListener, ex);
                    if (data != null)
                    {
                        IEnumerable<Message> messages = data.Where(item => item.Filter != MessageFilter && !String.IsNullOrWhiteSpace(item.Html));
                        DisplayMessages(messages.ToList(), 0);
                    }
                    /*else if (ex != null)
                    {
                        //callback(ex);
                    }*/
                });
        }

        /// <summary>
        /// Remove message
        /// </summary>
        /// <param name="messageId"></param>
        internal void RemoveMessage(Message message)
        {
            RemoveMessage(message, null);
        }

        /// <summary>
        /// notify that message was shown to user
        /// </summary>
        /// <param name="messageId"></param>
        internal void NotifyMessageOpened(Message message)
        {
            NotifyMessageOpened(message, null);
        }

        /// <summary>
        /// Send event to server
        /// </summary>
        /// <param name="eventName">Event name</param>
        /// <param name="params"></param>
        public void SendEvent(String eventName, Dictionary<string, object> @params, CMClientListener clientListener)
        {
            if(UseGps)
            {
                StartDetermineLocation();
            }
            SendEvent(eventName, @params, (data, ex) =>
                {
                    sendResponse(clientListener, ex);
                });
        }

        /// <summary>
        /// Call server to check pending messages
        /// </summary>
        public void GetPushedMessages(/*Action<Exception> callback*/CMClientListener clientListener)
        {
            GetPushedMessages((data, ex) =>
            {
                sendResponse(clientListener, ex);
                if (data != null)
                {
                    IEnumerable<Message> messages = data.Where(item => item.Filter == MessageFilter && !String.IsNullOrWhiteSpace(item.Html));
                    DisplayMessages(messages.ToList(), 0);
                }
                /*else if(ex != null)
                {
                    callback(ex);
                }*/
            });
        }


        private void GetPushedMessages(Action<Message[], Exception> callback)
        {

            Dictionary<string, object> data = new Dictionary<string, object>();
            data.Add(ApiKey_Key, ApiKey);
            data.Add(UserId_Key, UserId);

            ExecuteRequest("rulegrid/mobile/message/getPushedMessages", data, (result, ex) =>
            {
                if (result != null)
                {
                    try
                    {
                        PushMessage[] pushMessages = JsonConvert.DeserializeObject<PushMessage[]>(result);
                        Message[] messages = new Message[pushMessages.Length];

                        for (int i = 0; i < pushMessages.Length; i++)
                        {
                            messages[i] = new Message();
                            messages[i].fillFromPushMessage(pushMessages[i]);
                        }
                        callback(messages, null);
                    }
                    catch (JsonSerializationException e)
                    {
                        callback(null, e);
                    }
                    
                    //  GetMessagesHtml(messages, 0, callback);
                }
                else
                {
                    callback(null, ex);
                }
                
            });
        }

        /// <summary>
        /// Show messages screen
        /// </summary>
        /// <param name="messages"></param>
        /// <param name="messageId"></param>
        private void DisplayMessages(List<Message> messages, int messageId)
        {
            if (messages.Count == 0)
            {
                return;
            }

            LastDisplayContext = new MessagesContext()
                                     {
                                         Messages = messages,
                                         InitialMessageId = messageId,
                                     };

            PhoneApplicationFrame mainFrame = ((PhoneApplicationFrame)Application.Current.RootVisual);

            mainFrame.Navigate(Fullscreen
                                   ? new Uri("/ClyngMobile;component/MessagesFullscreen.xaml", UriKind.Relative)
                                   : new Uri("/ClyngMobile;component/Messages.xaml", UriKind.Relative));
        }

        //http communication

        /// <summary>
        /// Call server to register user
        /// </summary>
        /// <param name="callback"></param>
        public void RegisterUser(/*Action<string, Exception> callback*/CMClientListener clientListener)
        {
            Dictionary<string, object> data = new Dictionary<string, object>();
            data.Add(ApiKey_Key, ApiKey);
            data.Add(UserId_Key, UserId);
            data.Add(Identifier_Key, Token);

            ExecuteRequest("rulegrid/mobile/device/registerWindowsPhone", data, (str, ex) => 
                {
                    sendResponse(clientListener, ex);
                });
        }

        /// <summary>
        /// Call server to unregister user
        /// </summary>
        /// <param name="callback"></param>
        public void UnregisterUser(/*Action<string, Exception> callback*/ CMClientListener clientListener)
        {
            Dictionary<string, object> data = new Dictionary<string, object>();
            data.Add(ApiKey_Key, ApiKey);
            data.Add(UserId_Key, UserId);
            data.Add(Identifier_Key, Token);

            ExecuteRequest("rulegrid/mobile/device/unregisterWindows", data, (str, ex) =>
            {
                sendResponse(clientListener, ex);
            });
        }

        /// <summary>
        /// Get pending messages from server
        /// </summary>
        /// <param name="callback"></param>
        private void GetPendingMessages(Action<Message[], Exception> callback)
        {
            Dictionary<string, object> data = new Dictionary<string, object>();
            data.Add(ApiKey_Key, ApiKey);
            data.Add(UserId_Key, UserId);

            ExecuteRequest("rulegrid/mobile/message/getMessages", data, (result, ex) =>
                {
                    if (result != null)
                    {
                        try
                        {
                            Message[] messages = JsonConvert.DeserializeObject<Message[]>(result);
                            foreach (Message message in messages)
                            {
                                message.setCorrectHtml(this.DeviceType);
                            }

                            callback(messages, null);
                            //  GetMessagesHtml(messages, 0, callback);
                        }
                        catch (JsonSerializationException e)
                        {
                            callback(null, e);
                        }
                        
                    }
                    else if(ex != null)
                    {
                        callback(null, ex);
                    }
                });
        }

        /// <summary>
        /// Get html body for all messages with recursion
        /// </summary>
        /// <param name="messages"></param>
        /// <param name="index"></param>
        /// <param name="callback"></param>
        private void GetMessagesHtml(Message[] messages, int index, Action<Message[]> callback)
        {
            if(index >= messages.Length)
            {
                _dispatcher.Post(state => callback(messages), null);
                return;
            }

            Dictionary<string, object> data = new Dictionary<string, object>();
            data.Add(ApiKey_Key, ApiKey);
            data.Add(UserId_Key, UserId);
            data.Add(HTMLMessageId_Key, messages[index].htmlMessageId);
            data.Add(MessageId_Key, messages[index].messageId);
            data.Add(CampaignId_Key, messages[index].campaignId);

            WebClient webClient = new WebClient();
            webClient.Headers["Content-Type"] = "application/json";
            String httpBody = JsonConvert.SerializeObject(data);
            webClient.UploadStringAsync(new Uri(new Uri(ServerUrl), "rulegrid/mobile/message/getPhoneHTML"), "PUT", httpBody);
            webClient.UploadStringCompleted += (sender, e) =>
                                                   {
                                                       if(e.Error == null)
                                                       {
                                                           messages[index].Html = e.Result;
                                                       }

                                                       GetMessagesHtml(messages, index + 1, callback);
                                                   };
        }

        /// <summary>
        /// Call server to remove message
        /// </summary>
        /// <param name="messageId"></param>
        /// <param name="callback"></param>
        private void RemoveMessage(Message message, Action<string, Exception> callback)
        {
            Dictionary<string, object> data = new Dictionary<string, object>();
            data.Add(ApiKey_Key, ApiKey);
            data.Add(UserId_Key, UserId);
            data.Add( message.IsPush ? HTMLMessageId_Key : MessageId_Key, message.IsPush ? message.htmlMessageId : message.messageId);

            ExecuteRequest("rulegrid/mobile/message/removeMessage", data, callback);
        }

        /// <summary>
        /// Call server to notify about opened message
        /// </summary>
        /// <param name="messageId"></param>
        /// <param name="callback"></param>
        private void NotifyMessageOpened(Message message, Action<string, Exception> callback)
        {
            Dictionary<string, object> data = new Dictionary<string, object>();
            data.Add(ApiKey_Key, ApiKey);
            data.Add(UserId_Key, UserId);
            data.Add(MessageId_Key, message.messageId);
            data.Add(HTMLMessageId_Key, message.htmlMessageId);
            if( message.IsPush == true )
                data.Add(CampaignId_Key, message.campaignId);

            ExecuteRequest("rulegrid/mobile/message/messageOpened", data, callback);
        }

        /// <summary>
        /// Call server to send event
        /// </summary>
        /// <param name="eventName"></param>
        /// <param name="data"></param>
        /// <param name="callback"></param>
        private void SendEvent(String eventName, IDictionary<string, object> data,  Action<string, Exception> callback)
        {
            data = new Dictionary<string, object>(data ?? new Dictionary<string, object>());
            data.Add(EventName_Key, eventName);
            data.Add(ApiKey_Key, ApiKey);
            data.Add(UserId_Key, UserId);
            if( String.IsNullOrEmpty(Email) == false )
                data.Add(Email_Key, Email);
            data.Add(Locale_Key, Locale);
            data.Add(MobileDeviceToken_Key, Token);
            data.Add(MobileDevicePlatform_Key, DevicePlatform);
            data.Add(MobileDeviceType_Key, DeviceType);
            GeoCoordinate coordinate = UseGps && _detectedCoordinates != null ? _detectedCoordinates : Coordinates;
            if(coordinate != null)
            {
                data.Add(Latitude_Key, coordinate.Latitude);
                data.Add(Longitude_Key, coordinate.Longitude);
            }

            ExecuteRequest("rulegrid/events/process", data, callback);
        }

        public void setValue(String name, String value, CMClientListener clientListener/*Action<string> callback*/)
        {
            this._setValue(name, value, (str, ex) =>
                {
                    sendResponse(clientListener, ex);
                });
        }
        public void setValue(String name, Double value, CMClientListener clientListener/*Action<string> callback*/)
        {
            this._setValue(name, value, (str, ex) =>
                {
                    sendResponse(clientListener, ex);
                });
        }
        public void setValue(String name, DateTime value, CMClientListener clientListener/*Action<string> callback*/)
        { 
            string date = value.ToString("yyyy-MM-dd");
            this._setValue(name, date, (str, ex) =>
                {
                    sendResponse(clientListener, ex);
                });
        }
        public void setValue(String name, Boolean value, CMClientListener clientListener/*Action<string> callback*/)
        {
            this._setValue(name, value, (str, ex) =>
                {
                    sendResponse(clientListener, ex);
                });
        }

        /// <summary>
        /// Call server to set value
        /// </summary>one
        /// <param name="eventName"></param>
        /// <param name="data"></param>
        /// <param name="callback"></param>
        private void _setValue(String name, Object value, Action<string, Exception> callback)
        {
            IDictionary<string, object> data = new Dictionary<string, object>();
            data.Add(Name_Key, name);
            data.Add(Value_Key, value);
            data.Add(ApiKey_Key, ApiKey);
            data.Add(UserId_Key, UserId);
            ExecuteRequest("rulegrid/api/userParams/setValue", data, callback);
            //ExecuteRequestWithErrorHandle("rulegrid/api/userParams/setValue", data, callback);
        }

        /// <summary>
        /// Call server to change user id
        /// </summary>
        /// <param name="eventName"></param>
        /// <param name="data"></param>
        /// <param name="callback"></param>
        public void changeUserId(String newUserId, CMClientListener clientListener/*Action<string> callback*/)
        {
            IDictionary<string, object> data = new Dictionary<string, object>();
            data.Add(ApiKey_Key, ApiKey);
            data.Add(UserId_Key, UserId);
            data.Add(NewUserId_Key, newUserId);
            ExecuteRequest("rulegrid/api/user/changeUserId", data, (str, ex) =>
                {
                    sendResponse(clientListener, ex);
                });
            //ExecuteRequestWithErrorHandle("rulegrid/api/user/changeUserId", data, callback);
        }

            /// <summary>
        /// Perform http rest request to server
        /// </summary>
        /// <param name="relativePath"></param>
        /// <param name="data"></param>
        /// <param name="callback"></param>
        private void ExecuteRequestWithErrorHandle(String relativePath, IDictionary<string, object> data, Action<string> callback)
        {
            data = data ?? new Dictionary<string, object>();
            WebClient webClient = new WebClient();
            webClient.Headers["Content-Type"] = "application/json";
            String httpBody = JsonConvert.SerializeObject(data);
            Debug.WriteLine("Call {0} with body {1}", relativePath, httpBody);
            Uri uri = new Uri(new Uri(ServerUrl), relativePath);
            webClient.UploadStringAsync(uri, "PUT", httpBody);
            webClient.UploadStringCompleted += (sender, e) => 
                {
                    try{
                        HandleResult(e, (str, ex) => { } );
                        callback("");
                    }catch( Exception ex )
                    {
                        callback( ex.ToString() );
                    }
                };
        }

        /// <summary>
        /// Perform http rest request to server
        /// </summary>
        /// <param name="relativePath"></param>
        /// <param name="data"></param>
        /// <param name="callback"></param>
        private void ExecuteRequest(String relativePath, IDictionary<string, object> data, Action<string,Exception> callback)
        {
            data = data ?? new Dictionary<string, object>();
            WebClient webClient = new WebClient();
            webClient.Headers["Content-Type"] = "application/json";
            String httpBody = JsonConvert.SerializeObject(data);
            Debug.WriteLine("Call {0} with body {1}", relativePath, httpBody);
            Uri uri = new Uri(new Uri(ServerUrl), relativePath);
            webClient.UploadStringAsync(uri, "PUT", httpBody);
            webClient.UploadStringCompleted += (sender, e) => HandleResult(e, callback);
        }

        /// <summary>
        /// Handle response, check for errors
        /// </summary>
        /// <param name="result"></param>
        /// <param name="callback"></param>
        /// <param name="sync"></param>z
        /// 
        private void HandleResult(UploadStringCompletedEventArgs result, Action<String,Exception> callback)
        {
            if(result.Error != null)
            {
                if (callback != null)
                {
                    _dispatcher.Post(state => callback.Invoke(null, result.Error), null);
                }
                //Debug.Assert(false, result.Error.ToString());            
                return;
            }

            Debug.WriteLine("Response is {0}", result.Result);

            if(callback == null)
            {
                return;
            }

            if(String.IsNullOrEmpty(result.Result))
            {
                _dispatcher.Post(state => callback.Invoke(null, null), null);
            }

            try
            {
                Object deserialized = JsonConvert.DeserializeObject(result.Result);
                if (deserialized is JObject)
                {
                    JObject jobject = deserialized as JObject;
                    try
                    {
                        String state = jobject["status"].Value<String>();
                        if (state == "ERROR")
                        {
                            String errorMessage = jobject["message"].Value<String>();

                            if (errorMessage != null)
                            {
                                Debug.Assert(false, errorMessage);
                              //  return;
                            }
                            else
                            {
                                errorMessage = jobject["error_message"].Value<String>();
                                if (errorMessage != null)
                                {
                                    Debug.Assert(false, errorMessage);
                                    return;
                                }
                            }


                            try
                            {
                                String code = jobject["code"].Value<String>();
                                if (code == "NO.SUCH.USER.PARAMETER")
                                {
                                    throw new NoSuchUserException(errorMessage);
                                }
                                if (code == "NO.SUCH.PARAMETER")
                                {
                                    throw new NoSuchParameterException(errorMessage);
                                }

                                if (code == "ERROR.USER.ALREADY.EXISTS")
                                {
                                    throw new UserAlreadyExistException(errorMessage);
                                }
                                if (code == "WRONG.USERNAME.PASSWORD")
                                {
                                    throw new WrongUserNameOrPasswordException(errorMessage);
                                }
                            }
                            catch (System.Exception error)
                            {
                                _dispatcher.Post(call => callback.Invoke(null, error), null);
                            }
                        }
                    }
                    catch (ArgumentNullException e)
                    {
                        _dispatcher.Post(call => callback.Invoke(null, e), null);
                    }
                }

                _dispatcher.Post(state => callback.Invoke(result.Result, null), null);
            }
            catch (JsonException e)
            {
                Debug.Assert(false, e.Message);
//                _dispatcher.Post(state => callback.Invoke(null, e), null);
            }
        }
    }
}
