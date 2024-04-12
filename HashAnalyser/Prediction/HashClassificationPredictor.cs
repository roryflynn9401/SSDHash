using HashAnalyser.Data;
using HashAnalyser.Data.Models;
using HashAnalyser.Data.Models.Binary;
using HashAnalyser.Data.Models.Multiclass;
using Microsoft.ML;
using Microsoft.ML.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TorchSharp;
using static TorchSharp.torch.utils;

namespace HashAnalyser.Prediction
{
    /// <summary> 
    /// Class containing methods for predicting the category of log data
    /// </summary>
    public class HashClassificationPredictor
    {
        protected bool _useCPU = false;
        private MLContext _mlContext;

        public HashClassificationPredictor(DeviceType deviceType = DeviceType.CUDA)
        {
            if (deviceType == DeviceType.CPU) { _useCPU = true; }
            torch.InitializeDeviceType(deviceType);

            _mlContext = new MLContext(seed: 0)
            {
                GpuDeviceId = 0,
                FallbackToCpu = _useCPU,
            };
        }

        /// <summary> 
        /// Method for performing binary classification for unseen data using the model and data supplied in <paramref name="modelName"/> and <paramref name="dataSetFileName"/>. 
        /// Returns mapping of Hash, PredictedLabel.
        /// </summary>
        public IDataView? PredictBinary(string[] hashes, string modelName = "BinaryModel.zip")
        {
            var input = hashes.Select(x => new HashInput(x));
            var inputData = _mlContext.Data.LoadFromEnumerable(input);

            return PredictBinary(inputData, modelName);
        }

        public IDataView? PredictBinary(IDataView data, string modelName = "BinaryModel.zip")
        {
            ITransformer model = _mlContext.Model.Load(modelName, out var schema);
            var results = model.Transform(data);

            return results;
        }

        /// <summary> 
        /// Method for performing multiclass classification for unseen data using the model and data supplied in <paramref name="modelName"/> and <paramref name="dataSetFileName"/>. 
        /// Returns mapping of Hash, PredictedLabel.
        /// </summary>
        public IDataView? PredictMulticlass(string[] hashes, string modelName = "MulticlassModel.zip")
        {
            var input = hashes.Select(x => new MulticlassHashModel(x));
            var inputData = _mlContext.Data.LoadFromEnumerable(input);

            return PredictMulticlass(inputData, modelName);
        }

        public IDataView? PredictMulticlass(IDataView data, string modelName = "MulticlassModel.zip")
        {
            ITransformer model = _mlContext.Model.Load(modelName, out var schema);
            var results = model.Transform(data);

            return results;
        }

        /// <summary> 
        /// Method for performing binary classification for unseen labeled data using the model and data supplied in <paramref name="modelName"/> and <paramref name="dataSetFileName"/>. 
        /// Returns mapping of Hash, PredictedLabel and prints metrics in the console.
        /// </summary>
        public Dictionary<string, BinaryHashPrediction> PredictLabeledBinary(string dataSetFileName, string modelName = "BinaryModel.zip")
        {
            Dictionary<string, BinaryHashPrediction> binaryResults = new();

            ITransformer model = _mlContext.Model.Load(modelName, out var schema);
            TrainingDataFormatter _formatter = new(dataSetFileName);
            BinaryHashModel[] inputData = _formatter.LoadFileForBinary(dataSetFileName).ToArray();
            
            if (inputData is null || inputData.Length == 0) throw new ArgumentNullException(nameof(inputData));

            var engine = _mlContext.Model.CreatePredictionEngine<BinaryHashModel, BinaryHashPrediction>(model);

            foreach (var item in inputData)
            {
                binaryResults[item.Hash] = engine.Predict(new BinaryHashModel(item.Hash));
            }

            if (binaryResults.Count == 0) throw new ArgumentNullException(nameof(binaryResults));
            
            EvaluateBinaryPredictions(inputData, binaryResults);
            return binaryResults;
        }

        /// <summary> 
        /// Method for performing multiclass classification for unseen labeled data using the model and data supplied in <paramref name="modelName"/> and <paramref name="dataSetFileName"/>. 
        /// Returns mapping of Hash, PredictedLabel.
        /// </summary>
        public Dictionary<string, MulticlassHashPrediction> PredictLabeledMulticlass(string dataSetFileName, string modelName = "MulticlassModel.zip")
        {
            Dictionary<string, MulticlassHashPrediction> multiclassResults = new();
            ITransformer model = _mlContext.Model.Load(modelName, out var schema);
            TrainingDataFormatter _formatter = new(dataSetFileName);

            MulticlassHashModel[] inputData = _formatter.LoadFileForMulticlass(dataSetFileName).ToArray();
            var unlabeledData = inputData.Select(x => new BinaryHashModel { Hash = x.Hash }).ToArray();
            if (inputData is null || inputData.Length == 0) throw new ArgumentNullException(nameof(inputData));

            var engine = _mlContext.Model.CreatePredictionEngine<MulticlassHashModel, MulticlassHashPrediction>(model);

            foreach (var item in inputData)
            {
                multiclassResults[item.Hash] = engine.Predict(new MulticlassHashModel(item.Hash));
            }

            if (multiclassResults.Count == 0) throw new ArgumentNullException(nameof(multiclassResults));
            return multiclassResults;
        }

        /// <summary>
        /// Method for outputing metrics for labeled, unseen data. Requires Predict to have been run which populates <paramref name="_inputData">_inputData</paramref> and <paramref name="_results">_results</paramref>
        /// </summary>
        public void EvaluateBinaryPredictions(BinaryHashModel[] inputData, Dictionary<string, BinaryHashPrediction> binaryResults)
        {
            int correctPredictions = 0;
            int incorrectPredictions = 0;
            int TP = 0;
            int TN = 0;
            int FP = 0;
            int FN = 0;

            var predictions = new Dictionary<string, int>();

            foreach (var result in binaryResults)
            {
                var label = inputData.FirstOrDefault(x => x.Hash == result.Key)?.Label;

                if (label == true && result.Value.PredictedLabel == true)
                {
                    TP++;
                    correctPredictions++;
                }
                else if (label == false && result.Value.PredictedLabel == true)
                {
                    FP++;
                    incorrectPredictions++;
                }
                else if (label == false && result.Value.PredictedLabel == false)
                {
                    TN++;
                    correctPredictions++;
                }
                else if (label == true && result.Value.PredictedLabel == false)
                {
                    FN++;
                    incorrectPredictions++;
                }
            }

            Console.WriteLine($"""
                Correct Predictions: {correctPredictions}
                Incorrect Predictions: {incorrectPredictions}

                    Confusion Table
                    0           1
                -|----------------------
                0|  {TN}        {FN}
                 |
                1|  {FP}        {TP}


                Precision = {(double)TP / (TP + FP)}
                Recall = {(double)TP / (TP + FN)}
                Accuracy = {(double)(TP + TN) / (TP + TN + FN + FP)}
                F1 Score = {(double)TP / (TP + (0.5 * (FP + FN)))}
                """);


            foreach (var kvp in predictions)
            {
                Console.WriteLine(kvp.Key + " : " + kvp.Value);
            }
        }

    }
}
