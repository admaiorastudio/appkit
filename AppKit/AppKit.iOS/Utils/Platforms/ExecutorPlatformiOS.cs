namespace AdMaiora.AppKit.Utils
{
    using System;

    using Foundation;
    using MessageUI;
    using UIKit;

    public class ExecutorPlatformiOS : IExecutorPlatform
    {
        public void DebugOutput(string tag, string format, params object[] args)
        {
            System.Diagnostics.Debug.WriteLine(String.Concat("[", tag, "] : ", format), args);
        }

        public void ExecuteOnMainThread(object context, Action action)
        {
            UIApplication.SharedApplication.InvokeOnMainThread(() => action());
        }

        public void GetContextInfo(out string osVersion, out string appVersion, out string build)
        {
            osVersion = UIDevice.CurrentDevice.SystemVersion.ToString();

            appVersion = NSBundle.MainBundle.InfoDictionary["CFBundleShortVersionString"].ToString();
            build = NSBundle.MainBundle.InfoDictionary["CFBundleVersion"].ToString();
        }

        public void OpenBrowser(string url)
        {
            UIApplication.SharedApplication.OpenUrl(new NSUrl(url));
        }

        public void OpenPhoneCall(string phoneNumber)
        {
            UIApplication.SharedApplication.OpenUrl(new NSUrl(String.Format("tel://{0}", phoneNumber)));
        }

        public void OpenStore(string appId)
        {
            NSString iTunesLink = new NSString(String.Format("itms://itunes.apple.com/app/apple-store/id{0}?mt=8", appId));
            UIApplication.SharedApplication.OpenUrl(NSUrl.FromString(iTunesLink));
        }

        public void SendEmail(string[] toRecipients, string subject)
        {
            if (MFMailComposeViewController.CanSendMail)
            {
                MFMailComposeViewController mailController = new MFMailComposeViewController();
                mailController.SetSubject(subject);
                mailController.SetToRecipients(toRecipients);
                mailController.Finished += (sender, e) => ((UIViewController)sender).DismissViewController(true, null);
                UIApplication.SharedApplication.Windows[0].RootViewController.PresentViewController(mailController, true, null);
            }
            else
            {
                // Notify the user?
            }
        }
    }
}