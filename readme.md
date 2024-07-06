LineSorterApp
===

This is a simple example for an external sorting algorithm which can sort big sets of data in the format:

```
[Number]. [Word] [Word]... [WordN]
```

Usage
---

Generator

```
$ ./LineSorterGeneratorApp -h
Description:
  Application generating [num]. [text] line filled text files

Usage:
  LineSorterGeneratorApp [options]

Options:
  --output <output>            Path pointing where to put the results. Will be overwritten [default: output.txt]
  --target-size <target-size>  Target file size in bytes. Note: output can be a bit bigger just to avoid cutting the line content [default: Physical RAM/2]
  --dictionary <dictionary>    Dictionary of words separated with new lines that will be used to generate the result file. If not specified the bundled 
                               one will be used [default: words.txt]
  --version                    Show version information
  -?, -h, --help               Show help and usage information
```

Sorter

```
$ ./LineSorterApp -h
Description:
  Application for sorting [num]. [text] line filled text files

Usage:
  LineSorterApp [options]

Options:
  --input <input> (REQUIRED)     Path pointing to the the unsorted file
  --output <output>              Path pointing where to put the results. Will be overwritten [default: sorted.txt]
  --memory-limit <memory-limit>  Memory limit for the process in bytes. If not supplied half of available RAM will be used [default: 16784545792]
  --version                      Show version information
  -?, -h, --help                 Show help and usage information
```

Results
---

The result for a 1GB set of data is as follows

![Result of operation on 1GB set of data](./img/result-1gb.png)

19 seconds run time on a AMD Ryzen 5 3600 with a NVMe drive and 32GB of RAM. A bigger memory than data set size allows for sorting only in memory, but we can modify the `--memory-limit` parameter to split the input data in to chunks that will fit in memory.

Results for 100GB file TODO


Requirements
---

The only dependancy the apps have is the `System.CommandLine` library for easy cli argument parsing.

License
---
MIT