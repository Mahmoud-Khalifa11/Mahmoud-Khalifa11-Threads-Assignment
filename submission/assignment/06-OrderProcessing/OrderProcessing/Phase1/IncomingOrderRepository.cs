using OrderProcessing.Shared;
using OrderProcessing.Shared.Logging;
using OrderProcessing.Shared.Models;
using OrderProcessing.Shared.Storage;

namespace OrderProcessing.Phase1;

public sealed class IncomingOrderRepository
{
    private const string Who = nameof(IncomingOrderRepository);
    private readonly JsonFileStore _store;

    public IncomingOrderRepository(JsonFileStore store) => _store = store;

    public async Task<List<Order>> GetAllAsync(CancellationToken ct)
    {
        await Latency.OpenConnectionAsync(Who, ct);
        await Latency.ExecuteQueryAsync(Who, "incoming Orders", ct);

        List<Order> all = await _store.ReadAllAsync<Order>(DataFiles.IncomingOrders, ct);
        Log.Success($"{Who} -> {all.Count} incoming orders loaded");
        return all;
    }
}