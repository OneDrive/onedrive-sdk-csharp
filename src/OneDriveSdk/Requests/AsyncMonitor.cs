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
    using System.Threading;
    using System.Threading.Tasks;

    public class AsyncMonitor<T>
    {
        private AsyncOperationStatus asyncOperationStatus;
        private IBaseClient client;

        internal string monitorUrl;

        public AsyncMonitor(IBaseClient client, string monitorUrl)
        {
            this.client = client;
            this.monitorUrl = monitorUrl;
        }
        
        protected async Task<T> PollForOperationCompletionAsync(IProgress<AsyncOperationStatus> progress, CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                if (!this.client.IsAuthenticated)
                {
                    await this.client.AuthenticateAsync();
                }

                using (var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, this.monitorUrl))
                {
                    await this.client.AuthenticationProvider.AppendAuthHeaderAsync(httpRequestMessage);

                    using (var responseMessage = await this.client.HttpProvider.SendAsync(httpRequestMessage))
                    {
                        // The monitor service will return an Accepted status for any monitor operation that hasn't completed.
                        // If we have a success code that isn't Accepted, the operation is complete. Return the resulting object.
                        if (responseMessage.StatusCode != HttpStatusCode.Accepted && responseMessage.IsSuccessStatusCode)
                        {
                            using (var responseStream = await responseMessage.Content.ReadAsStreamAsync())
                            {
                                return this.client.HttpProvider.Serializer.DeserializeObject<T>(responseStream);
                            }
                        }

                        using (var responseStream = await responseMessage.Content.ReadAsStreamAsync())
                        {
                            this.asyncOperationStatus = this.client.HttpProvider.Serializer.DeserializeObject<AsyncOperationStatus>(responseStream);

                            if (this.asyncOperationStatus == null)
                            {
                                throw new OneDriveException(
                                    new Error
                                    {
                                        Code = OneDriveErrorCode.GeneralException.ToString(),
                                        Message = "Error retrieving monitor status."
                                    });
                            }

                            if (string.Equals(this.asyncOperationStatus.Status, "cancelled", StringComparison.OrdinalIgnoreCase))
                            {
                                return default(T);
                            }

                            if (string.Equals(this.asyncOperationStatus.Status, "failed", StringComparison.OrdinalIgnoreCase)
                                || string.Equals(this.asyncOperationStatus.Status, "deleteFailed", StringComparison.OrdinalIgnoreCase))
                            {
                                object message = null;
                                if (this.asyncOperationStatus.AdditionalData != null)
                                {
                                    this.asyncOperationStatus.AdditionalData.TryGetValue("message", out message);
                                }

                                throw new OneDriveException(
                                    new Error
                                    {
                                        Code = OneDriveErrorCode.GeneralException.ToString(),
                                        Message = message as string
                                    });
                            }
                            
                            if (progress != null)
                            {
                                progress.Report(this.asyncOperationStatus);
                            }
                        }
                    }
                }

                await Task.Delay(Constants.PollingIntervalInMs, cancellationToken);
            }
            
            return default(T);
        }
    }
}
