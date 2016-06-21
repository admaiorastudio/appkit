namespace AdMaiora.AppKit.Data
{
    using System;

    public class UserSettings
    {
        #region Constants and Fields

        private IUserSettingsPlatform _userSettings;

        #endregion

        #region Constructor

        public UserSettings(IUserSettingsPlatform userSettingsPlatform)
        {
            _userSettings = userSettingsPlatform;
        }

        #endregion

        #region Public Methods

        public int GetIntValue(string key)
        {
            return _userSettings.GetIntValue(key);
        }

        public string GetStringValue(string key)
        {
            return _userSettings.GetStringValue(key);
        }

        public bool GetBoolValue(string key)
        {
            return _userSettings.GetBoolValue(key);
        }

        public DateTime? GetDateTimeValue(string key)
        {
            string dateString = GetStringValue(key);

            if (String.IsNullOrWhiteSpace(dateString))
                return null;

            DateTime dateTime = DateTime.MinValue;
            if (!DateTime.TryParse(dateString, out dateTime))
                return null;

            return dateTime;
        }

        public void SetIntValue(string key, int value)
        {
            _userSettings.SetIntValue(key, value);
        }

        public void SetStringValue(string key, string value)
        {
            _userSettings.SetStringValue(key, value);
        }

        public void SetBoolValue(string key, bool value)
        {
            _userSettings.SetBoolValue(key, value);
        }

        public void SetDateTimeValue(string key, DateTime? value)
        {
            _userSettings.SetStringValue(key, value.HasValue ? value.ToString() : null);
        }

        #endregion
    }
}