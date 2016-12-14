namespace AdMaiora.AppKit.Utils
{
    using System;
    using System.Diagnostics;
    using System.Threading;
    using System.Threading.Tasks;

    using AdMaiora.AppKit.IO;

    public class Executor
    {
        #region Constants and Fields

        private IExecutorPlatform _executorPlatform;

        #endregion

        #region Constructors

        public Executor(IExecutorPlatform utilityPlatform)
        {
            _executorPlatform = utilityPlatform;
        }

        #endregion

        #region Public Methods

        public void OpenPhoneCall(string phoneNumber)
        {
            phoneNumber = phoneNumber.Replace(" ", String.Empty);
            _executorPlatform.OpenPhoneCall(phoneNumber);
        }

        public void OpenBrowser(string url)
        {
            _executorPlatform.OpenBrowser(url);
        }

        public void OpenStore(string appId = null)
        {
            _executorPlatform.OpenStore(appId);
        }

        public void SendEmail(string[] toRecipients, string subject, string text = null, FileUri[] attachments = null)
        {
            _executorPlatform.SendEmail(toRecipients, subject, text, attachments);
        }

        [Obsolete("This method is obsolete, you should use the ExecuteOnMainThread(Action action) method which no longer needs context object.")]
        public void ExecuteOnMainThread(object context, Action action)
        {
            _executorPlatform.ExecuteOnMainThread(context, action);
        }

        public void ExecuteOnMainThread(Action action)
        {
            _executorPlatform.ExecuteOnMainThread(null, action);
        }

        public async Task ExecuteOnAsyncTask<TResult>(CancellationToken ctk, Func<TResult> function, Action<TResult> whenFinished = null, Action<Exception> whenException = null)
        {
            TResult result = await Task.Factory
                .StartNew<TResult>(() =>
                {
                    TResult r = default(TResult);

                    if (ctk != CancellationToken.None
                        && ctk.IsCancellationRequested)
                    {
                        return r;
                    }

                    if (function != null)
                        r = function();

                    return r;
                })
                .ContinueWith<TResult>((t) =>
                {
                    if (t.Exception != null)
                    {
                        DebugOutput(t.Exception.ToString());

                        if (ctk != CancellationToken.None
                            && ctk.IsCancellationRequested)
                        {
                            return default(TResult);
                        }

                        if (whenException != null)
                            whenException(t.Exception);

#if DEBUG
                        Debugger.Break();
#endif
                    }

                    return t.Result;
                });

            if (ctk != CancellationToken.None
                && ctk.IsCancellationRequested)
            {
                return;
            }

            if (whenFinished != null)
                whenFinished(result);
        }

        public async Task ExecuteOnAsyncTask(CancellationToken ctk, Action action, Action whenFinished = null, Action<Exception> whenException = null)
        {
            await Task.Factory
                .StartNew(() =>
                {
                    if (ctk != CancellationToken.None
                        && ctk.IsCancellationRequested)
                    {
                        return;
                    }

                    if (action != null)
                        action();
                })
                .ContinueWith((t) =>
                {
                    if (t.Exception != null)
                    {
                        DebugOutput(t.Exception.ToString());

                        if (ctk != CancellationToken.None
                            && ctk.IsCancellationRequested)
                        {
                            return;
                        }

                        if (whenException != null)
                            whenException(t.Exception);


#if DEBUG
                        Debugger.Break();
#endif
                    }
                });

            if (ctk != CancellationToken.None
                && ctk.IsCancellationRequested)
            {
                return;
            }

            if (whenFinished != null)
                whenFinished();
        }

        public async Task ExecuteOnAsyncTaskDelayed(int millisecondsDelay, CancellationToken ctk, Action action, Action whenFinished = null, Action<Exception> whenException = null)
        {
            await Task.Delay(millisecondsDelay)
                .ContinueWith((t) =>
                {
                    if (ctk != CancellationToken.None
                        && ctk.IsCancellationRequested)
                    {
                        return;
                    }

                    if (action != null)
                        action();
                })
                .ContinueWith((t) =>
                {
                    if (t.Exception != null)
                    {
                        DebugOutput(t.Exception.ToString());

                        if (ctk != CancellationToken.None
                            && ctk.IsCancellationRequested)
                        {
                            return;
                        }

                        if (whenException != null)
                            whenException(t.Exception);


#if DEBUG
                        Debugger.Break();
#endif
                    }
                });

            if (ctk != CancellationToken.None
                && ctk.IsCancellationRequested)
            {
                return;
            }

            if (whenFinished != null)
                whenFinished();
        }

        public async Task ExecuteDelayedAction(int millisecondsDelay, CancellationToken ctk, Action action = null, Action<Exception> whenException = null)
        {
            await Task.Delay(millisecondsDelay)
                .ContinueWith((t) =>
                {
                    // Once waited we are done
                })
                .ContinueWith((t) =>
                {
                    if (t.Exception != null)
                    {
                        DebugOutput(t.Exception.ToString());

                        if (ctk != CancellationToken.None
                            && ctk.IsCancellationRequested)
                        {
                            return;
                        }

                        if (whenException != null)
                            whenException(t.Exception);


#if DEBUG
                        Debugger.Break();
#endif
                    }
                });

            if (ctk != CancellationToken.None
                && ctk.IsCancellationRequested)
            {
                return;
            }

            // Execute on the main thread
            if (action != null)
                action();
        }
        
        public void GetContextInfo(out string osVersion, out string appVersion, out string build)
        {
            _executorPlatform.GetContextInfo(out osVersion, out appVersion, out build);
        }

        public void DebugOutput(string tag, string format, params object[] args)
        {
            _executorPlatform.DebugOutput(tag, format, args);
        }

        public void DebugOutput(string message)
        {
            DebugOutput("AppKit", message);
        }

        #endregion
    }
}