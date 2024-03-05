using HashAnalyser.Data;
using HashAnalyser.Data.Models;
using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.TorchSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TorchSharp;
using static Microsoft.ML.DataOperationsCatalog;

namespace HashAnalyser.Training
{
    /// <summary> 
    /// Class for training a new model
    /// </summary>
    public class HashAnalysisTrainer
    {
        private HashModel[]? _trainingData;
        private bool _useCPU = false;

        /// <summary> 
        /// Class for training a new model using the device specified in <paramref name="deviceType"/>. Default is CUDA.
        /// </summary>
        public HashAnalysisTrainer(DeviceType deviceType = DeviceType.CUDA) 
        {
            if(deviceType == DeviceType.CPU) { _useCPU = true; }
            torch.InitializeDeviceType(deviceType);
        }

        /// <summary> 
        /// Method for training a new model with the data supplied <paramref name="dataSetFileName"/>. 
        /// </summary>
        public void Train(string dataSetFileName) 
        {
            var mlContext = new MLContext(seed: 0)
            {
                GpuDeviceId = 0,
                FallbackToCpu = _useCPU,
            };

            if (!File.Exists(dataSetFileName))
            {
                Console.WriteLine("File does not exist!");
                return;
            }

            TrainingDataFormatter _formatter = new(dataSetFileName);
            var data = _formatter.LoadFile(dataSetFileName);

            IDataView dataView = mlContext.Data.LoadFromEnumerable(data);

            var trainer = mlContext.Transforms.Conversion.MapValueToKey("Label")
                        .Append(mlContext.MulticlassClassification.Trainers.TextClassification(labelColumnName: "Label", sentence1ColumnName: "Hash"))
                        .Append(mlContext.Transforms.Conversion.MapKeyToValue("PredictedLabel"));

            dataView = mlContext.Data.ShuffleRows(dataView);
            TrainTestData trainValidationData = mlContext.Data.TrainTestSplit(dataView, testFraction: 0.2);

            Console.WriteLine("Begining training... \n");
            ITransformer model = trainer.Fit(trainValidationData.TrainSet);
            Console.WriteLine("Training complete... \n");

            Console.WriteLine("Saving model... \n");
            mlContext.Model.Save(model, dataView.Schema, "model.zip");

            Console.WriteLine("Evaluating model performance on train set... \n");
            IDataView transformedTest = model.Transform(trainValidationData.TrainSet);

            MulticlassClassificationMetrics metrics = mlContext.MulticlassClassification.Evaluate(transformedTest);

            Console.WriteLine($""" 
            Macro Accuracy: {metrics.MacroAccuracy}
            Micro Accuracy: {metrics.MicroAccuracy}
            Log Loss: {metrics.LogLoss}

            {metrics.ConfusionMatrix.GetFormattedConfusionTable()}

            """);

            Console.WriteLine("Evaluating model performance on test set...");

            transformedTest = model.Transform(trainValidationData.TestSet);
            metrics = mlContext.MulticlassClassification.Evaluate(transformedTest);

            Console.WriteLine($""" 
            Macro Accuracy: {metrics.MacroAccuracy}
            Micro Accuracy: {metrics.MicroAccuracy}
            Log Loss: {metrics.LogLoss}

            {metrics.ConfusionMatrix.GetFormattedConfusionTable()}

            """);
        }
    }
}
