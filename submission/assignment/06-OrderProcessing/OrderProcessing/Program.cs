using OrderProcessing.Phase1;
using OrderProcessing.Shared.Storage;

namespace OrderProcessing;

internal static class Program
{
    private static async Task Main(string[] args)
    {
        int? cancelAfterMs = ReadCancelAfter(args);

        string? mode = args
            .FirstOrDefault(a => !a.StartsWith("--") && !int.TryParse(a, out _))
            ?.ToLowerInvariant();

        mode ??= AskForMode();
        if (mode is null)
        {
            Console.WriteLine("Nothing to run.");
            return;
        }

        using var cts = new CancellationTokenSource();
        Console.CancelKeyPress += (_, e) =>
        {
            e.Cancel = true;
            cts.Cancel();
        };

        if (cancelAfterMs is int ms)
            cts.CancelAfter(ms);

        var store = new JsonFileStore();
        var seeder = new DataSeeder(store);

        try
        {
            switch (mode)
            {
                case "phase1":
                    await new Phase1Runner(store, seeder).RunAsync(cts.Token);
                    break;

                case "phase2":
                case "phase3":
                case "phase4":
                    Console.WriteLine($"{mode} is not built yet.");
                    break;

                case "all":
                    await new Phase1Runner(store, seeder).RunAsync(cts.Token);
                    break;

                default:
                    Console.WriteLine($"Unknown mode '{mode}'. Use: phase1 | phase2 | phase3 | phase4 | all");
                    break;
            }
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("Run cancelled.");
        }
    }

    private static int? ReadCancelAfter(string[] args)
    {
        int index = Array.IndexOf(args, "--cancel-after");
        if (index >= 0 && index + 1 < args.Length && int.TryParse(args[index + 1], out int ms))
            return ms;
        return null;
    }

    private static string? AskForMode()
    {
        Console.WriteLine("1) Phase 1 - Async data access layer");
        Console.WriteLine("2) Phase 2 - Concurrent processing");
        Console.WriteLine("3) Phase 3 - Full checkout workflow");
        Console.WriteLine("4) Phase 4 - Scale-up");
        Console.WriteLine("5) All phases");
        Console.Write("Choose: ");

        return Console.ReadLine()?.Trim() switch
        {
            "1" => "phase1",
            "2" => "phase2",
            "3" => "phase3",
            "4" => "phase4",
            "5" => "all",
            _ => null
        };
    }
}