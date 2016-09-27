// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

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
        public int RangeBegin { get; private set; }
        public int RangeEnd { get; private set; }
        public int TotalSessionLength { get; private set; }

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
            int rangeBegin,
            int rangeEnd,
            int totalSessionLength)
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
        public Task<UploadSession> PutAsync(Stream stream)
        {
            return this.SendAsync<UploadSession>(stream, CancellationToken.None);
        }

        /// <summary>
        /// Uploads the chunk using PUT.
        /// </summary>
        /// <returns>The status of the upload.</returns>
        public Task<UploadSession> PutAsync(Stream stream, CancellationToken cancellationToken)
        {
            this.Method = "PUT";
            this.Headers.Add(new HeaderOption("Content-Range",
                $"bytes {this.RangeBegin}-{this.RangeEnd}/{this.TotalSessionLength}"));
            return this.SendAsync<UploadSession>(stream, cancellationToken);
        }
    }
}
