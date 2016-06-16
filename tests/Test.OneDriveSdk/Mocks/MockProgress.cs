// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Test.OneDriveSdk.Mocks
{
    using System;

    using Microsoft.OneDrive.Sdk;
    using Moq;

    public class MockProgress : Mock<IProgress<AsyncOperationStatus>>
    {
        public MockProgress()
            : base(MockBehavior.Strict)
        {
            this.Setup(mockProgress => mockProgress.Report(It.IsAny<AsyncOperationStatus>()));
        }
    }
}
