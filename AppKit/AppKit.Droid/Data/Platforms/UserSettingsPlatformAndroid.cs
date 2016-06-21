namespace AdMaiora.AppKit.Data
{
    using System;

    using Android.Content;

    class UserSettingsPlatformAndroid : IUserSettingsPlatform
    {
        public bool GetBoolValue(string key)
        {
            ISharedPreferences preferences =
                Android.App.Application.Context.GetSharedPreferences("user-settings", FileCreationMode.Private);

            return preferences.GetBoolean(key, false);
        }
        
        public int GetIntValue(string key)
        {
            ISharedPreferences preferences =
                Android.App.Application.Context.GetSharedPreferences("user-settings", FileCreationMode.Private);

            return preferences.GetInt(key, 0);
        }
        
        public string GetStringValue(string key)
        {
            ISharedPreferences preferences =
                Android.App.Application.Context.GetSharedPreferences("user-settings", FileCreationMode.Private);

            return preferences.GetString(key, null);
        }
        
        public void SetBoolValue(string key, bool value)
        {
            ISharedPreferences preferences =
                Android.App.Application.Context.GetSharedPreferences("user-settings", FileCreationMode.Private);

            ISharedPreferencesEditor editor = preferences.Edit();
            editor.PutBoolean(key, value);
            editor.Commit();
        }
        
        public void SetIntValue(string key, int value)
        {
            ISharedPreferences preferences =
                Android.App.Application.Context.GetSharedPreferences("user-settings", FileCreationMode.Private);

            ISharedPreferencesEditor editor = preferences.Edit();
            editor.PutInt(key, value);
            editor.Commit();
        }

        public void SetStringValue(string key, string value)
        {
            ISharedPreferences preferences =
                Android.App.Application.Context.GetSharedPreferences("user-settings", FileCreationMode.Private);

            ISharedPreferencesEditor editor = preferences.Edit();
            editor.PutString(key, value);
            editor.Commit();
        }
    }
}