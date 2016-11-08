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
    using System.Threading.Tasks;
    using Android.Content;

    public class AndroidWebAuthenticationUi : IWebAuthenticationUi
    {
        public event EventHandler<AuthCompletedEventArgs> Completed;
        public event EventHandler<AuthFailedEventArgs> Failed;

        public AndroidWebAuthenticationUi(Context context)
        {
            this.Context = context;
        }

        public Context Context { get; private set; }

        public Task<IDictionary<string, string>> AuthenticateAsync(Uri requestUri, Uri callbackUri)
        {
            TaskCompletionSource<IDictionary<string, string>> tcs = new TaskCompletionSource<IDictionary<string, string>>();
            this.Completed += (s, e) =>
            {
                tcs.SetResult(e.AuthorizationParameters);
            };
            this.Failed += (s, e) =>
            {
                tcs.SetException(e.Error);
            };

            string stateKey = AndroidAuthenticationState.Default.Add<AndroidWebAuthenticationUi>(this);
            Intent intent = new Intent(this.Context, typeof(AndroidWebAuthenticationActivity));
            intent.PutExtra(AndroidConstants.AuthenticationStateKey, stateKey);
            intent.PutExtra(AndroidConstants.RequestUriKey, requestUri.ToString());
            intent.PutExtra(AndroidConstants.CallbackUriKey, callbackUri.ToString());
            Context.StartActivity(intent);
            return tcs.Task;
        }

        internal void OnCompleted(AuthCompletedEventArgs e)
        {
            if (Completed != null)
            {
                Completed(this, e);
            }
        }

        internal void OnFailed(AuthFailedEventArgs e)
        {
            if (Failed != null)
            {
                Failed(this, e);
            }
        }
    }
}