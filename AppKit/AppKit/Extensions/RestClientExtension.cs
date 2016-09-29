namespace AdMaiora.AppKit.Services
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    using RestSharp.Portable;
    using RestSharp.Portable.HttpClient;    

    static class RestClientExtension
    {
        #region Public Methods

        public static async Task<IRestResponse> Execute(this RestClient client, TrackedRestRequest request, CancellationToken ct)
        {
            request.Timer.Reset();
            request.Timer.Start();

            try
            {
                request.Response = await client.Execute(request.Request, ct);
                request.Timer.Stop();
                return request.Response;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                request.Timer.Stop();
            }
        }

        public static async Task<IRestResponse<T>> Execute<T>(this RestClient client, TrackedRestRequest request, CancellationToken ct)
        {
            request.Timer.Reset();
            request.Timer.Start();

            try
            {
                request.Response = await client.Execute<T>(request.Request, ct);
                request.Timer.Stop();
                return (IRestResponse<T>)request.Response;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                request.Timer.Stop();
            }
        }

        #endregion
    }
}
