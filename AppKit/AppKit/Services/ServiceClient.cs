﻿namespace AdMaiora.AppKit.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Threading;
    using System.Reflection;

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
        ApplicationJson
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

        private string _baseUrl;

        private double _defaultRequestTimeout;
        private string _defaultAccessTokenName;

        private string _accessToken;
        private DateTime? _accessTokenExpirationDate;

        private bool _handleHttpErrors;

        #endregion

        #region Constructors

        public ServiceClient(string baseUrl)
        {
            _baseUrl = baseUrl;
            _defaultRequestTimeout = 5;
            _defaultAccessTokenName = "Access-Token";
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
                if(!(Uri.TryCreate(value, UriKind.Absolute, out uriResult) 
                    && (uriResult.Scheme.ToLower() == "http" || uriResult.Scheme.ToLower() == "https")))
                {
                    throw new InvalidOperationException("You must set a valid base url. Only HTTP or HTTPS schemes are supported.");
                }

                _baseUrl = uriResult.AbsoluteUri;
            }
        }

        public double DefaultRequestTimeout
        {
            get
            {
                return _defaultRequestTimeout;
            }
            set
            {
                // No less then 5 seconds
                _defaultRequestTimeout = Math.Max(value, 5);
            }
        }

        public string DefaultAccessTokenName
        {
            get
            {
                return _defaultAccessTokenName;
            }
            set
            {
                _defaultAccessTokenName = value;
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
            client.Timeout = TimeSpan.FromSeconds(_defaultRequestTimeout);

            return client;
        }

        public RestRequest GetRestRequest(Method method, string resource, RequestContentType ct = RequestContentType.ApplicationJson, object parameters = null)
        {
            RestRequest request = new RestRequest(resource, method);

            if (this.IsAccessTokenValid)
                request.AddHeader(_defaultAccessTokenName, _accessToken);

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
