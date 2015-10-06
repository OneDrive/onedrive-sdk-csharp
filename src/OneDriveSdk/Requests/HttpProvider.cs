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
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Threading.Tasks;

    /// <summary>
    /// An <see cref="IHttpProvider"/> implementation using standard .NET libraries.
    /// </summary>
    public class HttpProvider : IHttpProvider, IDisposable
    {
        private HttpClientHandler httpClientHandler;
        private const int maxRedirects = 5;

        internal HttpClient httpClient;

        /// <summary>
        /// Constructs a new <see cref="HttpProvider"/>.
        /// </summary>
        /// <param name="serializer">A serializer for serializing and deserializing JSON objects.</param>
        public HttpProvider(ISerializer serializer = null)
        {
            this.Serializer = serializer ?? new Serializer();

            // We cannot change the AllowAutoRedirect property on the handler after the initial request is made.
            // In order to reuse the same client objects and not spin up a new one with defaults every time
            // we'll create 2 client instances and use whichever one we need to use at the time.

            this.httpClientHandler = new HttpClientHandler
            {
                AllowAutoRedirect = false,
            };

            this.httpClient = new HttpClient(this.httpClientHandler);

            // Disable request caching
            this.httpClient.DefaultRequestHeaders.CacheControl = new CacheControlHeaderValue
            {
                NoCache = true,
                NoStore = true
            };
        }

        /// <summary>
        /// Gets a serializer for serializing and deserializing JSON objects.
        /// </summary>
        public ISerializer Serializer { get; private set; }

        /// <summary>
        /// Disposes the HttpClient and HttpClientHandler instances.
        /// </summary>
        public void Dispose()
        {
            if (this.httpClient != null)
            {
                this.httpClient.Dispose();
            }

            if (this.httpClientHandler != null)
            {
                this.httpClientHandler.Dispose();
            }
        }

        /// <summary>
        /// Sends the request.
        /// </summary>
        /// <param name="request">The <see cref="HttpRequestMessage"/> to send.</param>
        /// <returns>The <see cref="HttpResponseMessage"/>.</returns>
        public async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request)
        {
            var initialResponse = await this.httpClient.SendAsync(request);
            var response = initialResponse;

            if (this.IsRedirect(initialResponse.StatusCode))
            {
                response = await this.HandleRedirect(initialResponse);

                if (response == null)
                {
                    throw new OneDriveException(
                        new Error
                        {
                            Code = OneDriveErrorCode.GeneralException.ToString(),
                            Message = "Location header not present in redirection response."
                        });
                }
            }

            if (!response.IsSuccessStatusCode && !this.IsRedirect(response.StatusCode))
            {
                using (response)
                {
                    var error = await this.ConvertErrorResponseAsync(response);

                    if (error != null && error.Error != null)
                    {
                        throw new OneDriveException(error.Error);
                    }

                    if (response != null && response.StatusCode == HttpStatusCode.NotFound)
                    {
                        throw new OneDriveException(new Error { Code = OneDriveErrorCode.ItemNotFound.ToString() });
                    }

                    throw new OneDriveException(
                        new Error
                        {
                            Code = OneDriveErrorCode.GeneralException.ToString(),
                            Message = "Unexpected exception returned from the service."
                        });
                }
            }

            return response;
        }

        internal async Task<HttpResponseMessage> HandleRedirect(HttpResponseMessage initialResponse, int redirectCount = 0)
        {
            if (initialResponse.Headers.Location == null)
            {
                return null;
            }

            using (initialResponse)
            using (var redirectRequest = new HttpRequestMessage(initialResponse.RequestMessage.Method, initialResponse.Headers.Location))
            {
                // Preserve headers for the next request
                foreach (var header in initialResponse.RequestMessage.Headers)
                {
                    redirectRequest.Headers.Add(header.Key, header.Value);
                }

                var response = await this.httpClient.SendAsync(redirectRequest);

                if (this.IsRedirect(response.StatusCode))
                {
                    if (++redirectCount > HttpProvider.maxRedirects)
                    {
                        throw new OneDriveException(
                            new Error
                            {
                                Code = OneDriveErrorCode.TooManyRedirects.ToString(),
                                Message = string.Format("More than {0} redirects encountered while sending the request.", HttpProvider.maxRedirects)
                            });
                    }

                    return await this.HandleRedirect(response, redirectCount);
                }

                return response;
            }
        }

        /// <summary>
        /// Converts the <see cref="HttpRequestException"/> into an <see cref="ErrorResponse"/> object;
        /// </summary>
        /// <param name="response">The <see cref="WebResponse"/> to convert.</param>
        /// <returns>The <see cref="ErrorResponse"/> object.</returns>
        private async Task<ErrorResponse> ConvertErrorResponseAsync(HttpResponseMessage response)
        {
            try
            {
                using (var responseStream = await response.Content.ReadAsStreamAsync())
                {
                    return this.Serializer.DeserializeObject<ErrorResponse>(responseStream);
                }
            }
            catch (Exception)
            {
                // If there's an exception deserializing the error response return null and throw a generic
                // OneDriveException later.
                return null;
            }
        }

        private bool IsRedirect(HttpStatusCode statusCode)
        {
            return (int)statusCode >= 300 && (int)statusCode < 400 && statusCode != HttpStatusCode.NotModified;
        }
    }
}
