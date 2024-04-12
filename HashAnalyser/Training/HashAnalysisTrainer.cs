using HashAnalyser.Data;
using HashAnalyser.Data.Models.Binary;
using HashAnalyser.Training.Models;
using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Tokenizers;
using Microsoft.ML.TorchSharp;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TorchSharp;
using static Microsoft.ML.DataOperationsCatalog;

namespace HashAnalyser.Training
{
    /// <summary> 
    /// Abtract Class containing base logic for training a new model
    /// </summary>
    public abstract class HashAnalysisTrainer
    {
        protected BinaryHashModel[]? _trainingData;
        protected bool _useCPU = false;
        protected MLContext _mlContext;

        /// <summary> 
        /// Abtract Class containing base logic for training a new model using the device specified in <paramref name="deviceType"/>. Default is CUDA.
        /// </summary>
        public HashAnalysisTrainer(DeviceType deviceType = DeviceType.CUDA) 
        {
            if(deviceType == DeviceType.CPU) { _useCPU = true; }
            torch.InitializeDeviceType(deviceType);

             _mlContext = new MLContext(seed: 0)
             {
                 GpuDeviceId = 0,
                 FallbackToCpu = _useCPU,
             };
        }

        /// <summary> 
        /// Method to be implemented in concrete class for specific training configuration. Uses the data supplied from the file specified in <paramref name="dataSetFileName"/>. 
        /// </summary>
        public abstract void TrainModel(string dataSetFileName);

        /// <summary> 
        /// Method to be implemented in concrete class to evaluate a model after training, using <paramref name="model"/> and <paramref name="data"/> for model and data inputs. 
        /// </summary>
        protected abstract void Evaluate(ITransformer model, IDataView data);

        /// <summary> 
        /// Method for training a new model with the data supplied <paramref name="dataView"/>. 
        /// </summary>
        protected ITransformer Train(IEstimator<ITransformer> trainer, IDataView dataView, string modelName = "model.zip")
        {
            dataView = _mlContext.Data.ShuffleRows(dataView);
            TrainTestData trainValidationData = _mlContext.Data.TrainTestSplit(dataView, testFraction: 0.2);

            Console.WriteLine("==================Begining training==================\n");
            var model = trainer.Fit(trainValidationData.TrainSet);
            Console.WriteLine("==================Training complete==================\n");

            Console.WriteLine("Saving model... \n");
            _mlContext.Model.Save(model, dataView.Schema, modelName);
            Console.WriteLine("=====================Model saved=====================\n");

            Evaluate(model, trainValidationData.TestSet);
            return model;
        }

    }
}
