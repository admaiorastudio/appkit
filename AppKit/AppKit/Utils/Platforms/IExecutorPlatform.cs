namespace AdMaiora.AppKit.Utils
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    using AdMaiora.AppKit.IO;

    public interface IExecutorPlatform
    {
        void OpenPhoneCall(string phoneNumber);

        void OpenBrowser(string url);

        void OpenStore(string appId = null);

        void SendEmail(string[] toRecipients, string subject, string text = null, FileUri[] attachments = null);

        void ExecuteOnMainThread(object context, Action action);

        void GetContextInfo(out string osVersion, out string appVersion, out string build);

        void DebugOutput(string tag, string format, params object[] args);
    }
}
