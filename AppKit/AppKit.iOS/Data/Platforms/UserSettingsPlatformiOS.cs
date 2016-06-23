namespace AdMaiora.AppKit.Data
{
    using System;

    using Foundation;

    public class UserSettingsPlatformiOS : IUserSettingsPlatform
    {
        public bool GetBoolValue(string key)
        {
            return NSUserDefaults.StandardUserDefaults.BoolForKey(key);
        }

        public int GetIntValue(string key)
        {
            return (int)NSUserDefaults.StandardUserDefaults.IntForKey(key);
        }

        public string GetStringValue(string key)
        {
            return NSUserDefaults.StandardUserDefaults.StringForKey(key);
        }

        public void SetBoolValue(string key, bool value)
        {
            NSUserDefaults.StandardUserDefaults.SetBool(value, key);
            NSUserDefaults.StandardUserDefaults.Synchronize();
        }

        public void SetIntValue(string key, int value)
        {
            NSUserDefaults.StandardUserDefaults.SetInt(value, key);
            NSUserDefaults.StandardUserDefaults.Synchronize();
        }

        public void SetStringValue(string key, string value)
        {
            NSUserDefaults.StandardUserDefaults.SetString(value, key);
            NSUserDefaults.StandardUserDefaults.Synchronize();
        }
    }
}