// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.OneDrive.Sdk.Helpers
{
    using Microsoft.Graph;

    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    public class UploadSessionHelper
    {
        private const int MaxChunkSize = 10 * 1024 * 1024;

        public UploadSession Session { get; private set; }
        private Stream uploadStream;
        private int length;
        private List<Tuple<int, int>> rangesRemaining;
        private IBaseClient client;
         
        public UploadSessionHelper(UploadSession session, IBaseClient client, Stream uploadStream, int streamLength)
        {
            if (!uploadStream.CanRead || !uploadStream.CanSeek)
            {
                throw new ArgumentException("Must provide stream that can read and seek");
            }

            this.Session = session;
            this.client = client;
            this.uploadStream = uploadStream;
            this.length = streamLength;
            this.rangesRemaining = new List<Tuple<int, int>> { new Tuple<int, int> (0, streamLength - 1) };
        }

        public bool IsComplete => this.rangesRemaining.Count == 0;

        public IEnumerable<UploadChunkRequest> GetUploadChunkRequests(IEnumerable<Option> options = null)
        {
            foreach (var range in this.rangesRemaining)
            {
                var currentRangeBegins = range.Item1;

                while (currentRangeBegins <= range.Item2)
                {
                    var nextChunkSize = NextChunkSize(currentRangeBegins, range.Item2);
                    var uploadRequest = new UploadChunkRequest(
                        this.Session.UploadUrl,
                        this.client,
                        options,
                        currentRangeBegins,
                        currentRangeBegins + nextChunkSize - 1,
                        this.length);
                    
                    yield return uploadRequest;

                    currentRangeBegins += nextChunkSize;
                }
            }
        }

        /// <summary>
        /// Get the status of the session.
        /// </summary>
        /// <returns>UploadSession returned by the server.</returns>
        public async Task<UploadSession> UpdateSessionStatusAsync()
        {
            var request = new UploadSessionRequest(this.Session, this.client, null);
            var newSession = await request.GetAsync();
            
            // nextExpectedRanges: https://dev.onedrive.com/items/upload_large_files.htm
            // Sample: ["12345-55232","77829-99375"]
            // Also, second number in range can be blank, which means 'until the end'
            var newRangesRemaining = new List<Tuple<int, int>>();
            foreach (var range in newSession.NextExpectedRanges)
            {
                var rangeSpecifiers = range.Split('-');
                newRangesRemaining.Add(new Tuple<int, int>(
                    int.Parse(rangeSpecifiers[0]),
                    string.IsNullOrEmpty(rangeSpecifiers[1])
                        ? this.length - 1
                        : int.Parse(rangeSpecifiers[1])));
            }

            this.rangesRemaining = newRangesRemaining;
            newSession.UploadUrl = this.Session.UploadUrl; // Sometimes the UploadUrl is not returned
            this.Session = newSession;
            return newSession;
        }

        public async Task DeleteSession()
        {
            var request = new UploadSessionRequest(this.Session, this.client, null);
            await request.DeleteAsync();
        }

        /// <summary>
        /// Upload the whole session.
        /// </summary>
        /// <param name="maxTries">Number of times to retry a given chunk request before giving up.</param>
        /// <returns></returns>
        public async Task UploadAsync(int maxTries = 3, IEnumerable<Option> options = null)
        {
            var uploadTries = 0;
            
            while (uploadTries < maxTries && !this.IsComplete)
            {
                var chunkRequests = this.GetUploadChunkRequests(options);

                foreach (var request in chunkRequests)
                {
                    var tries = 0;
                    var stream = new MemoryStream(request.RangeEnd - request.RangeBegin + 1);
                    await this.uploadStream.CopyToAsync(stream);

                    while (tries < 2) // Retry a given request only once
                    {
                        try
                        {
                            await request.PutAsync(stream);
                        }
                        catch (ServiceException exception)
                        {
                            if (exception.IsMatch("generalException"))
                            {
                                // Swallow and retry
                                tries += 1;
                            }
                            else if (exception.IsMatch("timeout"))
                            {
                                // Don't waste a retry on a timeout
                            }
                            else
                            {
                                throw;
                            }
                        }
                    }
                }

                await this.UpdateSessionStatusAsync();
                uploadTries += 1;
                if (!this.IsComplete)
                {
                    await Task.Delay(1000 * uploadTries * uploadTries);
                }
            }

            if (!this.IsComplete)
            {
                throw new TaskCanceledException("Task was cancelled because upload failed too many times.");
            }
        }

        private static int NextChunkSize(int rangeBegin, int rangeEnd)
        {
            return (rangeEnd - rangeBegin) > MaxChunkSize
                ? MaxChunkSize
                : rangeEnd - rangeBegin + 1;
        }
    }

    public class UploadSessionRequest : BaseRequest
    {
        private readonly UploadSession session;

        public UploadSessionRequest(UploadSession session, IBaseClient client, IEnumerable<Option> options)
            :base(session.UploadUrl, client, options)
        {
            this.session = session;
        }

        /// <summary>
        /// Deletes the specified Session
        /// </summary>
        /// <returns>The task to await.</returns>
        public Task DeleteAsync()
        {
            return this.DeleteAsync(CancellationToken.None);
        }

        /// <summary>
        /// Deletes the specified Session
        /// </summary>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> for the request.</param>
        /// <returns>The task to await.</returns>
        public async Task DeleteAsync(CancellationToken cancellationToken)
        {
            this.Method = "DELETE";
            await this.SendAsync<UploadSession>(null, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Gets the specified UploadSession.
        /// </summary>
        /// <returns>The Item.</returns>
        public Task<UploadSession> GetAsync()
        {
            return this.GetAsync(CancellationToken.None);
        }

        /// <summary>
        /// Gets the specified UploadSession.
        /// </summary>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> for the request.</param>
        /// <returns>The Item.</returns>
        public async Task<UploadSession> GetAsync(CancellationToken cancellationToken)
        {
            this.Method = "GET";
            var retrievedEntity = await this.SendAsync<UploadSession>(null, cancellationToken).ConfigureAwait(false);
            return retrievedEntity;
        }
    }
}
