namespace AdMaiora.AppKit
{
    using System;
    using System.Collections.Generic;

    using Android.App;
    using Android.Content;
    using Android.OS;
    using Android.Runtime;
    using Android.Gms.Common;
    using Android.Support.V4.App;
    using Android.Media;

    using Firebase.Iid;
    using Firebase.Messaging;

    using AdMaiora.AppKit.Data;
    using AdMaiora.AppKit.Notifications;
    using Firebase;

    [Service]
    [IntentFilter(new[] { "com.google.firebase.INSTANCE_ID_EVENT" })]
    class AppKitFirebaseIdService : FirebaseInstanceIdService
    {
        #region Service Methods

        public override void OnTokenRefresh()
        {
            string token = FirebaseInstanceId.Instance.Token;
            AppKitApplication.Current?.OnRegisteredForRemoteNotifications(token);
        }

        #endregion
    }

    [Service]
    [IntentFilter(new[] { "com.google.firebase.MESSAGING_EVENT" })]
    class AppKitFirebaseMessagingService : FirebaseMessagingService
    {
        #region Constants and Fields

        private static PushNotificationData _launchNotification;

        #endregion

        #region Helper Methods

        public static bool HasStartUpNotification(out PushNotificationData launchNotification)
        {
            launchNotification = null;

            if (_launchNotification == null)
                return false;

            launchNotification = _launchNotification;
            _launchNotification = null;

            return true;
        }

        public static PushNotificationData GetNotificationData(RemoteMessage message)
        {
            /*
             * JSON Sample for notifications
             * 
             * 
             * 
                {
                   "data": {
                      "title": "",
                      "body": "",
                      "action": 0, // Default value 0 stands for plain message (title and body)
                      "payload": { // Optional. A custom JSON payload to be used in conjunction with the action }
                   }
                }
            *
            */

            if (message == null)
                return null;

            var data = new PushNotificationData();

            data.Add("title", message.Data.ContainsKey("title") ? message.Data["title"] : null);
            data.Add("body", message.Data.ContainsKey("body") ? message.Data["body"] : null);
            data.Add("action", message.Data.ContainsKey("action") ? Int32.Parse(message.Data["action"]) : 0);
            data.Add("payload", message.Data.ContainsKey("payload") ? message.Data["payload"] : null);

            return data;
        }

        #endregion

        #region Service Methods

        public override void OnMessageReceived(RemoteMessage message)
        {
            if(AppKitApplication.Current == null
                || !AppKitApplication.Current.IsApplicationInForeground)
            {               
                var data = GetNotificationData(message);
                string title = (string)data["title"];
                string text = (string)data["body"];

                if(AppKitApplication.Current == null)
                    _launchNotification = data;

                Intent intent = new Intent();
                intent.AddFlags(ActivityFlags.ReorderToFront);
                intent.SetComponent(GetRootActivityComponentName());
                PendingIntent pendingIntent = PendingIntent.GetActivity(this, 0, intent, PendingIntentFlags.UpdateCurrent);

                var notificationBuilder = new NotificationCompat.Builder(this)
                    .SetSmallIcon(GetSmallIconResourceId())
                    .SetContentIntent(pendingIntent)
                    .SetContentTitle(title)
                    .SetContentText(text)                    
                    .SetSound(RingtoneManager.GetDefaultUri(RingtoneType.Notification))
                    .SetAutoCancel(true);

                if (Build.VERSION.SdkInt >= BuildVersionCodes.Lollipop)
                {
                    notificationBuilder.SetDefaults(NotificationCompat.DefaultVibrate);
                    notificationBuilder.SetPriority(NotificationCompat.PriorityDefault);
                    notificationBuilder.SetVibrate(new long[] { 50, 50 });
                }

                var notificationManager = NotificationManager.FromContext(this);
                notificationManager.Notify(0, notificationBuilder.Build());
            }

            AppKitApplication.Current?.OnReceivedRemoteNotification(message);
        }

        #endregion

        #region Methods

        private ComponentName GetRootActivityComponentName()
        {
            // Get the root activity of the task that your activity is running in
            //ActivityManager am = (ActivityManager)Android.App.Application.Context.GetSystemService(Context.ActivityService);
            //IList<ActivityManager.AppTask> tasks = am.AppTasks;
            //ActivityManager.AppTask task = tasks[0];
            //return task.TaskInfo.BaseActivity
            string packageName = Application.Context.PackageName;
            Intent intent = Application.Context.PackageManager.GetLaunchIntentForPackage(packageName);
            return intent.Component;
        }

        private int GetSmallIconResourceId()
        {
            string packageName = Application.Context.PackageName;
            int id = Application.Context.Resources.GetIdentifier("ic_notification", "drawable", packageName);    
            if(id == -1)
                return Application.Context.Resources.GetIdentifier("ic_launcher", "drawable", packageName);

            return id;
        }

        #endregion
    }

    public class AppKitApplication : Application
    {
        #region Inner Classes

        public class LifecycleManager : Java.Lang.Object, Android.App.Application.IActivityLifecycleCallbacks
        {
            #region Constants and Fields

