// ------------------------------------------------------------------------------
//  Copyright (c) 2015 Microsoft Corporation
// 
//  Permission is hereby granted, free of charge, to any person obtaining a copy
//  of this software and associated documentation files (the "Software"), to deal
//  in the Software without restriction, including without limitation the rights
//  to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//  copies of the Software, and to permit persons to whom the Software is
//  furnished to do so, subject to the following conditions:
// 
//  The above copyright notice and this permission notice shall be included in
//  all copies or substantial portions of the Software.
// 
//  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//  OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
//  THE SOFTWARE.
// ------------------------------------------------------------------------------

namespace Microsoft.OneDrive.Sdk
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Reflection;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    /// The base request class.
    /// </summary>
    public class BaseRequest : IBaseRequest
    {
        private readonly string sdkVersionHeaderValue;

        /// <summary>
        /// Constructs a new <see cref="BaseRequest"/>.
        /// </summary>
        /// <param name="requestUrl">The URL for the request.</param>
        /// <param name="client">The <see cref="IBaseClient"/> for handling requests.</param>
        /// <param name="options">The header and query options for the request.</param>
        public BaseRequest(
            string requestUrl,
            IBaseClient client,
            IEnumerable<Option> options = null)
        {
            this.Method = "GET";
            this.Client = client;
            this.Headers = new List<HeaderOption>();
            this.QueryOptions = new List<QueryOption>();

            this.RequestUrl = this.InitializeUrl(requestUrl);

            if (options != null)
            {
                var headerOptions = options.OfType<HeaderOption>();
                if (headerOptions != null)
                {
                    ((List<HeaderOption>)this.Headers).AddRange(headerOptions);
                }

                var queryOptions = options.OfType<QueryOption>();
                if (queryOptions != null)
                {
                    ((List<QueryOption>)this.QueryOptions).AddRange(queryOptions);
                }
            }

            this.sdkVersionHeaderValue = string.Format(
                Constants.Headers.SdkVersionHeaderValue,
                this.GetType().GetTypeInfo().Assembly.GetName().Version);
        }

        /// <summary>
        /// Gets or sets the content type for the request.
        /// </summary>
        public string ContentType { get; set; }

        /// <summary>
        /// Gets the <see cref="HeaderOption"/> collection for the request.
        /// </summary>
        public IList<HeaderOption> Headers { get; private set; }

        /// <summary>
        /// Gets the <see cref="IBaseClient"/> for handling requests.
        /// </summary>
        public IBaseClient Client { get; private set; }

        /// <summary>
        /// Gets or sets the HTTP method string for the request.
        /// </summary>
        public string Method { get; set; }

        /// <summary>
        /// Gets the URL for the request, without query string.
        /// </summary>
        public string RequestUrl { get; private set; }

        /// <summary>
        /// Gets the <see cref="QueryOption"/> collection for the request.
        /// </summary>
        internal IList<QueryOption> QueryOptions { get; private set; }

        /// <summary>
        /// Sends the request.
        /// </summary>
        /// <param name="serializableObject">The serializable object to send.</param>
        /// <returns>The task to await.</returns>
        public async Task SendAsync(object serializableObject)
        {
            using (var response = await this.SendRequestAsync(serializableObject))
            {
            }
        }

        /// <summary>
        /// Sends the request.
        /// </summary>
        /// <typeparam name="T">The expected reponse object type for deserialization.</typeparam>
        /// <param name="serializableObject">The serializable object to send.</param>
        /// <returns>The deserialized response object.</returns>
        public async Task<T> SendAsync<T>(object serializableObject)
        {
            using (var response = await this.SendRequestAsync(serializableObject))
            {
                if (response.Content != null)
                {
                    var responseString = await response.Content.ReadAsStringAsync();
                    return this.Client.HttpProvider.Serializer.DeserializeObject<T>(responseString);
                }

                return default(T);
            }
        }

        /// <summary>
        /// Sends the request.
        /// </summary>
        /// <typeparam name="T">The expected reponse object type for deserialization.</typeparam>
        /// <param name="serializableObject">The serializable object to send.</param>
        /// <returns>The stream.</returns>
        public async Task<Stream> SendStreamRequestAsync(object serializableObject)
        {
            var response = await this.SendRequestAsync(serializableObject);
            return await response.Content.ReadAsStreamAsync();
        }

        /// <summary>
        /// Sends the request.
        /// </summary>
        /// <typeparam name="T">The expected reponse object type for deserialization.</typeparam>
        /// <param name="serializableObject">The serializable object to send.</param>
        /// <returns>The <see cref="WebResponse"/> object.</returns>
        public async Task<HttpResponseMessage> SendRequestAsync(object serializableObject)
        {
            // We will generate a new auth token later if that isn't set on the client, so not calling
            // IsAuthenticated. Instead, verify the service info and base URL are initialized to make sure
            // AuthenticateAsync has previously been called on the client.
            if (this.Client.ServiceInfo == null || string.IsNullOrEmpty(this.Client.BaseUrl))
            {
                throw new OneDriveException(
                    new Error
                    {
                        Code = OneDriveErrorCode.InvalidRequest.ToString(),
                        Message = "The client must be authenticated before sending a request.",
                    });
            }

            using (var request = this.GetHttpRequestMessage())
            {
                await this.AuthenticateRequestAsync(request);

                if (serializableObject != null)
                {
                    var inputStream = serializableObject as Stream;

                    if (inputStream != null)
                    {
                        request.Content = new StreamContent(inputStream);
                    }
                    else
                    {
                        request.Content = new StringContent(this.Client.HttpProvider.Serializer.SerializeObject(serializableObject));
                    }

                    if (!string.IsNullOrEmpty(this.ContentType))
                    {
                        request.Content.Headers.ContentType = new MediaTypeHeaderValue(this.ContentType);
                    }
                }

                return await this.Client.HttpProvider.SendAsync(request);
            }
        }

        /// <summary>
        /// Gets the <see cref="WebRequest"/> representation of the request.
        /// </summary>
        /// <returns>The <see cref="WebRequest"/> representation of the request.</returns>
        public HttpRequestMessage GetHttpRequestMessage()
        {
            var queryString = this.BuildQueryString();
            var request = new HttpRequestMessage(new HttpMethod(this.Method), this.RequestUrl + queryString);

            this.AddHeadersToRequest(request);

            return request;
        }

        /// <summary>
        /// Adds all of the headers from the header collection to the request.
        /// </summary>
        /// <param name="request">The <see cref="WebRequest"/> representation of the request.</param>
        private void AddHeadersToRequest(HttpRequestMessage request)
        {
            if (this.Headers != null)
            {
                foreach (var header in this.Headers)
                {
                    request.Headers.Add(header.Name, header.Value);
                }
            }

            // Append SDK version header for telemetry
            request.Headers.Add(
                this.Client.ClientType == ClientType.Business
                    ? Constants.Headers.BusinessSdkVersionHeaderName
                    : Constants.Headers.ConsumerSdkVersionHeaderName,
                this.sdkVersionHeaderValue);
        }

        /// <summary>
        /// Adds the authentication header to the request.
        /// </summary>
        /// <param name="request">The <see cref="WebRequest"/> representation of the request.</param>
        /// <returns>The async task.</returns>
        private async Task AuthenticateRequestAsync(HttpRequestMessage request)
        {
            await this.Client.ServiceInfo.AuthenticationProvider.AppendAuthHeaderAsync(request);
        }

        /// <summary>
        /// Builds the query string for the request from the query option collection.
        /// </summary>
        /// <returns>The constructed query string.</returns>
        private string BuildQueryString()
        {
            if (this.QueryOptions != null)
            {
                var stringBuilder = new StringBuilder();

                foreach (var queryOption in this.QueryOptions)
                {
                    if (stringBuilder.Length == 0)
                    {
                        stringBuilder.AppendFormat("?{0}={1}", queryOption.Name, queryOption.Value);
                    }
                    else
                    {
                        stringBuilder.AppendFormat("&{0}={1}", queryOption.Name, queryOption.Value);
                    }
                }

                return stringBuilder.ToString();
            }

            return null;
        }

        /// <summary>
        /// Initializes the request URL for the request, breaking it into query options and base URL.
        /// </summary>
        /// <param name="requestUrl">The request URL.</param>
        /// <returns>The request URL minus query string.</returns>
        private string InitializeUrl(string requestUrl)
        {
            var uri = new Uri(requestUrl);
            
            if (!string.IsNullOrEmpty(uri.Query))
            {
                var queryString = uri.Query;
                if (queryString[0] == '?')
                {
                    queryString = queryString.Substring(1);
                }

                var queryOptions = queryString.Split('&').Select(
                        queryValue =>
                        {
                            var segments = queryValue.Split('=');
                            return new QueryOption(
                                WebUtility.UrlDecode(segments[0]),
                                segments.Length > 1 ? WebUtility.UrlDecode(segments[1]) : string.Empty);
                        });

                foreach(var queryOption in queryOptions)
                {
                    this.QueryOptions.Add(queryOption);
                }
            }

            return new UriBuilder(uri) { Query = string.Empty }.ToString();
        }
    }
}
