using OrderProcessing.Shared.Logging;

namespace 
OrderProcessing.Shared;

// Simulated latency steps (6.5). Each step logs a line, then waits.
public static class Latency
{
    public static async Task OpenConnectionAsync(string source, CancellationToken ct)
    {
        Log.Info($"{source} -> Opening connection...");
        await Task.Delay(200, ct);
    }

    public static async Task ExecuteQueryAsync(string source, string what, CancellationToken ct)
    {
        Log.Info($"{source} -> Executing query for {what}...");
        await Task.Delay(Random.Shared.Next(300, 601), ct);
    }

    public static async Task BeginTransactionAsync(string source, CancellationToken ct)
    {
        Log.Info($"{source} -> Beginning transaction...");
        await Task.Delay(100, ct);
    }

    public static async Task InsertOrUpdateAsync(string source, string what, CancellationToken ct)
    {
        Log.Info($"{source} -> Writing {what}...");
        await Task.Delay(400, ct);
    }

    public static async Task CommitAsync(string source, CancellationToken ct)
    {
        Log.Info($"{source} -> Committing...");
        await Task.Delay(200, ct);
  
































  }

}

