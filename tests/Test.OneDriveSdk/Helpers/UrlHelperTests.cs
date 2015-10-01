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

namespace Test.OneDriveSdk.Helpers
{
    using System;

    using Microsoft.OneDrive.Sdk;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class UrlHelperTests
    {
        [TestMethod]
        public void GetQueryOptions_EmptyFragment()
        {
            var uri = new Uri("https://localhost#");

            var queryValues = UrlHelper.GetQueryOptions(uri);

            Assert.AreEqual(0, queryValues.Count, "Unexpected query values returned.");
        }

        [TestMethod]
        public void GetQueryOptions_EmptyQueryString()
        {
            var uri = new Uri("https://localhost?");

            var queryValues = UrlHelper.GetQueryOptions(uri);

            Assert.AreEqual(0, queryValues.Count, "Unexpected query values returned.");
        }

        [TestMethod]
        public void GetQueryOptions_NoQueryString()
        {
            var uri = new Uri("https://localhost");

            var queryValues = UrlHelper.GetQueryOptions(uri);

            Assert.AreEqual(0, queryValues.Count, "Unexpected query values returned.");
        }

        [TestMethod]
        public void GetQueryOptions_MultipleFragments()
        {
            var uri = new Uri("https://localhost#key=value&key2=value%202");

            var queryValues = UrlHelper.GetQueryOptions(uri);

            Assert.AreEqual(2, queryValues.Count, "Unexpected query values returned.");
            Assert.AreEqual("value", queryValues["key"], "Unexpected query value.");
            Assert.AreEqual("value 2", queryValues["key2"], "Unexpected query value.");
        }

        [TestMethod]
        public void GetQueryOptions_SingleFragment()
        {
            var uri = new Uri("https://localhost#key=value");

            var queryValues = UrlHelper.GetQueryOptions(uri);

            Assert.AreEqual(1, queryValues.Count, "Unexpected query values returned.");
            Assert.AreEqual("value", queryValues["key"], "Unexpected query value.");
        }

        [TestMethod]
        public void GetQueryOptions_MultipleQueryOptions()
        {
            var uri = new Uri("https://localhost?key=value&key2=value%202");

            var queryValues = UrlHelper.GetQueryOptions(uri);

            Assert.AreEqual(2, queryValues.Count, "Unexpected query values returned.");
            Assert.AreEqual("value 2", queryValues["key2"], "Unexpected query value.");
        }

        [TestMethod]
        public void GetQueryOptions_SingleQueryOption()
        {
            var uri = new Uri("https://localhost?key=value");

            var queryValues = UrlHelper.GetQueryOptions(uri);

            Assert.AreEqual(1, queryValues.Count, "Unexpected query values returned.");
            Assert.AreEqual("value", queryValues["key"], "Unexpected query value.");
        }

        [TestMethod]
        public void GetQueryOptions_TrailingAmpersand()
        {
            var uri = new Uri("https://localhost?key=value&");

            var queryValues = UrlHelper.GetQueryOptions(uri);

            Assert.AreEqual(1, queryValues.Count, "Unexpected query values returned.");
            Assert.AreEqual("value", queryValues["key"], "Unexpected query value.");
        }
    }
}
