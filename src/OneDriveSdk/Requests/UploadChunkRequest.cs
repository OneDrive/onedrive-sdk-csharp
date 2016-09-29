// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

using System.Net;
using System.Net.Http.Headers;

namespace Microsoft.OneDrive.Sdk
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;

    using Microsoft.Graph;
    
    /// <summary>
    /// The type UploadChunkRequest.
    /// </summary>
    public partial class UploadChunkRequest : BaseRequest, IUploadChunkRequest
    {
        public long RangeBegin { get; private set; }
        public long RangeEnd { get; private set; }
        public long TotalSessionLength { get; private set; }
        public int RangeLength => (int)(this.RangeEnd - this.RangeBegin + 1);

        /// <summary>
        /// Constructs a new UploadChunkRequest.
        /// </summary>
        /// <param name="requestUrl">The URL for the built request.</param>
        /// <param name="client">The <see cref="IBaseClient"/> for handling requests.</param>
        /// <param name="options">Query and header option name value pairs for the request.</param>
        public UploadChunkRequest(
            string sessionUrl,
            IBaseClient client,
            IEnumerable<Option> options,
            long rangeBegin,
            long rangeEnd,
            long totalSessionLength)
            : base(sessionUrl, client, options)
        {
            this.RangeBegin = rangeBegin;
            this.RangeEnd = rangeEnd;
            this.TotalSessionLength = totalSessionLength;
        }

        /// <summary>
        /// Uploads the chunk using PUT.
        /// </summary>
        /// <returns>The status of the upload.</returns>
        public Task<UploadChunkResult> PutAsync(Stream stream)
        {
            return this.PutAsync(stream, CancellationToken.None);
        }

        /// <summary>
        /// Uploads the chunk using PUT.
        /// </summary>
        /// <param name="stream">Stream of data to be sent in the request. Length must be equal to the length
        /// of this chunk (as defined by this.RangeLength)</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>The status of the upload. If UploadSession.AdditionalData.ContainsKey("successResponse")
        /// is true, then the item has completed, and the value is the created item from the server.</returns>
        public async Task<UploadChunkResult> PutAsync(Stream stream, CancellationToken cancellationToken)
        {
            this.Method = "PUT";
            using (var response = await this.SendRequestAsync(stream, cancellationToken).ConfigureAwait(false))
            {
                if (response.Content != null)
                {
                    var responseString = await response.Content.ReadAsStringAsync();
                    
                    if (response.StatusCode == HttpStatusCode.Created || response.StatusCode == HttpStatusCode.OK)
                    {
                        return new UploadChunkResult
                            {
                                ItemResponse =
                                    this.Client.HttpProvider.Serializer.DeserializeObject<Item>(responseString)
                            };
                    }
                    else
                    {
                        return new UploadChunkResult
                            {
                                UploadSession =
                                    this.Client.HttpProvider.Serializer.DeserializeObject<UploadSession>(responseString)
                            };
                    }
                }

                return default(UploadChunkResult);
            }
        }

        private async Task<HttpResponseMessage> SendRequestAsync(
            Stream stream,
            CancellationToken cancellationToken,
            HttpCompletionOption completionOption = HttpCompletionOption.ResponseContentRead)
        {
            if (string.IsNullOrEmpty(this.RequestUrl))
            {
                throw new ArgumentNullException(nameof(this.RequestUrl), "Session Upload URL cannot be null or empty.");
            }

            if (this.Client.AuthenticationProvider == null)
            {
                throw new ArgumentNullException(nameof(this.Client.AuthenticationProvider), "Client.AuthenticationProvider must not be null.");
            }

            using (var request = this.GetHttpRequestMessage())
            {
                await this.Client.AuthenticationProvider.AuthenticateRequestAsync(request).ConfigureAwait(false);

                request.Content = new StreamContent(stream);
                request.Content.Headers.ContentRange =
                    ContentRangeHeaderValue.Parse($"bytes {this.RangeBegin}-{this.RangeEnd}/{this.TotalSessionLength}");
                request.Content.Headers.ContentLength = this.RangeLength;

                return await this.Client.HttpProvider.SendAsync(request, completionOption, cancellationToken).ConfigureAwait(false);
            }
        }
    }

    public class UploadChunkResult
    {
        public UploadSession UploadSession;
        public Item ItemResponse;
        public bool UploadSucceeded => this.ItemResponse != null;
    }
}
