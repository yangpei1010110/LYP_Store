dotnet standard 2.0 - Thread Safe Zero Alloc Query Dictionary In Memory
1. add or update data by key and cache the handle (alloc)
2. remove data by handle cache (non alloc)
3. try query data by handle cache (non alloc)
4. try update data by handle cache (non alloc)

How To Use This:
Example:
```csharp
var store = new LStore<string, int>();

// add or update
var addOrUpdateResult = store.TryAddOrUpdate("empty", 0, out var handle);

// query
var queryResult = store.TryQuery(handle, out var emptyString);

// update
var updateResult = store.TryUpdate(handle, 123);

// remove
var removeResult = store.TryRemove(handle);
```

Benchmark Alloc Test
```Benchmark
| Method      | TestDataCount | RandomReadCount | Mean         | Allocated |
|------------ |-------------- |---------------- |-------------:|----------:|
| QueryAlloc  | 10000         | 100             |     6.734 us |     112 B |
| AddAlloc    | 10000         | 100             |    13.863 us |     400 B |
| UpdateAlloc | 10000         | 100             |    17.820 us |     400 B |
| RemoveAlloc | 10000         | 100             |     4.847 us |     400 B |
| QueryAlloc  | 10000         | 1000            |    39.948 us |     400 B |
| AddAlloc    | 10000         | 1000            |   113.635 us |     400 B |
| UpdateAlloc | 10000         | 1000            |   135.755 us |     400 B |
| RemoveAlloc | 10000         | 1000            |    33.922 us |     400 B |
| QueryAlloc  | 10000         | 10000           |   285.521 us |     400 B |
| AddAlloc    | 10000         | 10000           |   994.240 us |     400 B |
| UpdateAlloc | 10000         | 10000           | 1,019.569 us |     400 B |
| RemoveAlloc | 10000         | 10000           |   270.475 us |     400 B |
| QueryAlloc  | 100000        | 100             |    12.256 us |     400 B |
| AddAlloc    | 100000        | 100             |    38.415 us |     400 B |
| UpdateAlloc | 100000        | 100             |    68.774 us |     400 B |
| RemoveAlloc | 100000        | 100             |    14.312 us |     400 B |
| QueryAlloc  | 100000        | 1000            |   116.013 us |     400 B |
| AddAlloc    | 100000        | 1000            |   271.959 us |     400 B |
| UpdateAlloc | 100000        | 1000            |   542.766 us |     400 B |
| RemoveAlloc | 100000        | 1000            |   111.101 us |     400 B |
| QueryAlloc  | 100000        | 10000           |   835.590 us |     400 B |
| AddAlloc    | 100000        | 10000           | 1,877.242 us |     400 B |
| UpdateAlloc | 100000        | 10000           | 2,875.943 us |     400 B |
| RemoveAlloc | 100000        | 10000           |   741.581 us |     400 B |
```

