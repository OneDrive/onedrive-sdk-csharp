// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.OneDrive.Sdk.Helpers
{
    using Microsoft.Graph;
    using Microsoft.OneDrive.Sdk.Models;

    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading.Tasks;

    class UploadSessionHelper
    {
        private const int MaxChunkSize = 10 * 1024 * 1024;

        public UploadSession session { get; private set; }
        private Stream uploadStream;
        private int length;
        private List<Tuple<int, int>> rangesRemaining;
        private IBaseClient client;
         
        public UploadSessionHelper(UploadSession session, IBaseClient client, Stream uploadStream, int streamLength)
        {
            this.session = session;
            this.client = client;
            this.uploadStream = uploadStream;
            this.length = streamLength;
            this.rangesRemaining = new List<Tuple<int, int>> { new Tuple<int, int> (0, streamLength) };
        }

        public Task CancelAsync()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<IUploadChunkRequest> GetChunkRequests(IEnumerable<Option> options = null)
        {
            foreach (var range in this.rangesRemaining)
            {
                yield return new UploadChunkRequest(
                    this.session.UploadUrl,
                    this.client,
                    options);
            }
        } 
    }
}
