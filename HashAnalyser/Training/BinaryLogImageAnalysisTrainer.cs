﻿using HashAnalyser.Data;
using HashAnalyser.Training.Models;
using Microsoft.ML;
using Microsoft.ML.Data;
using TorchSharp;
using static Microsoft.ML.Transforms.ValueToKeyMappingEstimator;

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
            var trainer = BinaryHashTransformer.GetImageModel(_mlContext, dataSetFileName);

            if (!Directory.Exists(dataSetFileName))
            {
                Console.WriteLine("File does not exist!");
                return;
            }
            var df = new TrainingDataFormatter("");

            var images = df.LoadImagesFromDirectory(folder: dataSetFileName, useFolderNameAsLabel: true, isBinary: true);
            var lbls = images.Select(x => x.Label).ToList();
            IDataView fullImagesDataset = _mlContext.Data.LoadFromEnumerable(images);

            IDataView shuffledFullImagesDataset = _mlContext.Transforms.Conversion.MapValueToKey(outputColumnName: "LabelAsKey", inputColumnName: "Label", keyOrdinality: KeyOrdinality.ByValue)
                                        .Append(_mlContext.Transforms.LoadRawImageBytes(outputColumnName: "Image", imageFolder: dataSetFileName, inputColumnName: "ImagePath"))
                                        .Fit(fullImagesDataset).Transform(fullImagesDataset);

            var pipeline = _mlContext.Transforms.Conversion.MapValueToKey("Label")
                                        .Append(_mlContext.MulticlassClassification.Trainers.ImageClassification(featureColumnName: "Image", labelColumnName: "LabelAsKey"))
                                        .Append(_mlContext.Transforms.Conversion.MapKeyToValue(outputColumnName: "PredictedLabel", inputColumnName: "PredictedLabel"));


            Train(pipeline, shuffledFullImagesDataset, "BinaryImageModel.zip");
        }


        /// <summary> 
        /// Helper method for evaluating a model after training, using model <paramref name="model"/> and the data  <paramref name="data"/>. 
        /// </summary>
        protected override void Evaluate(ITransformer model, IDataView data)
        {
            Console.WriteLine("=====================Evaluating model performance on test set=====================\n");
            IDataView transformedTest = model.Transform(data);

            MulticlassClassificationMetrics metrics = _mlContext.MulticlassClassification.Evaluate(transformedTest, labelColumnName: "LabelAsKey");

            Console.WriteLine($""" 
            Macro Accuracy: {metrics.MacroAccuracy}
            Micro Accuracy: {metrics.MicroAccuracy}
            Log Loss: {metrics.LogLoss}
            Top-k Accuracy: {metrics.TopKAccuracy}

            {metrics.ConfusionMatrix.GetFormattedConfusionTable()}

            """);
        }

    }

    public class ImageData
    {
        [LoadColumn(0)]
        public string ImagePath;

        [LoadColumn(1)]
        public string Label;
    }

    class InputData
    {
        [VectorType(1)] 
        public VBuffer<Byte> Image1; 
    }

    class OutputData
    {
        [VectorType()]
        public VBuffer<Byte> Image;
    }
}
