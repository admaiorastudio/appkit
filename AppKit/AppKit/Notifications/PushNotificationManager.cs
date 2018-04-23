namespace AdMaiora.AppKit.Notifications
{
    using AdMaiora.AppKit.Data;
    using System;
    using System.Collections.Generic;
    using System.Text;

    public class PushNotificationEventArgs : EventArgs
    {
        #region Properties

        public int Action
        {
            get;
            private set;
        }

        public string Payload
        {
            get;
            private set;
        }

        public Exception Error
        {
            get;
            private set;
        }

        #endregion

        #region Constructors

        public PushNotificationEventArgs(int action, string payload)
        {
            this.Action = action;
            this.Payload = payload;
        }

        public PushNotificationEventArgs(Exception error)
        {
            this.Error = error;
        }

        #endregion
    }

    public sealed class PushNotificationManager
    {
        #region Singleton Methods

        private static readonly Lazy<PushNotificationManager> _lazy =
            new Lazy<PushNotificationManager>(() => new PushNotificationManager());

        public static PushNotificationManager Current { get { return _lazy.Value; } }

        private PushNotificationManager()
        {
        }

        #endregion

        #region Events

        public event EventHandler<EventArgs> PushNotificationRegistered;
        public event EventHandler<PushNotificationEventArgs> PushNotificationRegistrationFailed;
        public event EventHandler<PushNotificationEventArgs> PushNotificationReceived;

        #endregion

        #region Constants and Fields

        private PushNotificationData _pendingNotification;

        #endregion

        #region Properties

        public bool HasPendingNotification
        {
            get { return _pendingNotification != null; }
        }

        #endregion

        #region Notification Methods

        public void HandlePushNotificationRegistered()
        {
            PushNotificationRegistered?.Invoke(this, EventArgs.Empty);
        }

        public void HandlePushNotificationRegistrationFailed(Exception error)
        {
            PushNotificationRegistrationFailed?.Invoke(this, new PushNotificationEventArgs(error));
        }

        public void HandlePushNotification(PushNotificationData notification)
        {
            int action = (int)notification["action"];
            string payload = (string)notification["payload"];

            PushNotificationReceived?.Invoke(this, new PushNotificationEventArgs(action, payload));
        }

        public void StorePendingNotification(PushNotificationData notification)
        {
            _pendingNotification = notification;
        }

        public PushNotificationData ConsumePendingNotification(bool notify = false)
        {
            if (_pendingNotification == null)
                return null;

            PushNotificationData notification = _pendingNotification;
            _pendingNotification = null;

            if (notify)
                HandlePushNotification(notification);

            return notification;
        }

        #endregion
    }
}
