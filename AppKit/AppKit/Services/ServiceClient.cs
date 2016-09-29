namespace AdMaiora.AppKit.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Threading;
    using System.Reflection;
    using System.Diagnostics;

    using RestSharp.Portable;
    using RestSharp.Portable.HttpClient;
    
    public enum RequestContentType
    {
        /// <summary>
        /// This means application/x-www-form-urlencoded
        /// </summary>
        ApplicationForm,
        /// <summary>
        /// This means application/json
        /// </summary>
        ApplicationJson,
        /// <summary>
        /// This means multipart/form-data, this allow file uploads BUT you can send form parameters only
        /// </summary>
        MultipartFormData
    }

    public class TrackedRestRequest
    {
        #region Constructors

        public TrackedRestRequest(RestRequest request, object payload)
        {
            this.Timer = new Stopwatch();
            this.Parameters = payload;
            this.Request = request;
        }

        #endregion

        #region Properties

        public Stopwatch Timer
        {
            get;
            private set;
        }

        public object Parameters
        {
            get;
            private set;
        }

        public RestRequest Request
        {
            get;
            private set;
        }

        public IRestResponse Response
        {
            get;
            set;
        }

        #endregion
    }

    public class ServiceClient
    {
        #region Inner Classes

        class RequestParameter
        {
            public string Name
            {
                get;
                set;
            }

            public object Value
            {
                get;
                set;
            }
        }

        #endregion

        #region Constants and Fields

        private IServiceClientPlatform _servicePlatform;

        private string _baseUrl;

        private double _requestTimeout;
        private string _accessTokenName;
        private string _multipartJsonField;

        private string _accessToken;
        private DateTime? _accessTokenExpirationDate;

        private bool _handleHttpErrors;

        #endregion

        #region Constructors

        public ServiceClient(IServiceClientPlatform servicePlatform, string baseUrl)
        {
            _servicePlatform = servicePlatform;

            _baseUrl = baseUrl;

            _requestTimeout = 5;
            _accessTokenName = "Authorization";
            _multipartJsonField = "json_body";
        }

        #endregion

        #region Properties

        public string BaseUrl
        {
            get
            {
                if (String.IsNullOrWhiteSpace(_baseUrl))
                    throw new InvalidOperationException("You must set a valid base url.");

                return _baseUrl;
            }
            set
            {
                Uri uriResult;
                if (!(Uri.TryCreate(value, UriKind.Absolute, out uriResult)
                    && (uriResult.Scheme.ToLower() == "http" || uriResult.Scheme.ToLower() == "https")))
                {
                    throw new InvalidOperationException("You must set a valid base url. Only HTTP or HTTPS schemes are supported.");
                }

                _baseUrl = uriResult.AbsoluteUri;
            }
        }

        public double RequestTimeout
        {
            get
            {
                return _requestTimeout;
            }
            set
            {
                // No less then 5 seconds
                _requestTimeout = Math.Max(value, 5);
            }
        }

        public string AccessToken
        {
            get
            {
                return _accessToken;
            }
            set
            {
                _accessToken = value;
            }
        }

        public string AccessTokenName
        {
            get
            {
                return _accessTokenName;
            }
            set
            {
                _accessTokenName = value;
            }
        }

        public DateTime? AccessTokenExpiration
        {
            get
            {
                return _accessTokenExpirationDate;
            }
            set
            {
                _accessTokenExpirationDate = value;
            }
        }

        public string MultipartJsonField
        {
            get
            {
                return _multipartJsonField;
            }
            set
            {
                _multipartJsonField = value;
            }
        }

        public bool HandleHttpErrors
        {
            get
            {
                return _handleHttpErrors;
            }
            set
            {
                _handleHttpErrors = value;
            }

        }

        public NetworkConnection NetworkConnection
        {
            get
            {
                return _servicePlatform.GetNetworkConnection();
            }
        }

        public bool IsAccessTokenValid
        {
            get
            {
                if (String.IsNullOrWhiteSpace(_accessToken))
                    return false;

                if (_accessTokenExpirationDate.GetValueOrDefault(DateTime.MinValue) <= DateTime.Now)
                    return false;

                return true;
            }
        }

        public bool IsNetworkAvailable
        {
            get
            {
                return _servicePlatform.IsNetworkAvailable();
            }
        }

        #endregion

        #region Request Methods

        public async Task<IRestResponse> Request(string resource, Method method,
            CancellationToken t = default(CancellationToken),
            RequestContentType ct = RequestContentType.ApplicationJson,
            object parameters = null)
        {
            return await Request(resource, method, t, null, null, ct, parameters);
        }

        public async Task<IRestResponse<T>> Request<T>(string resource, Method method,
            CancellationToken t = default(CancellationToken),
            RequestContentType ct = RequestContentType.ApplicationJson,
            object parameters = null)
        {
            return await Request<T>(resource, method, t, null, null, ct, parameters);
        }

        public async Task<IRestResponse> Request(string resource, Method method,
            CancellationToken t = default(CancellationToken),
            Action<RestClient> configureClient = null,
            Action<RestRequest> configureRequest = null,
            RequestContentType ct = RequestContentType.ApplicationJson,
            object parameters = null)
        {
            RestClient client = null;
            RestRequest request = null;

            await (new TaskFactory()).StartNew(
                () =>
                {
                    client = GetRestClient();
                    configureClient?.Invoke(client);

                    request = GetRestRequest(resource, method, ct, parameters);
                    configureRequest?.Invoke(request);

                });

            return await client.Execute(request, t);
        }

        public async Task<IRestResponse<T>> Request<T>(string resource, Method method,
            CancellationToken t = default(CancellationToken),
            Action<RestClient> configureClient = null,
            Action<RestRequest> configureRequest = null,
            RequestContentType ct = RequestContentType.ApplicationJson,
            object parameters = null)
        {
            RestClient client = null;
            RestRequest request = null;

            await (new TaskFactory()).StartNew(
                () =>
                {
                    client = GetRestClient();
                    configureClient?.Invoke(client);

                    request = GetRestRequest(resource, method, ct, parameters);
                    configureRequest?.Invoke(request);

                });


            return await client.Execute<T>(request, t);
        }

        public async Task<IRestResponse> TrackedRequest(string resource, Method method, Action<TrackedRestRequest> track,
            CancellationToken t = default(CancellationToken),
            RequestContentType ct = RequestContentType.ApplicationJson,
            object parameters = null)
        {
            return await TrackedRequest(resource, method, track, t, null, null, ct, parameters);
        }

        public async Task<IRestResponse<T>> TrackedRequest<T>(string resource, Method method, Action<TrackedRestRequest> track,
            CancellationToken t = default(CancellationToken),
            RequestContentType ct = RequestContentType.ApplicationJson,
            object parameters = null)
        {
            return await TrackedRequest<T>(resource, method, track, t, null, null, ct, parameters);
        }

        public async Task<IRestResponse> TrackedRequest(string resource, Method method, Action<TrackedRestRequest> track,
            CancellationToken t = default(CancellationToken),
            Action<RestClient> configureClient = null,
            Action<RestRequest> configureRequest = null,
            RequestContentType ct = RequestContentType.ApplicationJson,
            object parameters = null)
        {
            RestClient client = null;
            RestRequest request = null;

            await (new TaskFactory()).StartNew(
                () =>
                {
                    client = GetRestClient();
                    configureClient?.Invoke(client);

                    request = GetRestRequest(resource, method, ct, parameters);
                    configureRequest?.Invoke(request);

                });

            var trequest = new TrackedRestRequest(request, parameters);
            var response = await client.Execute(trequest, t);
            track?.Invoke(trequest);

            return response;
        }

        public async Task<IRestResponse<T>> TrackedRequest<T>(string resource, Method method, Action<TrackedRestRequest> track,
            CancellationToken t = default(CancellationToken),
            Action<RestClient> configureClient = null,
            Action<RestRequest> configureRequest = null,
            RequestContentType ct = RequestContentType.ApplicationJson,
            object parameters = null)
        {
            RestClient client = null;
            RestRequest request = null;

            await (new TaskFactory()).StartNew(
                () =>
                {
                    client = GetRestClient();
                    configureClient?.Invoke(client);

                    request = GetRestRequest(resource, method, ct, parameters);
                    configureRequest?.Invoke(request);

                });


            var trequest = new TrackedRestRequest(request, parameters);
            var response = await client.Execute<T>(trequest, t);
            track?.Invoke(trequest);

            return response;
        }

        #endregion

        #region Public Methods

        public void RefreshAccessToken(string accessToken, DateTime expirationDate)
        {
            _accessToken = accessToken;
            _accessTokenExpirationDate = expirationDate;
        }

        public void InvalidateAccessToken()
        {
            _accessToken = null;
            _accessTokenExpirationDate = null;
        }

        public RestClient GetRestClient()
        {
            RestClient client = new RestClient(this.BaseUrl);
            client.IgnoreResponseStatusCode = !_handleHttpErrors;
            client.Timeout = TimeSpan.FromSeconds(_requestTimeout);

            return client;
        }

        public RestRequest GetRestRequest(string resource, Method method, RequestContentType ct = RequestContentType.ApplicationJson, object parameters = null)
        {
            RestRequest request = new RestRequest(resource, method);

            if (this.IsAccessTokenValid)
                request.AddHeader(_accessTokenName, _accessToken);

            switch(ct)
            {
                case RequestContentType.ApplicationForm:

                    // Set content type
                    request.AddHeader("Content-Type", "application/x-www-form-urlencoded");

                    // Set parameters
                    if(parameters != null)
                    {
                        foreach (var p in GetParameters(parameters))
                            request.AddParameter(p.Name, p.Value);                               
                    }

                    break;

                case RequestContentType.ApplicationJson:

                    // Set content type
                    request.AddHeader("Content-Type", "application/json");

                    // Set parameters
                    if(parameters != null)
                    {
                        if(request.Method == Method.GET)
                        {
                            foreach (var p in GetParameters(parameters))
                                request.AddParameter(p.Name, p.Value);
                        }
                        else
                        {
                            request.AddBody(parameters);
                        }                        
                    }

                    break;

                case RequestContentType.MultipartFormData:

                    // We do not specify any any content type header
                    // letting RestSharp decide what to use

                    // We add parameters as a single object parameter
                    // letting RestSharp to do the right JSON serialization
                    if (parameters != null)                                            
                        request.AddParameter(_multipartJsonField, parameters, ParameterType.RequestBody, "application/json");                    

                    break;

            }            
            
            return request;
        }

        #endregion

        #region Methods

        private IEnumerable<RequestParameter> GetParameters(object parameters)
        {
            if (parameters != null)
            {
                Type type = parameters.GetType();
                var properties = type.GetRuntimeProperties();
                foreach (PropertyInfo prop in properties)
                {
                    object val = prop.GetValue(parameters, null);
                    if (val != null)
                        yield return new RequestParameter { Name = prop.Name, Value = val };
                }
            }
        }

        #endregion
    }
}
