// See https://aka.ms/new-console-template for more information

using FASTER.core;
using SimpleChecks.Data;

const string baseDir = "FasterData";
const int keyMod = 10;

Console.WriteLine("Press any key to start");
Console.ReadKey();
    
while (true)
{
    var config = new FasterKVSettings<Key, Value>(baseDir)
    {
        TryRecoverLatest = true,
        RemoveOutdatedCheckpoints = true,
        MemorySize = 1L << 10,
        PageSize = 1L << 9,
        EqualityComparer = new Key.Comparer()
    };
    var store = new FasterKV<Key, Value>(config);
    
    ReadValues(store,"Before populate");

    PopulateStore(store);

    store.Log.FlushAndEvict(false);
    ReadValues(store,"After populate");

    store.TakeHybridLogCheckpointAsync(CheckpointType.Snapshot).GetAwaiter().GetResult();
    store.Dispose();
    config.Dispose();
    
    Console.WriteLine("Press any key to resume and 'q' to exist");
    if (Console.ReadKey().KeyChar == 'q')
    {
        break;
    }
}

void PopulateStore(FasterKV<Key, Value> store)
{
    using var session = store.NewSession(new SimpleFunctions<Key, Value>());
    var random = new Random();
    for (var ii = 0; ii < 100; ii++)
    {
        var key = new Key(ii % keyMod);
        var value = new Value(key.key + random.Next(keyMod));
        session.Upsert(key, value);
    }
}

void ReadValues(IFasterKV<Key, Value> store, string actionComment)
{
    using var session = store.NewSession(new SimpleFunctions<Key, Value>());
    for (int ii = 0; ii < 100; ii++)
    {
        var key = new Key(ii % keyMod);
        var status = session.Read(key, out var readedValue);
        if (status.Found)
        {
            Console.WriteLine($"{key} => {readedValue} found right way in {actionComment}");
        }
        if (status.IsPending)
        {
            session.CompletePendingWithOutputs(out var iterator, true);
            using (iterator)
            {
                while (iterator.Next())
                {
                    if (iterator.Current.Status.Found)
                    {
                        readedValue = iterator.Current.Output;
                        Console.WriteLine($"{key} => {readedValue} found by iterator in {actionComment}");
                    }
                }
            }
        }
        if (status.NotFound)
        {
            Console.WriteLine($"{key} =>  not found in {actionComment}");
        }
    }
}