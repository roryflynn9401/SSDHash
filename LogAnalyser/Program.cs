using BenchmarkDotNet.Running;
using CsvHelper;
using HashAnalyser.Data.Models.Binary;
using HashAnalyser.Data.Models.Multiclass;
using HashAnalyser.Prediction;
using HashAnalyser.Training;
using LogAnalyser.Processing;
using LogAnalyser.Testing;
using Microsoft.ML;
using SkiaSharp;
using SSDHash;
using System.Globalization;

namespace LogAnalyser
{
    public class Program
    {
        private HashAnalysisTrainer? _trainer;

        public static async Task Main(string[] args)
        {

            if(args.Contains("--help")) HelpMenu();
            if (args.Length < 2)
            {
                Console.WriteLine("Invalid arguements supplied. Usage: <tool> <command> <args>");
                HelpMenu();
            }

            await ProcessCommand(args);
        }

        #region Menu Prints
        private static async Task ProcessCommand(string[] args)
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
                    ProcessPipelineCommand(commandParams);
                    break;
                case "hash":
                    await ProcessHashCommand(commandParams);
                    break;
                case "test":
                    ProcessTestCommand(commandParams);
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
                    case "-c":
                        TrainClusteringModel(trainArgs);
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

        private static void ProcessPipelineCommand(string[] args)
        {
            string? filePath = GetDatasetPath(args);
            

            var pipeline = new LogPredictionPipeline();
            var res = pipeline.ProcessLogs(filePath);
            if (res is null) return;
            WriteCsv(res, "pipelineout");
        }


        private static async Task ProcessHashCommand(string[] trainArgs)
        {
            foreach (var arg in trainArgs)
            {
                switch (arg.ToLower())
                {
                    case "-s":
                        if (trainArgs.Contains("--dataset"))
                        {
                            Console.WriteLine("Dataset supplied in single record mode. Use -m to process files");
                            return;
                        }
                        Console.WriteLine("Input the data you want to hash (JSON,XML or CSV format)");
                        var input = Console.ReadLine();
                        if (input == null)
                        {
                            Console.WriteLine("Invalid Input supplied. Exiting.");
                            return;
                        }

                        var ssdhash = new HashExtractor();
                        var hash = ssdhash.GetHash(input);
                        if(hash != null)
                        {
                            Console.WriteLine("Hash: " + hash);
                        }
                        else
                        {
                            Console.WriteLine("Invalid Data input. Check data is in correct format");
                        }
                        break;
                    case "-m":
                        string? filePath = GetDatasetPath(trainArgs);
                        if (filePath is null) return;

                        var hashes = await HashProcessing.ComputeHashesFromFile(filePath);
                        if (hashes is null) return;
                        WriteCsv(hashes);
                        break;
                }
            }
        }


        private static void ProcessTestCommand(string[] args)
        {
            foreach(var arg in args)
            {
                switch(arg.ToLower())
                {
                    case "-ssd":
                        BenchmarkRunner.Run<SSDHashPerformanceBenchmarks>();
                        return;
                    case "-ml":
                        BenchmarkRunner.Run<PipelinePerformanceBenchmarks>();
                        return;
                    case "-v":
                        var argIndex = Array.IndexOf(args, "-v");
                        var hashIndex = argIndex + 1;
                        if(hashIndex >= args.Length)
                        {
                            Console.WriteLine("No hash supplied with --hash");
                            return;
                        }
                        var hash = args[hashIndex];
                        var isValid = IsHashValid(hash);
                        Console.WriteLine(hash + " : " + (isValid ? "Valid" : "Invalid"));
                        return;
                    default:
                        InvalidArguement();
                        return;
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
                    -c|                 - Trains a model using KMeans clustering
                Variables
                    --dataset           - File path for training/test dataset in CSV format
            predict:
                    Arguments:
                    -b|                 - Predicts the class of a hash record using a binary classification model  (benign, malicious)
                    -m|                 - Predicts the class of a hash record using a multiclass classification model (c&c, dos etc.)
                Variables
                    --dataset           - File path for model inputs in CSV format
            pipeline:   - Takes fuzzy hash inputs and performs multi-stage classification, outputting malicious records and their behaviour type in file output
                Arguments:
                    -o|                 - Output file name
                Variables
                    --dataset           - File path for model inputs in CSV format
            hash:   - Hashes an input using SSDHash
                Arguments:
                    -s|                 - Permits hashing of single records through an interactive session
                    -m|                 - Hashes all records in the given dataset file
                Variables
                    --dataset           - File path for model inputs in JSON,XML or CSV format
            test:
                Arguements:
                    -ssd                - Runs performance tests related to SSDHash (Performance tests must be run in Release mode)
                    -ml                 - Runs performance tests related to the ML classifiers (Performance tests must be run in Release mode)
                    -v                  - Verify a hash is valid - Supply hash with --hash
                Variables:
                    --hash              - Hash input for validation
            """);
        }


        #endregion


        #region Commands

        private static Action InvalidArguement = () => Console.WriteLine("Invalid arguement supplied. Get help by running --help");
        private static Action PrintInvalidFileText = () => Console.WriteLine("Invalid file path arguement supplied. Provide the path to the dataset file using --dataset PATH/TO/FILE");

        private static void TrainBinaryModel(string[] trainArgs) => TrainModel(trainArgs, new BinaryHashAnalysisTrainer());

        private static void TrainMulticlassModel(string[] trainArgs) => TrainModel(trainArgs, new MulticlassHashAnalysisTrainer());

        private static void TrainClusteringModel(string[] trainArgs) => TrainModel(trainArgs, new ClusteringHashAnalysisTrainer());

        private static void TrainModel(string[] trainArgs, HashAnalysisTrainer trainer)
        {
            string? filePath = GetDatasetPath(trainArgs);

            if (filePath is null)
            {
                PrintInvalidFileText();
                return;
            }

            trainer.TrainModel(filePath);
        }


        private static Dictionary<string, BinaryHashPrediction>? PredictBinaryModel(string[] trainArgs)
        {
            var transformer = new HashClassificationPredictor();
            string ? filePath = GetDatasetPath(trainArgs); ;

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
            string? filePath = GetDatasetPath(trainArgs);

            if (filePath is null)
            {
                PrintInvalidFileText();
                return null;
            }

            return transformer.PredictLabeledMulticlass(filePath);
        }

        private static string? GetDatasetPath(string[] args)
        {
            string? filePath = null;
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "--dataset")
                {
                    if (i < args.Length - 1 && File.Exists(args[i + 1]))
                    {
                        filePath = args[i + 1];
                        break;
                    }
                    else
                    {
                        PrintInvalidFileText();
                    }
                }
            }
            return filePath;
        }

        private static void WriteCsv(object[] outputs, string outputFileName = "out")
        {
            try
            {
                using (var sw = new StreamWriter(outputFileName+".csv"))
                {
                    using (var csvw = new CsvWriter(sw, CultureInfo.InvariantCulture))
                    {
                        csvw.WriteRecords(outputs);
                    }
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine("Error writing output to file. Check the output path is valid.");
            }
            
        }

        private static bool IsHashValid(string hash)
        {
            var allowedChars = new[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F' };
            var standardHash = hash.ToUpper();
            if(hash.Length == 64)
            {
                if(hash.All(c => allowedChars.Contains(c)))
                { 
                    return true; 
                }
            }
            return false;
        }

        #endregion
    }
}