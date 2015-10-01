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
    using System.Net;

    public static class UrlHelper
    {
        public static IDictionary<string, string> GetQueryOptions(Uri resultUri)
        {
            string[] queryParams = null;
            var queryValues = new Dictionary<string, string>();

            int fragmentIndex = resultUri.AbsoluteUri.IndexOf("#", StringComparison.Ordinal);
            if (fragmentIndex > 0 && fragmentIndex < resultUri.AbsoluteUri.Length + 1)
            {
                queryParams = resultUri.AbsoluteUri.Substring(fragmentIndex + 1).Split('&');
            }
            else if (fragmentIndex < 0)
            {
                if (!string.IsNullOrEmpty(resultUri.Query))
                {
                    queryParams = resultUri.Query.TrimStart('?').Split('&');
                }
            }

            if (queryParams != null)
            {
                foreach (var param in queryParams)
                {
                    if (!string.IsNullOrEmpty(param))
                    {
                        string[] kvp = param.Split('=');
                        queryValues.Add(kvp[0], WebUtility.UrlDecode(kvp[1]));
                    }
                }
            }

            return queryValues;
        }
    }
}
