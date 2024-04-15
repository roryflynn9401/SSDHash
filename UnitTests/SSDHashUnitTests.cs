using NUnit.Framework;
using SSDHash;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitTests
{
    [TestFixture]
    public class SSDHashUnitTests
    {
        private string ssdData = string.Empty;
        private string ssdhashDigest = string.Empty;
        private HashExtractor ssdHash = new();

        [SetUp]
        public void Setup()
        {
            ssdData = """{ "timestamp": "2023-12-11T08:30:45.123Z", "message": "User ’john_doe’ successfully logged in." }""";
            ssdhashDigest = "0000000007000000E0A000000000000000000000000000000000040000000000";
        }

        [Test]
        public void ValidData()
        {
            var hash = ssdHash.GetHash(ssdData);
            if(hash == ssdhashDigest)
            {
                Assert.Pass();
            }
            Assert.Fail();
        }

        [Test]
        public void MalformedSSDData()
        {
            var malformedData = new string(ssdData.Take(ssdData.Length - 10).ToArray());
            if(ssdHash.GetHash(malformedData) == null)
            {
                Assert.Pass();
            }
            Assert.Fail();
        }

        [Test]
        public async Task ValidBatch()
        {
            var batch = Enumerable.Range(0, 10).Select(x => ssdData).ToArray();
            var hashes = await HashProcessing.ComputeHashes(batch);
            if(hashes.All(x => x == ssdhashDigest))
            { 
                Assert.Pass(); 
            }
            Assert.Fail();
        }
        [Test]

        public async Task MalformedSSDBatch()
        {
            var malformedData = new string(ssdData.Take(ssdData.Length - 10).ToArray());
            var batch = Enumerable.Range(0, 10).Select(x => malformedData).ToArray();
            var hashes = await HashProcessing.ComputeHashes(batch);
            if (hashes.All(x => string.IsNullOrEmpty(x)))
            {
                Assert.Pass();
            }
            Assert.Fail();
        }
    }
}