            private int _resumed;
            private int _paused;
            private int _started;
            private int _stopped;

            private bool _inPause;

            private Stack<Activity> _activities;

            #endregion

            #region Events

            public event EventHandler Resumed;
            public event EventHandler Paused;

            #endregion

            #region Properties

            public Activity CurrentActivity
            {
                get
                {
                    if (_activities == null)
                        return null;

                    return _activities.Peek();
                }
            }

            public bool IsApplicationRunning
            {
                get
                {
                    return _started > _stopped;
                }
            }

            public bool IsApplicationInForeground
            {
                get
                {
                    return _resumed > _paused;
                }
            }

            #endregion

            #region IActivityLifecycleCallbacks Methods

            public void OnActivityCreated(Activity activity, Bundle savedInstanceState)
            {
                //Android.Util.Log.Info(GlobalSettings.AppLogTag, activity.GetType().Name + " Created");
            }

            public void OnActivityStarted(Activity activity)
            {
                ++_started;

                //Android.Util.Log.Info(GlobalSettings.AppLogTag, activity.GetType().Name + " Started");
            }

            public void OnActivityResumed(Activity activity)
            {
                ++_resumed;

                if (_inPause)
                {
                    _inPause = false;
                    if (Resumed != null)
                        Resumed(this, EventArgs.Empty);
                }

                if (_activities == null)
                    _activities = new Stack<Activity>();

                _activities.Push(activity);

                //Android.Util.Log.Info(GlobalSettings.AppLogTag, activity.GetType().Name + " Resumed");
            }

            public void OnActivityPaused(Activity activity)
            {
                ++_paused;

                if (!this.IsApplicationInForeground)
                {
                    _inPause = true;
                    if (Paused != null)
                        Paused(this, EventArgs.Empty);
                }

                if (_activities != null)
                    _activities.Pop();

                //Android.Util.Log.Info(GlobalSettings.AppLogTag, activity.GetType().Name + " Paused");
            }

            public void OnActivitySaveInstanceState(Activity activity, Bundle outState)
            {
            }

            public void OnActivityStopped(Activity activity)
            {
                ++_stopped;

                //Android.Util.Log.Info(GlobalSettings.AppLogTag, activity.GetType().Name + " Stopped");
            }

            public void OnActivityDestroyed(Activity activity)
            {
                //Android.Util.Log.Info(GlobalSettings.AppLogTag, activity.GetType().Name + " Destroyed");
            }

            #endregion
        }

        #endregion

        #region Constants and Fields        

        private LifecycleManager _lfcb;

        #endregion

        #region Constructors

        public AppKitApplication(IntPtr javaReference, JniHandleOwnership transfer)
            : base(javaReference, transfer)
        {
            AppKitApplication.Current = this;           

            // Handle activities lifecycle
            _lfcb = new AppKitApplication.LifecycleManager();
            _lfcb.Resumed += App_Resumed;
            _lfcb.Paused += App_Paused;

            RegisterActivityLifecycleCallbacks(_lfcb);
        }

        #endregion

        #region Properties

        public static AppKitApplication Current
        {
            get;
            private set;
        }

        public bool IsApplicationRunning
        {
            get
            {
                return _lfcb.IsApplicationRunning;
            }
        }

        public bool IsApplicationInForeground
        {
            get
            {
                return _lfcb.IsApplicationInForeground;
            }
        }

        #endregion

        #region Application Methods

        public override void OnCreate()
        { 
            base.OnCreate();

            PushNotificationData notification = null;
            if (AppKitFirebaseMessagingService.HasStartUpNotification(out notification))
                PushNotificationManager.Current.StorePendingNotification(notification);
        }

        public virtual void OnRegisteredForRemoteNotifications(string token)
        {
        }

        public virtual void OnFailedToRegisterForRemoteNotifications(Exception ex)
        {
        }

        public virtual void OnReceivedRemoteNotification(RemoteMessage message)
        {
            PushNotificationData notification = AppKitFirebaseMessagingService.GetNotificationData(message);
            OnReceivedRemoteNotification(notification);
        }

        public virtual void OnReceivedRemoteNotification(PushNotificationData data)
        {
        }

        public virtual void OnResume()
        {
        }

        public virtual void OnPause()
        {
        }

        #endregion

        #region Methods

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (_lfcb != null)
            {
                UnregisterActivityLifecycleCallbacks(_lfcb);

                _lfcb.Resumed -= App_Resumed;
                _lfcb.Paused -= App_Paused;
            }
        }

        protected void RegisterForRemoteNotifications()
        {
            // Configure Push Notifications
            int resultCode = GoogleApiAvailability.Instance.IsGooglePlayServicesAvailable(this.ApplicationContext);
            if (resultCode == ConnectionResult.Success)
            {
                // Everything is fine!
            }
            else
            {
                OnFailedToRegisterForRemoteNotifications(new GooglePlayServicesNotAvailableException(resultCode));
            }
        }

        #endregion

        #region Event Handlers

        private void App_Resumed(object sender, EventArgs e)
        {
            OnResume();
        }

        private void App_Paused(object sender, EventArgs e)
        {
            OnPause();
        }

        #endregion
    }
}