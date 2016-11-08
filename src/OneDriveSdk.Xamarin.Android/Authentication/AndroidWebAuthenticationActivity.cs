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
    using Android.App;
    using Android.Content;
    using Android.OS;
    using Android.Content.PM;
    using Android.Webkit;

    [Activity(Label = "OneDrive", Theme = "@android:style/Theme.NoTitleBar", ScreenOrientation = ScreenOrientation.Portrait)]
    public class AndroidWebAuthenticationActivity : Activity
    {
        private WebView webView;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            base.SetContentView(Resource.Layout.webform);
            this.webView = FindViewById<WebView>(Resource.Id.webView);
            this.WebAuthenticationUi = GetWebAuthenticationUi();
            this.RequestUri = GetRequestUri();
            this.CallbackUri = GetCallbackUri();
            this.BeginLoadAuthorizationUrl();
        }

        public AndroidWebAuthenticationUi WebAuthenticationUi { get; private set; }

        private AndroidWebAuthenticationUi GetWebAuthenticationUi()
        {
            if (!Intent.HasExtra(AndroidConstants.AuthenticationStateKey))
                return null;
            string stateKey = Intent.GetStringExtra(AndroidConstants.AuthenticationStateKey);
            return AndroidAuthenticationState.Default.Remove<AndroidWebAuthenticationUi>(stateKey);
        }

        public Uri RequestUri { get; private set; }

        private Uri GetRequestUri()
        {
            if (!Intent.HasExtra(AndroidConstants.RequestUriKey))
                return null;
            return new Uri(Intent.GetStringExtra(AndroidConstants.RequestUriKey));
        }

        public Uri CallbackUri { get; private set; }

        private Uri GetCallbackUri()
        {
            if (!Intent.HasExtra(AndroidConstants.CallbackUriKey))
                return null;
            return new Uri(Intent.GetStringExtra(AndroidConstants.CallbackUriKey));
        }

        private void BeginLoadAuthorizationUrl()
        {
            Client client = new Client(this);
            this.webView.Settings.JavaScriptEnabled = true;
            this.webView.SetWebViewClient(client);
            this.webView.LoadUrl(RequestUri.ToString());
        }

        private void OnPageFinished(WebView view, string url)
        {
            Uri source = new Uri(url);
            if (source.AbsoluteUri.StartsWith(CallbackUri.ToString()))
            {
                var parameters = UrlHelper.GetQueryOptions(source);
                this.WebAuthenticationUi.OnCompleted(new AuthCompletedEventArgs(parameters));
                base.Finish();
            }
        }

        private class Client : WebViewClient
        {
            private AndroidWebAuthenticationActivity activity;

            public Client(AndroidWebAuthenticationActivity activity)
            {
                this.activity = activity;
            }

            public override bool ShouldOverrideUrlLoading(WebView view, IWebResourceRequest request)
            {
                return false;
            }

            public override void OnPageFinished(WebView view, string url)
            {
                this.activity.OnPageFinished(view, url);
                base.OnPageFinished(view, url);
            }
        }
    }
}