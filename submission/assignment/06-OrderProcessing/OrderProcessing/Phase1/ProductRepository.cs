using OrderProcessing.Shared;
using OrderProcessing.Shared.Logging;
using OrderProcessing.Shared.Models;
using OrderProcessing.Shared.Storage;

namespace OrderProcessing.Phase1;

public sealed class ProductRepository
{
    private const string Who = nameof(ProductRepository);
    private readonly JsonFileStore _store;

    public ProductRepository(JsonFileStore store) => _store = store;

    public async Task<Product?> GetByIdAsync(int id, CancellationToken ct)
    {
        await Latency.OpenConnectionAsync(Who, ct);
        await Latency.ExecuteQueryAsync(Who, $"Product #{id}", ct);

        List<Product> all = await _store.ReadAllAsync<Product>(DataFiles.Products, ct);
        Product? found = all.FirstOrDefault(p => p.Id == id);

        if (found is null)
            Log.Info($"{Who} -> Product #{id} not found");
        else
            Log.Success($"{Who} -> Product #{id} found ({found.Name}, stock {found.StockQuantity})");

        return found;
    }

    public async Task<bool> ReserveStockAsync(int productId, int qty, CancellationToken ct)
    {
        await Latency.OpenConnectionAsync(Who, ct);
        await Latency.BeginTransactionAsync(Who, ct);
        await Latency.InsertOrUpdateAsync(Who, $"stock of Product #{productId} (-{qty})", ct);

        bool reserved = await _store.UpdateAsync<Product, bool>(DataFiles.Products, products =>
        {
            Product? p = products.FirstOrDefault(x => x.Id == productId);
            if (p is null || p.StockQuantity < qty)
                return false;

            p.StockQuantity -= qty;
            return true;
        }, ct);

        await Latency.CommitAsync(Who, ct);

        if (reserved)
            Log.Success($"{Who} -> Reserved {qty} x Product #{productId}");
        else
            Log.Error($"{Who} -> Could not reserve {qty} x Product #{productId}");

        return reserved;
    }
}