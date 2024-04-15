using HashAnalyser.Data;
using SSDHash;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitTests
{
    [TestFixture]
    public class IOTests
    {
        private string AnonFilePath, TrainingFilePath;
        private TrainingDataFormatter _formatter;

        [SetUp]
        public void Setup()
        {
            AnonFilePath = Environment.CurrentDirectory + "../../../../TestFiles/anon.json";
            TrainingFilePath = "F:\\source\\repos\\ResearchProject\\LogAnalyser\\bin\\Debug\\net7.0\\trainingData.csv";
            _formatter = new(TrainingFilePath);
        }
        
        [Test]
        public async Task AnonIO()
        {
            var objs = await HashProcessing.GetFileObjects(AnonFilePath);
            if(objs == null)
                Assert.Fail();

            if (objs?.Any() == true)
                Assert.Pass();
        }

        [Test]
        public void BinaryIO()
        {
            var binaryData = _formatter.LoadFileForBinary(TrainingFilePath).ToArray();
            if(binaryData.Length == 0)
            {
                Assert.Fail();
            }
            Assert.Pass();
        }

        [Test]
        public void MulticlassIO()
        {
            var multiclassData = _formatter.LoadFileForMulticlass(TrainingFilePath).ToArray();
            if (multiclassData.Length == 0)
            {
                Assert.Fail();
            }
            Assert.Pass();
        }
    }
}
