using HashAnalyser.Data;
using HashAnalyser.Data.Models.Binary;
using HashAnalyser.Data.Models.Multiclass;
using HashAnalyser.Training.Models;
using Microsoft.ML;
using Microsoft.ML.Data;
using TorchSharp;

namespace HashAnalyser.Training
{
    /// <summary> 
    /// Class containing logic for training a new binary model
    /// </summary>
    public class BinaryRawLogAnalysisTrainer : HashAnalysisTrainer
    {

        /// <summary> 
        /// Class for training a new binary model using the device specified in <paramref name="deviceType"/>. Default is CUDA.
        /// </summary>
        public BinaryRawLogAnalysisTrainer(DeviceType deviceType = DeviceType.CUDA) : base(deviceType) { }


        public override void TrainModel(string dataSetFileName)
        {
            var trainer = BinaryHashTransformer.GetRawLogModel(_mlContext);

            if (!File.Exists(dataSetFileName))
            {
                Console.WriteLine("File does not exist!");
                return;
            }

            TrainingDataFormatter _formatter = new(dataSetFileName);
            var data = _formatter.LoadRawLogFileForBinary(dataSetFileName);

            IDataView dataView = _mlContext.Data.LoadFromEnumerable(data);
            Train(trainer, dataView, "BinaryRawModel.zip");
        }


        /// <summary> 
        /// Helper method for evaluating a model after training, using model <paramref name="model"/> and the data  <paramref name="data"/>. 
        /// </summary>
        protected override void Evaluate(ITransformer model, IDataView data)
        {
            Console.WriteLine("=====================Evaluating model performance on test set=====================\n");
            IDataView transformedTest = model.Transform(data);

            BinaryClassificationMetrics metrics = _mlContext.BinaryClassification.Evaluate(transformedTest, probabilityColumnName: "Score", labelColumnName: "label");

            Console.WriteLine($""" 
            Accuracy: {metrics.Accuracy}
            F1 Score: {metrics.F1Score}
            ROC AUC: {metrics.AreaUnderRocCurve}
            {metrics.ConfusionMatrix.GetFormattedConfusionTable()}

            """);
        }

    }
}