Benchmark Performance With Dictionary
```Benchmark
| Method                   | TestDataCount | RandomReadCount | Mean        | Ratio | Allocated | Alloc Ratio |
|------------------------- |-------------- |---------------- |------------:|------:|----------:|------------:|
| DefaultDicWithKeyCache   | 100000        | 100             |    15.63 us |  1.00 |     400 B |        1.00 |
| DefaultDicWithKeyCombine | 100000        | 100             |    28.80 us |  1.95 |    3600 B |        9.00 |
| LStoreWithHandleCache    | 100000        | 100             |    14.29 us |  0.95 |     400 B |        1.00 |
| LStoreWithKeyCache       | 100000        | 100             |    43.73 us |  2.94 |     400 B |        1.00 |
| LStoreWithKeyCombine     | 100000        | 100             |    46.87 us |  3.14 |    3600 B |        9.00 |
|                          |               |                 |             |       |           |             |
| DefaultDicWithKeyCache   | 100000        | 1000            |   130.81 us |  1.00 |     400 B |        1.00 |
| DefaultDicWithKeyCombine | 100000        | 1000            |   229.44 us |  1.83 |   32240 B |       80.60 |
| LStoreWithHandleCache    | 100000        | 1000            |   122.82 us |  0.99 |     400 B |        1.00 |
| LStoreWithKeyCache       | 100000        | 1000            |   361.99 us |  2.88 |     400 B |        1.00 |
| LStoreWithKeyCombine     | 100000        | 1000            |   356.52 us |  2.84 |   32272 B |       80.68 |
|                          |               |                 |             |       |           |             |
| DefaultDicWithKeyCache   | 100000        | 10000           |   573.35 us |  1.00 |     400 B |        1.00 |
| DefaultDicWithKeyCombine | 100000        | 10000           | 1,287.20 us |  2.31 |  319408 B |      798.52 |
| LStoreWithHandleCache    | 100000        | 10000           |   641.94 us |  1.14 |     400 B |        1.00 |
| LStoreWithKeyCache       | 100000        | 10000           | 1,971.33 us |  3.55 |     400 B |        1.00 |
| LStoreWithKeyCombine     | 100000        | 10000           | 2,102.66 us |  3.76 |  319440 B |      798.60 |
|                          |               |                 |             |       |           |             |
| DefaultDicWithKeyCache   | 1000000       | 100             |    36.34 us |  1.00 |     400 B |        1.00 |
| DefaultDicWithKeyCombine | 1000000       | 100             |    38.17 us |  1.06 |    4368 B |       10.92 |
| LStoreWithHandleCache    | 1000000       | 100             |    23.57 us |  0.65 |     400 B |        1.00 |
| LStoreWithKeyCache       | 1000000       | 100             |    85.21 us |  2.36 |     400 B |        1.00 |
| LStoreWithKeyCombine     | 1000000       | 100             |    71.44 us |  1.97 |    4632 B |       11.58 |
|                          |               |                 |             |       |           |             |
| DefaultDicWithKeyCache   | 1000000       | 1000            |   286.80 us |  1.00 |     400 B |        1.00 |
| DefaultDicWithKeyCombine | 1000000       | 1000            |   324.37 us |  1.14 |   39640 B |       99.10 |
| LStoreWithHandleCache    | 1000000       | 1000            |   182.50 us |  0.64 |     400 B |        1.00 |
| LStoreWithKeyCache       | 1000000       | 1000            |   643.27 us |  2.25 |      64 B |        0.16 |
| LStoreWithKeyCombine     | 1000000       | 1000            |   515.39 us |  1.75 |   39592 B |       98.98 |
|                          |               |                 |             |       |           |             |
| DefaultDicWithKeyCache   | 1000000       | 10000           | 1,607.79 us |  1.00 |     400 B |        1.00 |
| DefaultDicWithKeyCombine | 1000000       | 10000           | 2,098.46 us |  1.30 |  392288 B |      980.72 |
| LStoreWithHandleCache    | 1000000       | 10000           | 1,073.95 us |  0.74 |     400 B |        1.00 |
| LStoreWithKeyCache       | 1000000       | 10000           | 4,732.32 us |  2.89 |     688 B |        1.72 |
| LStoreWithKeyCombine     | 1000000       | 10000           | 3,994.94 us |  2.48 |  392144 B |      980.36 |
```

