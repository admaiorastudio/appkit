namespace AdMaiora.AppKit.Data
{
    using System;

    public interface IUserSettingsPlatform
    {
        int GetIntValue(string key);
        string GetStringValue(string key);
        bool GetBoolValue(string key);

        void SetIntValue(string key, int value);
        void SetStringValue(string key, string value);
        void SetBoolValue(string key, bool value);
    }
}