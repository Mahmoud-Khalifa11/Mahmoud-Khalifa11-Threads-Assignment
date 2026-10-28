using System.Diagnostics;
using OrderProcessing.Shared.Models;
using OrderProcessing.Shared.Storage;

namespace OrderProcessing.Phase1;

public sealed class Phase1Runner
{
    private readonly JsonFileStore _store;
    private readonly DataSeeder _seeder;

    public Phase1Runner(JsonFileStore store, DataSeeder seeder)
    {
        _store = store;
        _seeder = seeder;
    }

    public async Task RunAsync(CancellationToken ct)
    {
        var incomingRepo = new IncomingOrderRepository(_store);
        var orderRepo = new OrderRepository(_store);

        var syncValidator = new SyncOrderValidator(
            new SyncCustomerRepository(_store),
            new SyncProductRepository(_store),
            new SyncCouponRepository(_store));

        var validator = new OrderValidator(
            new CustomerRepository(_store),
            new ProductRepository(_store),
            new CouponRepository(_store));

        // 1) reset, then load the 20 incoming orders
        Console.WriteLine("=== Phase 1: reset data ===");
        await _seeder.ResetAsync(ct);
        List<Order> orders = await incomingRepo.GetAllAsync(ct);

        // 2) sync run
        Console.WriteLine();
        Console.WriteLine("=== SYNC run (Thread.Sleep, results not saved) ===");
        var syncWatch = Stopwatch.StartNew();
        foreach (Order order in orders)
        {
            ct.ThrowIfCancellationRequested();
            syncValidator.ValidateOrder(order);
        }
        syncWatch.Stop();

        // 3) reset again
        Console.WriteLine();
        Console.WriteLine("=== Phase 1: reset data again ===");
        await _seeder.ResetAsync(ct);
        orders = await incomingRepo.GetAllAsync(ct);

        // 4) async run: await validation
        Console.WriteLine();
        Console.WriteLine("=== ASYNC run (Task.Delay, results saved) ===");
        var asyncWatch = Stopwatch.StartNew();
        var validateWatch = new Stopwatch();
        var saveWatch = new Stopwatch();
        foreach (Order order in orders)
        {
            ct.ThrowIfCancellationRequested();

            validateWatch.Start();
            await validator.ValidateOrderAsync(order, ct);
            validateWatch.Stop();

            saveWatch.Start();
            await orderRepo.SaveAsync(order, ct);
            saveWatch.Stop();
        }
        asyncWatch.Stop();

        Console.WriteLine();
        List<Order> all = await orderRepo.GetAllAsync(ct);
        List<Order> validated = await orderRepo.GetByStatusAsync(OrderStatus.Validated, ct);
        List<Order> rejected = await orderRepo.GetByStatusAsync(OrderStatus.Rejected, ct);

        Console.WriteLine();
        Console.WriteLine("===== Validation Summary =====");
        Console.WriteLine($"Total Orders: {all.Count}");
        Console.WriteLine($"Validated: {validated.Count}");
        Console.WriteLine($"Rejected: {rejected.Count}");
        foreach (Order r in rejected.OrderBy(o => o.Id))
            Console.WriteLine($" - Order #{r.Id}: {r.FailureReason}");
        Console.WriteLine($"Sync Time: {syncWatch.Elapsed.TotalSeconds:F1} seconds");
        Console.WriteLine($"Async Time: {asyncWatch.Elapsed.TotalSeconds:F1} seconds");
        Console.WriteLine($"  (async validation only: {validateWatch.Elapsed.TotalSeconds:F1} s, async saving: {saveWatch.Elapsed.TotalSeconds:F1} s)");
        Console.WriteLine($"orders.json now contains {all.Count} records.");
    }
}