namespace AdMaiora.AppKit
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    using Android.App;
    using Android.Content;
    using Android.OS;
    using Android.Runtime;
    using Android.Gms.Common;
    using Android.Gms.Gcm.Iid;
    using Android.Gms.Gcm;
    using Android.Views;
    using Android.Widget;

    public class AppKitApplication : Application
    {
        #region Inner Classes

        class LifecycleManager : Java.Lang.Object, Android.App.Application.IActivityLifecycleCallbacks
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

        [Service(Exported = false)]
        public class GcmRegistrationIntentService : IntentService
        {
            #region Constants and Fields

            public static string GoogleGcmSenderID;
            public static AppKitApplication Holder;

            private static object _locker = new object();

            #endregion

            #region Constructors

            public GcmRegistrationIntentService() 
                : base("GcmRegistrationIntentService")
            {
            }

            #endregion

            #region Methods

            protected override void OnHandleIntent(Intent intent)
            {
                try
                {
                    lock (_locker)
                    {
                        var instanceID = InstanceID.GetInstance(this);
                        var token = instanceID.GetToken(
                            GoogleGcmSenderID, GoogleCloudMessaging.InstanceIdScope, null);

                        Subscribe(token);

                        Holder.OnRegisteredForRemoteNotifications(instanceID, token);
                    }
                }
                catch (Exception ex)
                {
                    Holder.OnFailedToRegisterForRemoteNotifications(ex);
                }
            }

            private void Subscribe(string token)
            {
                var pubSub = GcmPubSub.GetInstance(this);
                pubSub.Subscribe(token, "/topics/global", null);
            }

            #endregion
        }

        [Service(Exported = false), IntentFilter(new[] { "com.google.android.gms.iid.InstanceID" })]
        public class GcmInstanceIDListenerService : InstanceIDListenerService
        {
            #region Methods

            public override void OnTokenRefresh()
            {
                var intent = new Intent(this, typeof(GcmRegistrationIntentService));
                StartService(intent);
            }

            #endregion
        }

        [Service(Exported = false), IntentFilter(new[] { "com.google.android.c2dm.intent.RECEIVE" })]
        public class GcmListenerService : Android.Gms.Gcm.GcmListenerService
        {
            #region Constants and Fields

            public static AppKitApplication Holder;

            #endregion

            #region Public Methods

            public override void OnMessageReceived(string from, Bundle data)
            {
                Holder.OnReceivedRemoteNotification(from, data);
            }

            #endregion
        }

        #endregion

        #region Constants and Fields

        private LifecycleManager _lfcb;

        #endregion

        #region Constructors and Destructors

        public AppKitApplication(IntPtr javaReference, JniHandleOwnership transfer)
            : base(javaReference, transfer)
        {
            // Handle activities lifecycle
            _lfcb = new AppKitApplication.LifecycleManager();            
            _lfcb.Resumed += App_Resumed;
            _lfcb.Paused += App_Paused;

            RegisterActivityLifecycleCallbacks(_lfcb);
        }

        #endregion

        #region Properties

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

        public virtual void OnRegisteredForRemoteNotifications(InstanceID instanceID, string token)
        {            
        }

        public virtual void OnFailedToRegisterForRemoteNotifications(Exception ex)
        {
        }

        public virtual void OnReceivedRemoteNotification(string from, Bundle bundle)
        {
            OnReceivedRemoteNotification(GetNotificationData(bundle));
        }

        public virtual void OnReceivedRemoteNotification(Dictionary<string, object> data)
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

        protected void RegisterForRemoteNotifications(string googleGcmSenderId)
        {
            // Configure Push Notifications
            int resultCode = GoogleApiAvailability.Instance.IsGooglePlayServicesAvailable(this.ApplicationContext);
            if (resultCode == ConnectionResult.Success)
            {
                GcmRegistrationIntentService.GoogleGcmSenderID = googleGcmSenderId;
                GcmRegistrationIntentService.Holder = this;

                GcmListenerService.Holder = this;

                var intent = new Intent(this, typeof(GcmRegistrationIntentService));
                StartService(intent);
            }
            else
            {
                OnFailedToRegisterForRemoteNotifications(new GooglePlayServicesNotAvailableException(resultCode));
            }
        }

        private Dictionary<string, object> GetNotificationData(Bundle bundle)
        {
            /*
             * JSON Sample for notifications
             * 
             * 
             * 
                {
                   "data":{
                      "title": "",
                      "body": "",
                      "action": 0, // Default value 0 stands for plain message (title and body)
                      "payload":{ // Optional. A custom JSON payload to be used in conjunction with the action }
                   }
                }
            *
            */

            if (bundle == null)
                return null;

            var data = new Dictionary<string, object>();

            data.Add("title", bundle.GetString("title"));
            data.Add("body", bundle.GetString("body"));
            data.Add("action", Int32.Parse(bundle.GetString("action", "0")));
            data.Add("payload", bundle.GetString("payload"));

            return data;
        }

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