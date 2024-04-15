using HashAnalyser.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitTests
{
    [TestFixture]
    public class EncodingTests
    {
        private string validEncoding;
        private string ssdhashDigest = "10004000D0000000008072A00500030000000907000000000E006000C00000B0";

        [SetUp]
        public void Setup()
        {
            validEncoding = "able across affirm almost anything artist away before building buy cause choose commercial could daughter despite do economic ever executive figure firm future get guy him hour improve interview knowledge learn list major measure miss mrs never often one part pattern place possible production quickly receive represent road season side significant smile speech state stuff talk this thousand toward two violence weight woman world ";
        }

        [Test]
        public void ValidDataEncode()
        {
            var formatter = new TrainingDataFormatter("");
            var encodedHash = formatter.PositionallyEncode(ssdhashDigest);

            if (encodedHash == validEncoding)
                Assert.Pass();

            Assert.Fail();
        }

        [Test]
        public void ValidDataDecode()
        {
            var formatter = new TrainingDataFormatter("");
            var decodedHash = formatter.PositionallyDecode(validEncoding);

            if (decodedHash == ssdhashDigest)
                Assert.Pass();

            Assert.Fail();
        }
    }
}
