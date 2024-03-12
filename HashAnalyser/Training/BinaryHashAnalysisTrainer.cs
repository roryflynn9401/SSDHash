using HashAnalyser.Data;
using HashAnalyser.Data.Models.Binary;
using HashAnalyser.Training.Models;
using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Tokenizers;
using TorchSharp;
using static Microsoft.ML.DataOperationsCatalog;

namespace HashAnalyser.Training
{
    /// <summary> 
    /// Class containing logic for training a new binary model
    /// </summary>
    public class BinaryHashAnalysisTrainer : HashAnalysisTrainer
    {

        /// <summary> 
        /// Class for training a new binary model using the device specified in <paramref name="deviceType"/>. Default is CUDA.
        /// </summary>
        public BinaryHashAnalysisTrainer(DeviceType deviceType = DeviceType.CUDA) : base(deviceType) { }


        public override void TrainModel(string dataSetFileName)
        {
            var trainer = BinaryHashTransformer.GetModel(_mlContext);

            if (!File.Exists(dataSetFileName))
            {
                Console.WriteLine("File does not exist!");
                return;
            }

            TrainingDataFormatter _formatter = new(dataSetFileName);
            var data = _formatter.LoadFileForBinary(dataSetFileName);

            IDataView dataView = _mlContext.Data.LoadFromEnumerable(data);
            Train(trainer, dataView, "BinaryModel.zip");
        }


        /// <summary> 
        /// Helper method for evaluating a model after training, using model <paramref name="model"/> and the data  <paramref name="data"/>. 
        /// </summary>
        protected override void Evaluate(ITransformer model, IDataView data)
        {
            Console.WriteLine("=====================Evaluating model performance on test set=====================\n");
            IDataView transformedTest = model.Transform(data);

            BinaryClassificationMetrics metrics = _mlContext.BinaryClassification.Evaluate(transformedTest);

            Console.WriteLine($""" 
            Accuracy: {metrics.Accuracy}
            F1 Score: {metrics.F1Score}

            {metrics.ConfusionMatrix.GetFormattedConfusionTable()}

            """);
        }

    }
}
