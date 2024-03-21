using HashAnalyser.Data.Models.Binary;
using HashAnalyser.Data.Models.Multiclass;
using HashAnalyser.Prediction;
using HashAnalyser.Training;

namespace LogAnalyser
{
    public class Program
    {
        private HashAnalysisTrainer? _trainer;

        public static void Main(string[] args)
        {

            if(args.Contains("--help")) HelpMenu();
            if (args.Length < 2)
            {
                Console.WriteLine("Invalid arguements supplied. Usage: <tool> <command> <args>");
                HelpMenu();
            }

            ProcessCommand(args);
        }

        #region Menu Prints

        private static void ProcessCommand(string[] args)
        {
            var command = args.First().ToLower();
            if (command == null) return;

            var commandParams = args.Where(x => x != command).ToArray();

            switch (command)
            {
                case "train":
                    ProcessTrainCommand(commandParams);
                    break;
                case "predict":
                    ProcessPredictCommand(commandParams);
                    break;
                case "pipeline":
                    break;
            }
        }

        private static void ProcessTrainCommand(string[] trainArgs)
        {
            foreach (var arg in trainArgs)
            {
                switch (arg.ToLower())
                {
                    case "-b":
                        TrainBinaryModel(trainArgs);
                        break;
                    case "-m":
                        TrainMulticlassModel(trainArgs);
                        break;
                }
            }
        }

        private static void ProcessPredictCommand(string[] trainArgs)
        {
            foreach (var arg in trainArgs)
            {
                switch (arg.ToLower())
                {
                    case "-b":
                        PredictBinaryModel(trainArgs);
                        break;
                    case "-m":
                        PredictMulticlassModel(trainArgs);
                        break;
                }
            }
        }

        private static void HelpMenu()
        {
            Console.WriteLine($"""
            This is the help menu for SSDHash. Below are the command-line arguments available:
            -h|--help : Help Menu,

            Commands:
            train 
                Arguments:
                    -b|                 - Trains a model using binary classification (benign, malicious)
                    -m|                 - Trains a model using multiclass classification (c&c, dos etc.)
                Variables
                    --dataset           - File path for training/test dataset

                 

            """);
        }


        #endregion


        #region Commands

        private static Action PrintInvalidFileText = () => Console.WriteLine("Invalid file path arguement supplied. Provide the path to the dataset file using --dataset PATH/TO/FILE");

        private static void TrainBinaryModel(string[] trainArgs) => TrainModel(trainArgs, new BinaryHashAnalysisTrainer());

        private static void TrainMulticlassModel(string[] trainArgs) => TrainModel(trainArgs, new MulticlassHashAnalysisTrainer());

        private static void TrainModel(string[] trainArgs, HashAnalysisTrainer trainer)
        {
            string? filePath = null;

            for(int i =0; i < trainArgs.Length; i++)
            {
                if (trainArgs[i] == "--dataset")
                {
                    if(i < trainArgs.Length - 1 && File.Exists(trainArgs[i + 1]))
                    {
                        filePath = trainArgs[i + 1];
                        break;
                    }
                    else
                    {
                        PrintInvalidFileText();
                        return;
                    }
                }
            }
            if(filePath is null)
            {
                PrintInvalidFileText();
                return;
            }

            trainer.TrainModel(filePath);
        }


        private static Dictionary<string, BinaryHashPrediction>? PredictBinaryModel(string[] trainArgs)
        {
            var transformer = new HashClassificationPredictor();
            string? filePath = null;

            for (int i = 0; i < trainArgs.Length; i++)
            {
                if (trainArgs[i] == "--dataset")
                {
                    if (i < trainArgs.Length - 1 && File.Exists(trainArgs[i + 1]))
                    {
                        filePath = trainArgs[i + 1];
                        break;
                    }
                    else
                    {
                        PrintInvalidFileText();
                        return null;
                    }
                }
            }
            if (filePath is null)
            {
                PrintInvalidFileText();
                return null;
            }

            return transformer.PredictLabeledBinary(filePath);
        }

        private static Dictionary<string, MulticlassHashPrediction>? PredictMulticlassModel(string[] trainArgs)
        {
            var transformer = new HashClassificationPredictor();
            string? filePath = null;

            for (int i = 0; i < trainArgs.Length; i++)
            {
                if (trainArgs[i] == "--dataset")
                {
                    if (i < trainArgs.Length - 1 && File.Exists(trainArgs[i + 1]))
                    {
                        filePath = trainArgs[i + 1];
                        break;
                    }
                    else
                    {
                        PrintInvalidFileText();
                        return null;
                    }
                }
            }
            if (filePath is null)
            {
                PrintInvalidFileText();
                return null;
            }

            return transformer.PredictLabeledMulticlass(filePath);
        }

        #endregion
    }
}