using HashAnalyser.Prediction;
using HashAnalyser.Training;
using System.Linq;

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

        private static void TrainBinaryModel(string[] trainArgs)
        {
            var trainer = new BinaryHashAnalysisTrainer();
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

        private static void PredictBinaryModel(string[] trainArgs)
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
                        return;
                    }
                }
            }
            if (filePath is null)
            {
                PrintInvalidFileText();
                return;
            }

            var results = transformer.PredictBinary(filePath);
            transformer.VerifyBinaryPredictions();
        }

        #endregion
    }
}