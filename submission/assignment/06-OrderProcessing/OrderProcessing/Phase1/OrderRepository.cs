using OrderProcessing.Shared;
using OrderProcessing.Shared.Logging;
using OrderProcessing.Shared.Models;
using OrderProcessing.Shared.Storage;

namespace OrderProcessing.Phase1;

public sealed class OrderRepository
{
    private const string Who = nameof(OrderRepository);
    private readonly JsonFileStore _store;

    public OrderRepository(JsonFileStore store) => _store = store;

    public async Task SaveAsync(Order order, CancellationToken ct)
    {
        await Latency.OpenConnectionAsync(Who, ct);
        await Latency.BeginTransactionAsync(Who, ct);
        await Latency.InsertOrUpdateAsync(Who, $"Order #{order.Id}", ct);

        await _store.UpdateAsync<Order>(DataFiles.Orders, orders =>
        {
            int index = orders.FindIndex(o => o.Id == order.Id);
            if (index >= 0)
                orders[index] = order;
            else
                orders.Add(order);
        }, ct);

        await Latency.CommitAsync(Who, ct);
        Log.Success($"{Who} -> Order #{order.Id} saved ({order.Status})");
    }

    public async Task<List<Order>> GetAllAsync(CancellationToken ct)
    {
        await Latency.OpenConnectionAsync(Who, ct);
        await Latency.ExecuteQueryAsync(Who, "all Orders", ct);

        List<Order> all = await _store.ReadAllAsync<Order>(DataFiles.Orders, ct);
        Log.Success($"{Who} -> {all.Count} orders loaded");
        return all;
    }

    public async Task<List<Order>> GetByStatusAsync(OrderStatus status, CancellationToken ct)
    {
        await Latency.OpenConnectionAsync(Who, ct);
        await Latency.ExecuteQueryAsync(Who, $"Orders with status {status}", ct);

        List<Order> all = await _store.ReadAllAsync<Order>(DataFiles.Orders, ct);
        List<Order> filtered = all.Where(o => o.Status == status).ToList();
        Log.Success($"{Who} -> {filtered.Count} orders with status {status}");
        return filtered;
    }
}