Benchmark Alloc Test Code
```csharp
[MemoryDiagnoser]
public class BenchmarkDicAlloc
{
    [Params(10_000, 100_000)]
    public int TestDataCount;
    [Params(100, 1_000, 10_000)]
    public int RandomReadCount;
    public int[]             RandomReadIndexes;
    public string[]          KeyCache;
    public int[]             Data;
    public int[]             UpdateData;
    public int[]             Result;

    public LHandle<string>[] QueryHandleArr;
    public LStore<string, int> QueryStore;
    public LHandle<string>[] AddHandleArr;
    public LStore<string, int> AddStore;
    public LHandle<string>[] UpdateHandleArr;
    public LStore<string, int> UpdateStore;
    public LHandle<string>[] RemoveHandleArr;
    public LStore<string, int> RemoveStore;

    [GlobalSetup]
    public void GlobalSetup()
    {
        GC.Collect();
        QueryStore = new LStore<string, int>();
        AddStore = new LStore<string, int>();
        UpdateStore = new LStore<string, int>();
        RemoveStore = new LStore<string, int>();
        RandomReadIndexes = new int[RandomReadCount];
        Data = new int[TestDataCount];
        UpdateData = new int[TestDataCount];
        KeyCache = new string[TestDataCount];
        Result = new int[TestDataCount];
        QueryHandleArr = new LHandle<string>[TestDataCount];
        AddHandleArr = new LHandle<string>[TestDataCount];
        UpdateHandleArr = new LHandle<string>[TestDataCount];
        RemoveHandleArr = new LHandle<string>[TestDataCount];
        Random random = new();
        for (int i = 0; i < RandomReadCount; i++)
        {
            RandomReadIndexes[i] = random.Next(0, TestDataCount);
        }

        for (int i = 0; i < TestDataCount; i++)
        {
            Data[i] = i * 2;
            UpdateData[i] = random.Next(0, 100);
            KeyCache[i] = i.ToString();
            if (!QueryStore.TryAddOrUpdate(KeyCache[i], Data[i], out QueryHandleArr[i]))
            {
                throw new Exception("_store.TryAddOrUpdate fail");
            }
            
            if (!AddStore.TryAddOrUpdate(KeyCache[i], Data[i], out AddHandleArr[i]))
            {
                throw new Exception("_store.TryAddOrUpdate fail");
            }
            
            if (!UpdateStore.TryAddOrUpdate(KeyCache[i], Data[i], out UpdateHandleArr[i]))
            {
                throw new Exception("_store.TryAddOrUpdate fail");
            }
            
            if (!RemoveStore.TryAddOrUpdate(KeyCache[i], Data[i], out RemoveHandleArr[i]))
            {
                throw new Exception("_store.TryAddOrUpdate fail");
            }
        }

        AddStore.Clear();
        GC.Collect();
    }

    [IterationSetup]
    public void IterationSetup()
    {
        GC.Collect();
    }

    [IterationCleanup]
    public void IterationCleanup()
    {
        GC.Collect();
    }

    [Benchmark]
    public void QueryAlloc()
    {
        for (int i = 0; i < RandomReadCount; i++)
        {
            int index = RandomReadIndexes[i];
            LHandle<string> handle = QueryHandleArr[index];
            if (QueryStore.TryQuery(handle, out int data))
            {
                Result[index] = data;
            }
        }
    }
    
    [Benchmark]
    public void AddAlloc()
    {
        for (int i = 0; i < RandomReadCount; i++)
        {
            int index = RandomReadIndexes[i];
            AddStore.TryAddOrUpdate(KeyCache[index], UpdateData[index], out AddHandleArr[index]);
        }
    }
    
    [Benchmark]
    public void UpdateAlloc()
    {
        for (int i = 0; i < RandomReadCount; i++)
        {
            int index = RandomReadIndexes[i];
            UpdateStore.TryAddOrUpdate(KeyCache[index], UpdateData[index], out UpdateHandleArr[index]);
        }
    }
    
    [Benchmark]
    public void RemoveAlloc()
    {
        for (int i = 0; i < RandomReadCount; i++)
        {
            int index = RandomReadIndexes[i];
            RemoveStore.TryRemove(RemoveHandleArr[index]);
        }
    }
}
```

