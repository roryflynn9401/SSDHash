using HashAnalyser.Data;
using HashAnalyser.Data.Models;
using Microsoft.ML;
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
        private HashModel[]? _inputData;
        private Dictionary<string, HashPrediction> _results = new();

        public HashClassificationPredictor(DeviceType? deviceType = null)
        {
            torch.InitializeDeviceType(deviceType == null ? DeviceType.CUDA : deviceType.Value);
        }

        /// <summary> 
        /// Method for performing classification using the model and data supplied in <paramref name="modelName"/> and <paramref name="dataSetFileName"/>. 
        /// Returns mapping of Hash, PredictedLabel.
        /// </summary>
        public Dictionary<string, HashPrediction> Predict(string dataSetFileName, string modelName = "model.zip")
        {
            var mlContext = new MLContext(seed: 0)
            {
                GpuDeviceId = 0,
                FallbackToCpu = false,
            };
            ITransformer model = mlContext.Model.Load("model.zip", out var schema);
            TrainingDataFormatter _formatter = new(dataSetFileName);

            _inputData = _formatter.LoadFile(dataSetFileName).Select(x => new HashModel(x.Hash)).ToArray();
            if (_inputData is null || _inputData.Length == 0) throw new ArgumentNullException(nameof(_inputData));

            var dataView = mlContext.Data.LoadFromEnumerable(_inputData);

            var engine = mlContext.Model.CreatePredictionEngine<HashModel, HashPrediction>(model);

            foreach (var item in _inputData)
            {
                _results[item.Hash] = engine.Predict(new HashModel(item.Hash));
            }

            if(_results.Count == 0) throw new ArgumentNullException(nameof(_results));
            return _results;
        }

        /// <summary>
        /// Method for outputing metrics for labeled, unseen data. Requires Predict to have been run which populates <paramref name="_inputData">_inputData</paramref> and <paramref name="_results">_results</paramref>
        /// </summary>
        public void VerifyPredictions()
        {
            int correctPredictions = 0;
            int incorrectPredictions = 0;
            int TP = 0;
            int TN = 0;
            int FP = 0;
            int FN = 0;

            var predictions = new Dictionary<string, int>();

            foreach (var result in _results)
            {
                var label = _inputData.FirstOrDefault(x => x.Hash == result.Key)?.Label;

                if (label == "malicious" && result.Value.PredictedLabel == "malicious")
                {
                    TP++;
                }
                else if (label == "malicious")
                {
                    FP++;
                }
                else if (label == "benign" && result.Value.PredictedLabel == "benign")
                {
                    TN++;
                }
                else if (label == "benign")
                {
                    FN++;
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
