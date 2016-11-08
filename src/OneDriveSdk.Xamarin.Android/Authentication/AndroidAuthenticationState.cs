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

    internal class AndroidAuthenticationState
    {
        private static AndroidAuthenticationState instance = new AndroidAuthenticationState();
        private Dictionary<string, object> dictionary;

        protected AndroidAuthenticationState()
        {
            this.dictionary = new Dictionary<string, object>();
        }

        public static AndroidAuthenticationState Default
        {
            get { return instance; }
        }

        public string Add<T>(T state) where T : class
        {
            string key = Guid.NewGuid().ToString();
            this.dictionary.Add(key, state);
            return key;
        }

        public T Remove<T>(string key) where T : class
        {
            if (this.dictionary.ContainsKey(key))
            {
                T state = this.dictionary[key] as T;
                this.dictionary.Remove(key);
                return state;
            }
            return null;
        }
    }
}