// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.OneDrive.Sdk
{
    using System;
    using System.IO;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    
    using Microsoft.Graph;

    /// <summary>
    /// The interface IUploadChunkRequest.
    /// </summary>
    public partial interface IUploadChunkRequest : IBaseRequest
    {
        /// <summary>
        /// Puts the specified Chunk.
        /// </summary>
        /// <returns>The task to await.</returns>
        Task<UploadChunkResult> PutAsync(Stream stream);
    }
}
