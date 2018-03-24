namespace AdMaiora.AppKit.Services
{    
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    using RestSharp;

    public static class RestClientExtension
    {
        #region Public Methods

        public static async Task<IRestResponse> Execute(this RestClient client, TrackedRestRequest request, CancellationToken ct)
        {
            request.Timer.Reset();
            request.Timer.Start();

            try
            {
                request.Response = await client.ExecuteTaskAsync(request.Request, ct);
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
                request.Response = await client.ExecuteTaskAsync<T>(request.Request, ct);
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
