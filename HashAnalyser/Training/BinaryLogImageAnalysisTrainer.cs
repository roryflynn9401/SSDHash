using HashAnalyser.Data;
using HashAnalyser.Training.Models;
using Microsoft.ML;
using Microsoft.ML.Data;
using TorchSharp;

namespace HashAnalyser.Training
{
    /// <summary> 
    /// Class containing logic for training a new binary model
    /// </summary>
    public class BinaryLogImageAnalysisTrainer : HashAnalysisTrainer
    {

        /// <summary> 
        /// Class for training a new binary model using the device specified in <paramref name="deviceType"/>. Default is CUDA.
        /// </summary>
        public BinaryLogImageAnalysisTrainer(DeviceType deviceType = DeviceType.CUDA) : base(deviceType) { }


        public override void TrainModel(string dataSetFileName)
        {
            var trainer = BinaryHashTransformer.GetImageModel(_mlContext);

            if (!File.Exists(dataSetFileName))
            {
                Console.WriteLine("File does not exist!");
                return;
            }

            TrainingDataFormatter _formatter = new(dataSetFileName);
            var data = _formatter.LoadFileForMulticlass(dataSetFileName);

            IDataView dataView = _mlContext.Data.LoadFromEnumerable(data);
            

            Train(trainer, dataView, "MulticlassImageModel.zip");
        }


        /// <summary> 
        /// Helper method for evaluating a model after training, using model <paramref name="model"/> and the data  <paramref name="data"/>. 
        /// </summary>
        protected override void Evaluate(ITransformer model, IDataView data)
        {
            Console.WriteLine("=====================Evaluating model performance on test set=====================\n");
            IDataView transformedTest = model.Transform(data);

            MulticlassClassificationMetrics metrics = _mlContext.MulticlassClassification.Evaluate(transformedTest, labelColumnName: "Label");

            Console.WriteLine($""" 
            Macro Accuracy: {metrics.MacroAccuracy}
            Micro Accuracy: {metrics.MicroAccuracy}
            Log Loss: {metrics.LogLoss}
            Top-k Accuracy: {metrics.TopKAccuracy}

            {metrics.ConfusionMatrix.GetFormattedConfusionTable()}

            """);
        }

    }
}
