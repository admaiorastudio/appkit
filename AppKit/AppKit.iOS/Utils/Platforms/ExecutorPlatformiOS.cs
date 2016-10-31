namespace AdMaiora.AppKit.Utils
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.IO;

    using Foundation;
    using CoreFoundation;
    using MessageUI;
    using UIKit;

    using AdMaiora.AppKit.IO;

    public class ExecutorPlatformiOS : IExecutorPlatform
    {
        public void DebugOutput(string tag, string format, params object[] args)
        {
            if (args == null || args.Length == 0)
            {
                System.Diagnostics.Debug.WriteLine(String.Concat("[", tag, "] : ", format));
            }
            else
            {
                System.Diagnostics.Debug.WriteLine(String.Concat("[", tag, "] : ", format), args);
            }
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

        public void SendEmail(string[] toRecipients, string subject, string text = null, FileUri[] attachments = null)
        {
            if (!MFMailComposeViewController.CanSendMail)
                throw new InvalidOperationException("System can not send email.");
            
            MFMailComposeViewController mailController = new MFMailComposeViewController();
            mailController.SetToRecipients(toRecipients);
            mailController.SetSubject(subject);
            mailController.SetMessageBody(text, false);                                                                      
            mailController.Finished += (sender, e) => ((UIViewController)sender).DismissViewController(true, null);

            if (attachments != null
                && attachments.Length > 0)
            {
                attachments
                    .ToList()
                    .ForEach(a =>
                    {
                        mailController.AddAttachmentData(
                            NSData.FromUrl(a.ToNSUrl()),
                            "application/octet-stream",
                            Path.GetFileName(a.AbsolutePath));
                    });                    
            }

            UIApplication.SharedApplication
                .Windows[0]
                .RootViewController
                .PresentViewController(mailController, true, null);
        }
    }
}