namespace AdMaiora.AppKit
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using AdMaiora.AppKit.Data;
    using AdMaiora.AppKit.Notifications;
    using Foundation;
    using UIKit;
    using UserNotifications;

    class AppKitApnMessagingService
    {
        #region Helper Methods

        public static bool HandleStartUpNotification(NSDictionary launchOptions, out PushNotificationData launchNotification)
        {
            launchNotification = null;

            if (launchOptions != null
                && launchOptions.ContainsKey(UIApplication.LaunchOptionsRemoteNotificationKey))
            {
                var message = launchOptions.ObjectForKey(UIApplication.LaunchOptionsRemoteNotificationKey) as NSDictionary;
                if (message != null)
                {
                    launchNotification = GetNotificationData(message);
                    return true;
                }
            }

            return false;
        }


        public static PushNotificationData GetNotificationData(NSDictionary message)
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

            if (message == null)
                return null;

            PushNotificationData notification = new PushNotificationData();

            // Are we getting this from an "remote notification launch options" ?
            if (message.ContainsKey(UIApplication.LaunchOptionsRemoteNotificationKey))
                message = (NSDictionary)message.ObjectForKey(UIApplication.LaunchOptionsRemoteNotificationKey);

            // Check to see if the dictionary has the aps key.  
            // This is the notification payload you would have sent
            if (message.ContainsKey(new NSString("aps")))
            {
                //Get the aps dictionary
                NSDictionary aps = message.ObjectForKey(new NSString("aps")) as NSDictionary;
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
                        if (aps.ObjectForKey(new NSString("alert")) is NSString)
                        {
                            var alert = aps.ObjectForKey(new NSString("alert")) as NSString;
                            notification.Add("title", null);
                            notification.Add("body", alert.ToString());
                        }
                        else if (aps.ObjectForKey(new NSString("alert")) is NSDictionary)
                        {
                            var alert = aps.ObjectForKey(new NSString("alert")) as NSDictionary;
                            if (alert != null)
                            {
                                if (alert.ContainsKey(new NSString("title")))
                                    notification.Add("title", alert.ObjectForKey(new NSString("title")).ToString());

                                if (alert.ContainsKey(new NSString("body")))
                                    notification.Add("body", alert.ObjectForKey(new NSString("body")).ToString());
                            }
                        }
                    }
                }
            }

            // Check to see if the dictionary has the data key.  
            // This is the custom payload you would have sent
            if (message.ContainsKey(new NSString("data")))
            {
                //Get the data dictionary
                NSDictionary data = message.ObjectForKey(new NSString("data")) as NSDictionary;
                if (data != null)
                {
                    notification.Add("action", (data.ObjectForKey(new NSString("action")) as NSNumber).Int32Value);

                    if (data.ContainsKey(new NSString("payload")))
                    {
                        if (data.ObjectForKey(new NSString("payload")) is NSString)
                        {
                            NSString payload = data.ObjectForKey(new NSString("payload")) as NSString;
                            notification.Add("payload", payload.ToString());
                        }
                        else if (data.ObjectForKey(new NSString("payload")) is NSDictionary)
                        {
                            NSDictionary target = data.ObjectForKey(new NSString("payload")) as NSDictionary;
                            if (target != null)
                            {
                                NSError error = null;
                                NSData d = NSJsonSerialization.Serialize(target, NSJsonWritingOptions.PrettyPrinted, out error);
                                NSString json = NSString.FromData(d, NSStringEncoding.UTF8);
                                notification.Add("payload", json.ToString());
                            }
                        }
                    }
                }
            }
            else
            {
                notification.Add("action", 0);
                notification.Add("payload", null);
            }

            return notification;
        }

        #endregion
    }

    public abstract class UIAppKitApplicationDelegate : UIApplicationDelegate
    {
        #region Constants and Fields

        private AppKitApnMessagingService _messagingService = new AppKitApnMessagingService();

        #endregion

        #region Properties

        public override UIWindow Window
        {
            get;
            set;
        }

        public bool IsApplicationInForeground
        {
            get
            {
                return this.Window != null 
                    && UIApplication.SharedApplication.ApplicationState == UIApplicationState.Active;
            }
        }

        #endregion

        #region Application Methods

        public override void ReceivedRemoteNotification(UIApplication application, NSDictionary userInfo)
        {
            PushNotificationData notification = AppKitApnMessagingService.GetNotificationData(userInfo);
            ReceivedRemoteNotification(notification);
        }

        public virtual void ReceivedRemoteNotification(PushNotificationData data)
        {

        }

        #endregion

        #region Methods

        protected void RegisterForRemoteNotifications()
        {

            if (UIDevice.CurrentDevice.CheckSystemVersion(10, 0))
            {
                UNUserNotificationCenter.Current.RequestAuthorization(
                    UNAuthorizationOptions.Alert | UNAuthorizationOptions.Sound | UNAuthorizationOptions.Sound,
                    (granted, error) =>
                    {
                        if (granted)
                            InvokeOnMainThread(UIApplication.SharedApplication.RegisterForRemoteNotifications);
                    });
            }
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

        protected bool RegisterMainLauncher(UIViewController mainViewController, NSDictionary launchOptions)
        {
            PushNotificationData notification = null;
            if (!AppKitApnMessagingService.HandleStartUpNotification(launchOptions, out notification))
                PushNotificationManager.Current.StorePendingNotification(notification);

            this.Window = new UIWindow(UIScreen.MainScreen.Bounds);
            this.Window.RootViewController = mainViewController;
            this.Window.MakeKeyAndVisible();

            return true;
        }

        #endregion
    }
}