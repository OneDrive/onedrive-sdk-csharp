// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Microsoft.Graph;

using Test.OneDrive.Sdk.Mocks;

namespace Test.OneDrive.Sdk
{

    using Microsoft.OneDrive.Sdk;
    using Microsoft.OneDrive.Sdk.Helpers;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class ChunkedUploadProviderTests
    {
        private Mock<UploadSession> uploadSession;
        private Mock<IBaseClient> client;
        private Mock<Stream> uploadStream;
        private int myChunkSize;

        private Mock<ChunkedUploadProvider> mockChunkUploadProvider;

        [TestInitialize]
        public void TestInitialize()
        {
            this.uploadSession = new Mock<UploadSession>();
            this.uploadSession.Object.NextExpectedRanges = new List<string> {"0-"};
            this.uploadSession.Object.UploadUrl = "http://www.example.com/api/v1.0";
            this.client = new Mock<IBaseClient>();
            this.uploadStream = new Mock<Stream>();
            this.myChunkSize = 4242;
            this.mockChunkUploadProvider = new Mock<ChunkedUploadProvider>();
        }

        [TestMethod]
        public void ConstructorTest_Valid()
        {
            this.StreamSetup(true);

            var uploadProvider = new ChunkedUploadProvider(
                this.uploadSession.Object, 
                this.client.Object,
                this.uploadStream.Object,
                320*1024);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ConstructorTest_InvalidStream()
        {
            this.StreamSetup(false);

            var uploadProvider = new ChunkedUploadProvider(
                this.uploadSession.Object,
                this.client.Object,
                this.uploadStream.Object,
                320*1024);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ConstructorTest_InvalidChunkSize()
        {
            this.StreamSetup(false);

            var uploadProvider = new ChunkedUploadProvider(
                this.uploadSession.Object,
                this.client.Object,
                this.uploadStream.Object,
                12);
        }

        [TestMethod]
        public void GetUploadChunkRequests_OneRangeOneChunk()
        {
            var chunkSize = 320 * 1024;
            var totalSize = 100;
            var results = this.SetupGetUploadChunksTest(chunkSize, totalSize, new List<string> {"0-"});
            var expectedRanges = new List<Tuple<long, long, long>> {new Tuple<long, long, long>(0, 99, 100)};
            this.AssertChunksAre(this.CreateUploadExpectedChunkRequests(expectedRanges), results);
        }

        [TestMethod]
        public void GetUploadChunkRequests_OneRangeMultiChunk()
        {
            var chunkSize = 320 * 1024;
            var totalSize = chunkSize*2 + 1;
            var results = this.SetupGetUploadChunksTest(chunkSize, totalSize, new List<string> { "0-" });
            var expectedRanges = new List<Tuple<long, long, long>>
                {
                    new Tuple<long, long, long>(0, chunkSize-1, totalSize),
                    new Tuple<long, long, long>(chunkSize, 2*chunkSize-1, totalSize),
                    new Tuple<long, long, long>(2*chunkSize, 2*chunkSize, totalSize),
                };
            this.AssertChunksAre(this.CreateUploadExpectedChunkRequests(expectedRanges), results);
        }

        [TestMethod]
        public void GetUploadChunkRequests_MultiRangeMultiChunk()
        {
            var chunkSize = 320 * 1024;
            var totalSize = chunkSize*5;
            var offset = 20;
            var results = this.SetupGetUploadChunksTest(chunkSize, totalSize, new List<string>
                {
                    $"0-{chunkSize}",
                    $"{chunkSize*3 - offset}-"
                });
            var expectedRanges = new List<Tuple<long, long, long>>
                {
                    // 0 - chunkSize
                    new Tuple<long, long, long>(0, chunkSize - 1, totalSize),
                    new Tuple<long, long, long>(chunkSize, chunkSize, totalSize),
                    // (chunkSize*3-offset) - end
                    new Tuple<long, long, long>(3*chunkSize - offset, 4*chunkSize - offset - 1, totalSize),
                    new Tuple<long, long, long>(4*chunkSize - offset, 5*chunkSize - offset - 1, totalSize),
                    new Tuple<long, long, long>(5*chunkSize - offset, 5*chunkSize - 1, totalSize)
                };
            this.AssertChunksAre(this.CreateUploadExpectedChunkRequests(expectedRanges), results);
        }

        [TestMethod]
        public void GetRangesRemaining_OneRangeOneChunk()
        {
            var chunkSize = 320*1024;
            var totalSize = 100;
            var results = this.SetupRangesRemainingTest(chunkSize, totalSize, new List<string> {"0-"});
            var expected = new List<Tuple<long, long>> {new Tuple<long, long>(0, 99)};
            this.AssertRangesAre(expected, results);
        }

        [TestMethod]
        public void GetRangesRemaining_OneRangeMultiChunk()
        {
            var chunkSize = 320*1024;
            var totalSize = chunkSize*2 + 1;
            var results = this.SetupRangesRemainingTest(chunkSize, totalSize, new List<string> { "0-" });
            var expected = new List<Tuple<long, long>>
                {
                    new Tuple<long, long>(0, chunkSize*2)
                };
            this.AssertRangesAre(expected, results);
        }

        [TestMethod]
        public void GetRangesRemaining_MultiRangeMultiChunk()
        {
            var chunkSize = 320*1024;
            var totalSize = chunkSize*5;
            var results = this.SetupRangesRemainingTest(chunkSize, totalSize, new List<string>
                {
                    $"0-{chunkSize - 1}",
                    $"{chunkSize*2}-{chunkSize*3}",
                    $"{chunkSize*4}-"
                });
            var expected = new List<Tuple<long, long>>
                {
                    new Tuple<long, long>(0, chunkSize - 1),
                    new Tuple<long, long>(chunkSize*2, chunkSize*3),
                    new Tuple<long, long>(chunkSize*4, chunkSize*5-1)
                };
            this.AssertRangesAre(expected, results);
        }

        private List<Tuple<long, long>> SetupRangesRemainingTest(
            int chunkSize,
            long currentFileSize,
            List<string> nextExpectedRanges)
        {
            this.StreamSetup(true);
            var provider = new TestChunkedUploadProvider(
                this.uploadSession.Object,
                this.client.Object,
                this.uploadStream.Object,
                chunkSize);
            var url = "http://myurl";
            this.uploadStream.Setup(s => s.Length).Returns(currentFileSize);
            var session = new UploadSession
                {
                    UploadUrl = url,
                    NextExpectedRanges = nextExpectedRanges
                };

            return provider.GetRangesRemainingProxy(session);
        }

        private IEnumerable<UploadChunkRequest> SetupGetUploadChunksTest(int chunkSize, long totalSize, IEnumerable<string> ranges)
        {
            this.uploadSession.Object.NextExpectedRanges = ranges;
            this.uploadStream = new Mock<Stream>();
            this.uploadStream.Setup(s => s.Length).Returns(totalSize);
            this.StreamSetup(true);

            var provider = new ChunkedUploadProvider(
                this.uploadSession.Object,
                this.client.Object,
                this.uploadStream.Object,
                chunkSize);

            return provider.GetUploadChunkRequests();
        } 

        private void AssertRangesAre(IList<Tuple<long, long>> rangesExpected, IList<Tuple<long, long>> rangesReceived)
        {
            Assert.AreEqual(rangesExpected.Count, rangesReceived.Count, "Unexpected number of ranges remaining");
            for (var index = 0; index < rangesExpected.Count; index++)
            {
                Assert.AreEqual(
                    rangesExpected[index],
                    rangesReceived[index],
                    string.Format("Expected range {0}-{1}, received {2}-{3}",
                        rangesExpected[index].Item1,
                        rangesExpected[index].Item2,
                        rangesReceived[index].Item1,
                        rangesReceived[index].Item2));
            }
        }

        private void AssertChunksAre(IEnumerable<UploadChunkRequest> expectedChunks,
                                     IEnumerable<UploadChunkRequest> receivedChunks)
        {
            Assert.AreEqual(expectedChunks.Count(), receivedChunks.Count(), "Incorrect number of chunks received");
            var receivedSet = new HashSet<Tuple<long, long, long>>();
            foreach (var chunk in receivedChunks)
            {
                receivedSet.Add(new Tuple<long, long, long>(chunk.RangeBegin, chunk.RangeEnd, chunk.TotalSessionLength));
            }

            foreach (var chunk in expectedChunks)
            {
                Assert.IsTrue(receivedSet.Contains(new Tuple<long, long, long>(chunk.RangeBegin, chunk.RangeEnd, chunk.TotalSessionLength)),
                    $"Expected chunk not found: {chunk.RangeBegin}-{chunk.RangeEnd}/{chunk.TotalSessionLength}");
            }
        }

        private IEnumerable<UploadChunkRequest> CreateUploadExpectedChunkRequests(
            IEnumerable<Tuple<long, long, long>> chunkSpecifiers)
        {
            return chunkSpecifiers.Select(chunk => new UploadChunkRequest(
                "http://www.example.com/api/v1.0",
                this.client.Object,
                null,
                chunk.Item1,
                chunk.Item2,
                chunk.Item3));
        }

        private void StreamSetup(bool canReadAndSeek)
        {
            this.uploadStream.Setup(s => s.CanSeek).Returns(canReadAndSeek);
            this.uploadStream.Setup(s => s.CanRead).Returns(canReadAndSeek);
        }
    }
}
