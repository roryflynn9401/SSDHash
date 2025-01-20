# SSDHash & Log Analysis Tool
# Paper
https://www.sciencedirect.com/science/article/pii/S016740482500001X?via%3Dihub
# Features
- Hash Files or string inputs using SSDHash
- Train Binary/Multiclass Models on the fuzzy hash outputs
- Use trained models to make predictions
- Run Performance benchmarks on both the algorithm and ml components
- Verify a hash is valid

# Requirements
## Framework
- .Net 7.0
- Nvidia CUDA toolkit

## Packages
- ML.Net
- Microsoft.ML.Torchsharp
- TorchSharp-cuda-windows or TorchSharp-cpu-windows (or their linux equivalents)
- BenchmarkDotnet (For Performance benchmarks)
- Scottplot (for visualizations)
- NUnit
- Json.Net
- CsvHelper

# Usage
```yaml
 -h|--help : Help Menu,

Commands:
train 
    Arguments:
        -b|                 - Trains a model using binary classification (benign, malicious)
        -m|                 - Trains a model using multiclass classification (c&c, dos etc.)
        -c|                 - Trains a model using KMeans clustering
    Variables
        --dataset           - File path for training/test dataset in CSV format
predict: - Predict using each model individually
    Arguments:
        -b|                 - Predicts the class of a hash record using a binary classification model  (benign, malicious)
        -m|                 - Predicts the class of a hash record using a multiclass classification model (c&c, dos etc.)
        -l|                 - Predicts the class of the chosen model using a labelled dataset, outputting relevant accuracy metrics
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
test: - Various performance or validation tests
    Arguments:
        -ssd                - Runs performance tests related to SSDHash (Performance tests must be run in Release mode)
        -ml                 - Runs performance tests related to the ML classifiers (Performance tests must be run in Release mode)
        -v                  - Verify a hash is valid - Supply hash with --hash
    Variables:
                    --hash              - Hash input for validation


