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

namespace Test.OneDriveSdk.Authentication
{
    using System.Collections.Generic;

    using Microsoft.OneDrive.Sdk;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class OAuthErrorHandlerTests
    {
        [TestMethod]
        [ExpectedException(typeof(OneDriveException))]
        public void ValidateError_NoDescription()
        {
            var errorMessage = "This is an error.";
            var responseValues = new Dictionary<string, string>
            {
                { Constants.Authentication.ErrorKeyName, errorMessage },
            };

            try
            {
                OAuthErrorHandler.ThrowIfError(responseValues);
            }
            catch(OneDriveException exception)
            {
                Assert.AreEqual(OneDriveErrorCode.AuthenticationFailure.ToString(), exception.Error.Code, "Unexpected error code.");
                Assert.AreEqual(errorMessage, exception.Error.Message, "Unexpected error message.");

                // Re-throw to kick off final validation.
                throw;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(OneDriveException))]
        public void ValidateError_WithDescription()
        {
            var errorMessage = "This is an error.";
            var errorDescription = "Error description";
            var responseValues = new Dictionary<string, string>
            {
                { Constants.Authentication.ErrorDescriptionKeyName, errorDescription },
                { Constants.Authentication.ErrorKeyName, errorMessage },
            };

            try
            {
                OAuthErrorHandler.ThrowIfError(responseValues);
            }
            catch (OneDriveException exception)
            {
                Assert.AreEqual(OneDriveErrorCode.AuthenticationFailure.ToString(), exception.Error.Code, "Unexpected error code.");
                Assert.AreEqual(errorDescription, exception.Error.Message, "Unexpected error message.");

                // Re-throw to kick off final validation.
                throw;
            }
        }
    }
}
