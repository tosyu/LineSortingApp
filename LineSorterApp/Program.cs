﻿using System.Diagnostics;
using System.CommandLine;

using LineSorterApp.Helpers;
using LineSorterApp.DataStructrues;

namespace LineSorterApp;

class Program
{
    public static async Task<int> Main(string[] args)
    {
        var inputFileOption = new Option<FileInfo>(
            name: "--input",
            description: "Path pointing to the the unsorted file"
        )
        {
            IsRequired = true
        };

        var outputFileOption = new Option<FileInfo>(
            name: "--output",
            () => new FileInfo("sorted.txt"),
            description: "Path pointing where to put the results. Will be overwritten"
        );

        var memoryLimitOption = new Option<long>(
            name: "--memory-limit",
            () => FetchAvailableMemory(),
            description: "Memory limit for the process in bytes. If not supplied quarter of available RAM will be used"
        );

        var rootCommand = new RootCommand("Application for sorting [num]. [text] line filled text files");
        rootCommand.AddOption(inputFileOption);
        rootCommand.AddOption(outputFileOption);
        rootCommand.AddOption(memoryLimitOption);

        rootCommand.SetHandler(Run, inputFileOption, outputFileOption, memoryLimitOption);

        return await rootCommand.InvokeAsync(args);
    }

    static long FetchAvailableMemory()
    {
        var memoryInfo = GC.GetGCMemoryInfo();
        return memoryInfo.TotalAvailableMemoryBytes / 4;
    }

    static void Run(FileInfo inputFile, FileInfo outputFile, long memoryLimit)
    {
        var stopWatch = new Stopwatch();
        stopWatch.Start();

        var temporaryFiles = inputFile.SplitBySize(memoryLimit);

        temporaryFiles.SortSplits();
        temporaryFiles.MergeSplitsInto(outputFile);

        temporaryFiles.ForEach(file => File.Delete(file.FullName));

        stopWatch.Stop();
        Console.WriteLine($"Done in {stopWatch.Elapsed}");

    }
}