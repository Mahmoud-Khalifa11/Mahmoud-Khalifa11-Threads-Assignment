using OrderProcessing.Shared.Logging;

namespace OrderProcessing.Shared;

// Same steps as Latency, but the thread is BLOCKED while it waits.
public static class SyncLatency
{
    public static void OpenConnection(string source)
    {
        Log.Info($"{source} -> Opening connection...");
        Thread.Sleep(200);
    }

    public static void ExecuteQuery(string source, string what)
    {
        Log.Info($"{source} -> Executing query for {what}...");
        Thread.Sleep(Random.Shared.Next(300, 601));
    }
}
