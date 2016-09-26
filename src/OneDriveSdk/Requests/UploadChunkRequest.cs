// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.OneDrive.Sdk
{
    using Microsoft.Graph;
    
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// The type UploadChunkRequest.
    /// </summary>
    public partial class UploadChunkRequest : BaseRequest, IUploadChunkRequest
    {
        /// <summary>
        /// Constructs a new UploadChunkRequest.
        /// </summary>
        /// <param name="requestUrl">The URL for the built request.</param>
        /// <param name="client">The <see cref="IBaseClient"/> for handling requests.</param>
        /// <param name="options">Query and header option name value pairs for the request.</param>
        public UploadChunkRequest(
            string sessionUrl,
            IBaseClient client,
            IEnumerable<Option> options)
            : base(sessionUrl, client, options)
        {
        }

        /// <summary>
        /// Uploads the chunk using PUT.
        /// </summary>
        /// <returns>The status of the upload.</returns>
        public Task PutAsync(byte[] bytes, int begin, int end, int total)
        {
            throw new NotImplementedException();
        }
    }
}
