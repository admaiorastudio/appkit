namespace AdMaiora.AppKit
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    using Foundation;
    using UIKit;

    public class UIAppKitApplicationDelegate : UIApplicationDelegate
    {
        #region Properties

        public override UIWindow Window
        {
            get;
            set;
        }

        #endregion

        #region Application Methods

        public override void ReceivedRemoteNotification(UIApplication application, NSDictionary userInfo)
        {
            ReceivedRemoteNotification(GetNotificationData(userInfo));
        }

        public virtual void ReceivedRemoteNotification(Dictionary<string, object> data)
        {

        }

        #endregion

        #region Methods

        protected void RegisterForRemoteNotifications()
        {
            if (UIDevice.CurrentDevice.CheckSystemVersion(8, 0))
            {
                var pushSettings = UIUserNotificationSettings.GetSettingsForTypes(
                       UIUserNotificationType.Alert | UIUserNotificationType.Badge | UIUserNotificationType.Sound,
                       new NSSet());

                UIApplication.SharedApplication.RegisterUserNotificationSettings(pushSettings);
                UIApplication.SharedApplication.RegisterForRemoteNotifications();
            }
            else
            {
                UIRemoteNotificationType notificationTypes = UIRemoteNotificationType.Alert | UIRemoteNotificationType.Badge | UIRemoteNotificationType.Sound;
                UIApplication.SharedApplication.RegisterForRemoteNotificationTypes(notificationTypes);
            }
        }

        private Dictionary<string, object> GetNotificationData(NSDictionary userInfo)
        {
            /*
             * JSON Sample for notifications
             * 
             * 
             * 
                {
                   "aps":{
                      "alert": {
                        "title": "",
                        "body": ""
                      }
                   },
                   "data":{
                      "action": 0, // Default value 0 stands for plain message (title and body)
                      "payload":{ // Optional. A custom JSON payload to be used in conjunction with the action }
                   }
                }
            *
            */

            if (userInfo == null)
                return null;

            var values = new Dictionary<string, object>();

            // Are we getting this from an "remote notification launch options" ?
            if (userInfo.ContainsKey(UIApplication.LaunchOptionsRemoteNotificationKey))
                userInfo = (NSDictionary)userInfo.ObjectForKey(UIApplication.LaunchOptionsRemoteNotificationKey);

            // Check to see if the dictionary has the aps key.  
            // This is the notification payload you would have sent
            if (userInfo.ContainsKey(new NSString("aps")))
            {
                //Get the aps dictionary
                NSDictionary aps = userInfo.ObjectForKey(new NSString("aps")) as NSDictionary;
                if (aps != null)
                {
                    //Extract the alert text
                    // NOTE: If you're using the simple alert by just specifying
                    // "  aps:{alert:"alert msg here"}  ", this will work fine.
                    // But if you're using a complex alert with Localization keys, etc.,
                    // your "alert" object from the aps dictionary will be another NSDictionary.
                    // Basically the JSON gets dumped right into a NSDictionary,
                    // so keep that in mind.
                    if (aps.ContainsKey(new NSString("alert")))
                    {
                        NSDictionary alert = aps.ObjectForKey(new NSString("alert")) as NSDictionary;
                        if (alert != null)
                        {
                            values.Add("title", alert.ObjectForKey(new NSString("title")).ToString());
                            values.Add("body", alert.ObjectForKey(new NSString("body")).ToString());
                        }
                    }
                }
            }

            // Check to see if the dictionary has the data key.  
            // This is the custom payload you would have sent
            if (userInfo.ContainsKey(new NSString("data")))
            {
                //Get the data dictionary
                NSDictionary data = userInfo.ObjectForKey(new NSString("data")) as NSDictionary;
                if (data != null)
                {
                    values.Add("action", (data.ObjectForKey(new NSString("action")) as NSNumber).Int32Value);

                    if (data.ContainsKey(new NSString("payload")))
                    {
                        NSDictionary target = data.ObjectForKey(new NSString("payload")) as NSDictionary;
                        if (target != null)
                        {
                            NSError error = null;
                            NSData d = NSJsonSerialization.Serialize(target, NSJsonWritingOptions.PrettyPrinted, out error);
                            NSString json = NSString.FromData(d, NSStringEncoding.UTF8);
                            values.Add("payload", json.ToString());
                        }
                    }
                }
            }

            return values;
        }

        #endregion
    }
}