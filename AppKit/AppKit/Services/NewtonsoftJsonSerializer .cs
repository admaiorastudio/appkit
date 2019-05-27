//Copyright(c) Philipp Wagner.All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AdMaiora.AppKit.Services.Serialization
{
    using Newtonsoft.Json;
    using System.IO;
    using RestSharp.Serializers;
    using RestSharp.Deserializers;


    public class NewtonsoftJsonSerializer : ISerializer, IDeserializer
    {
        private Newtonsoft.Json.JsonSerializer _serializer;

        public NewtonsoftJsonSerializer(Newtonsoft.Json.JsonSerializer serializer)
        {
            _serializer = serializer;
        }

        public string ContentType
        {
            get { return "application/json"; } // Probably used for Serialization?
            set { }
        }

        public string DateFormat { get; set; }

        public string Namespace { get; set; }

        public string RootElement { get; set; }

        public string Serialize(object obj)
        {
            using (var stringWriter = new StringWriter())
            {
                using (var jsonTextWriter = new JsonTextWriter(stringWriter))
                {
                    _serializer.Serialize(jsonTextWriter, obj);

                    return stringWriter.ToString();
                }
            }
        }

        public T Deserialize<T>(RestSharp.IRestResponse response)
        {
            var content = response.Content;

            using (var stringReader = new StringReader(content))
            {
                using (var jsonTextReader = new JsonTextReader(stringReader))
                {
                    return _serializer.Deserialize<T>(jsonTextReader);
                }
            }
        }

        public static NewtonsoftJsonSerializer Default
        {
            get
            {
                return new NewtonsoftJsonSerializer(new Newtonsoft.Json.JsonSerializer()
                {
                    NullValueHandling = NullValueHandling.Ignore,
                });
            }
        }
    }
}