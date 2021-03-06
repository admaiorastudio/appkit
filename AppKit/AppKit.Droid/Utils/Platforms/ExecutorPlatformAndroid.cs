namespace AdMaiora.AppKit.Utils
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Android.App;
    using Android.Content;
    using Android.OS;

    using AdMaiora.AppKit.IO;
    using Android.Support.V4.Content;

    public class ExecutorPlatformAndroid : IExecutorPlatform
    {
        public void DebugOutput(string tag, string format, params object[] args)
        {
            Android.Util.Log.Debug(tag, format, args);
        }

        public void ExecuteOnMainThread(object context, Action action)
        {
            // If the context become null we log the fact that we wont execute
            // the call back. This should be better than trowing exception. That is. 
            // Context may become invalid (null) when closing the current activity
            if (context == null)
            {
                (new Handler(Looper.MainLooper)).Post(action);
            }
            else
            {
                if (context is Activity)
                {
                    Activity activity = context as Activity;
                    activity.RunOnUiThread(() => action());
                }
                else if (context is Fragment)
                {
                    Fragment fragment = context as Fragment;
                    if (fragment.Activity != null
                        && fragment.View != null)
                    {
                        fragment.Activity.RunOnUiThread(
                            () =>
                            {
                                if (fragment.Activity != null)
                                    action();
                            });
                    }
                }
                else
                {
                    Android.Util.Log.Debug("Core", "Unable to invoke on main thread, context is invalid!");
                }
            }
        }

        public void GetContextInfo(out string osVersion, out string appVersion, out string build)
        {
            osVersion = Android.OS.Build.VERSION.Release;

            var packageManager = Application.Context.PackageManager;
            var packageInfo = packageManager.GetPackageInfo(Application.Context.PackageName, 0);
            appVersion = packageInfo.VersionName;
            build = packageInfo.VersionCode.ToString();
        }

        public void OpenBrowser(string url)
        {
            Intent intent = new Intent(Intent.ActionView, Android.Net.Uri.Parse(url));
            intent.SetFlags(ActivityFlags.NewTask);

            try
            {
                Application.Context.StartActivity(intent);
            }
            catch (Exception ex)
            {
                /* Do Nothing */
            }
        }

        public void OpenPhoneCall(string phoneNumber)
        {
            Intent intent = new Intent(Intent.ActionDial);
            intent.SetData(Android.Net.Uri.Parse(String.Format("tel:{0}", phoneNumber)));
            intent.SetFlags(ActivityFlags.NewTask);

            try
            {
                Application.Context.StartActivity(intent);
            }
            catch (Exception ex)
            {
                /* Do Nothing */
            }
        }

        public void OpenStore(string appId)
        {
            string packageName = Application.Context.PackageName;

            try
            {
                Intent intent = new Intent(Intent.ActionView, Android.Net.Uri.Parse(String.Concat("market://details?id=", packageName)));
                Application.Context.StartActivity(intent);
            }
            catch (Android.Content.ActivityNotFoundException ex)
            {
                Intent intent = new Intent(Intent.ActionView, Android.Net.Uri.Parse(String.Concat("https://play.google.com/store/apps/details?id=", packageName)));
                Application.Context.StartActivity(intent);
            }
        }

        public void SendEmail(string[] toRecipients, string subject, string text = null, FileUri[] attachments = null)
        {
            Intent intent = new Intent(Intent.ActionSendMultiple);
            intent.SetType("message/rfc822");
            intent.PutExtra(Intent.ExtraEmail, toRecipients);
            intent.PutExtra(Intent.ExtraSubject, subject);
            intent.PutExtra(Intent.ExtraText, ((Java.Lang.ICharSequence)new Java.Lang.String(text ?? String.Empty)));

            if (attachments != null
                && attachments.Length > 0)
            {
                var files = new List<Android.Net.Uri>();
                var context = Android.App.Application.Context;

                files.AddRange(attachments.Select(x =>
                    FileProvider.GetUriForFile(context, $"{context.PackageName}.fileprovider", new Java.IO.File(x.AbsolutePath))));

                intent.PutParcelableArrayListExtra(Intent.ExtraStream, (IList<Android.OS.IParcelable>)files.ToArray());
            }

            Intent chooser = Intent.CreateChooser(intent, "Invio e-mail");
            chooser.SetFlags(ActivityFlags.NewTask);
            Application.Context.StartActivity(chooser);
        }
    }
}