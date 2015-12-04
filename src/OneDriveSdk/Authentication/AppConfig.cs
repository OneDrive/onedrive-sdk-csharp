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
    public class AppConfig
    {
        /// <summary>
        /// Gets or sets the application ID for Active Directory authentication.
        /// </summary>
        public string ActiveDirectoryAppId { get; set; }

        /// <summary>
        /// Gets or sets the client secret for Active Directory authentication.
        /// </summary>
        public string ActiveDirectoryClientSecret { get; set; }

        /// <summary>
        /// Gets or sets the application return URL for Active Directory authentication.
        /// </summary>
        public string ActiveDirectoryReturnUrl { get; set; }

        /// <summary>
        /// Gets or sets the service endpoint URL for OneDrive for Business.
        /// </summary>
        public string ActiveDirectoryServiceEndpointUrl { get; set; }

        /// <summary>
        /// Gets or sets the service resource for Active Directory authentication.
        /// </summary>
        public string ActiveDirectoryServiceResource { get; set; }

        /// <summary>
        /// Gets or sets the application ID for Microsoft account authentication.
        /// </summary>
        public string MicrosoftAccountAppId { get; set; }

        /// <summary>
        /// Gets or sets the client secret for Microsoft account authentication.
        /// </summary>
        public string MicrosoftAccountClientSecret { get; set; }

        /// <summary>
        /// Gets or sets the application return URL for Microsoft account authentication.
        /// </summary>
        public string MicrosoftAccountReturnUrl { get; set; }

        /// <summary>
        /// Gets or sets the requested scopes for Microsoft account authentication.
        /// </summary>
        public string[] MicrosoftAccountScopes { get; set; }
    }
}