Benchmark Performance With Dictionary code
```csharp
[MemoryDiagnoser]
public class BenchmarkDicAlloc
{
    [Params(10_000, 100_000)]
    public int TestDataCount;
    [Params(100, 1_000, 10_000)]
    public int RandomReadCount;
    public int[]             RandomReadIndexes;
    public string[]          KeyCache;
    public int[]             Data;
    public int[]             UpdateData;
    public int[]             Result;

    public LHandle<string>[] QueryHandleArr;
    public LStore<string, int> QueryStore;
    public LHandle<string>[] AddHandleArr;
    public LStore<string, int> AddStore;
    public LHandle<string>[] UpdateHandleArr;
    public LStore<string, int> UpdateStore;
    public LHandle<string>[] RemoveHandleArr;
    public LStore<string, int> RemoveStore;

    [GlobalSetup]
    public void GlobalSetup()
    {
        GC.Collect();
        QueryStore = new LStore<string, int>();
        AddStore = new LStore<string, int>();
        UpdateStore = new LStore<string, int>();
        RemoveStore = new LStore<string, int>();
        RandomReadIndexes = new int[RandomReadCount];
        Data = new int[TestDataCount];
        UpdateData = new int[TestDataCount];
        KeyCache = new string[TestDataCount];
        Result = new int[TestDataCount];
        QueryHandleArr = new LHandle<string>[TestDataCount];
        AddHandleArr = new LHandle<string>[TestDataCount];
        UpdateHandleArr = new LHandle<string>[TestDataCount];
        RemoveHandleArr = new LHandle<string>[TestDataCount];
        Random random = new();
        for (int i = 0; i < RandomReadCount; i++)
        {
            RandomReadIndexes[i] = random.Next(0, TestDataCount);
        }

        for (int i = 0; i < TestDataCount; i++)
        {
            Data[i] = i * 2;
            UpdateData[i] = random.Next(0, 100);
            KeyCache[i] = i.ToString();
            if (!QueryStore.TryAddOrUpdate(KeyCache[i], Data[i], out QueryHandleArr[i]))
            {
                throw new Exception("_store.TryAddOrUpdate fail");
            }
            
            if (!AddStore.TryAddOrUpdate(KeyCache[i], Data[i], out AddHandleArr[i]))
            {
                throw new Exception("_store.TryAddOrUpdate fail");
            }
            
            if (!UpdateStore.TryAddOrUpdate(KeyCache[i], Data[i], out UpdateHandleArr[i]))
            {
                throw new Exception("_store.TryAddOrUpdate fail");
            }
            
            if (!RemoveStore.TryAddOrUpdate(KeyCache[i], Data[i], out RemoveHandleArr[i]))
            {
                throw new Exception("_store.TryAddOrUpdate fail");
            }
        }

        AddStore.Clear();
        GC.Collect();
    }

    [IterationSetup]
    public void IterationSetup()
    {
        GC.Collect();
    }

    [IterationCleanup]
    public void IterationCleanup()
    {
        GC.Collect();
    }

    [Benchmark]
    public void QueryAlloc()
    {
        for (int i = 0; i < RandomReadCount; i++)
        {
            int index = RandomReadIndexes[i];
            LHandle<string> handle = QueryHandleArr[index];
            if (QueryStore.TryQuery(handle, out int data))
            {
                Result[index] = data;
            }
        }
    }
    
    [Benchmark]
    public void AddAlloc()
    {
        for (int i = 0; i < RandomReadCount; i++)
        {
            int index = RandomReadIndexes[i];
            AddStore.TryAddOrUpdate(KeyCache[index], UpdateData[index], out AddHandleArr[index]);
        }
    }
    
    [Benchmark]
    public void UpdateAlloc()
    {
        for (int i = 0; i < RandomReadCount; i++)
        {
            int index = RandomReadIndexes[i];
            UpdateStore.TryAddOrUpdate(KeyCache[index], UpdateData[index], out UpdateHandleArr[index]);
        }
    }
    
    [Benchmark]
    public void RemoveAlloc()
    {
        for (int i = 0; i < RandomReadCount; i++)
        {
            int index = RandomReadIndexes[i];
            RemoveStore.TryRemove(RemoveHandleArr[index]);
        }
    }
}
```
