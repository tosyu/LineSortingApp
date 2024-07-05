LineSorterApp
===

This is a simple example for an external sorting algorithm which can sort big sets of data in the format:

```
[Number]. [Word] [Word]... [WordN]
```

The result for a 1GB set of data is as follows

![Result of operation on 1GB set of data](./img/result-1gb.png)

19 seconds run time on a AMD Ryzen 5 3600 with a NVMe drive and 32GB of RAM. A bigger memory than data set size allows for sorting only in memory, but we can modify the `--memory-limit` parameter to split the input data in to chunks that will fit in memory.

Results for 100GB file TODO
