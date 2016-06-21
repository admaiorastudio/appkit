namespace AdMaiora.AppKit.Utils
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public interface IUtilityPlatform
    {
        void OpenPhoneCall(string phoneNumber);

        void OpenBrowser(string url);

        void OpenStore(string appId = null);

        void SendEmail(string[] toRecipients, string subject);

        void ExecuteOnMainThread(object context, Action action);

        void GetContextInfo(out string osVersion, out string appVersion, out string build);

        void DebugOutput(string tag, string format, params object[] args);
    }
}
