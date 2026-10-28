using OrderProcessing.Shared;
using OrderProcessing.Shared.Logging;
using OrderProcessing.Shared.Models;
using OrderProcessing.Shared.Storage;

namespace OrderProcessing.Phase1;

public sealed class SyncProductRepository
{
    private const string Who = nameof(SyncProductRepository);
    private readonly JsonFileStore _store;

    public SyncProductRepository(JsonFileStore store) => _store = store;

    public Product? GetById(int id)
    {
        SyncLatency.OpenConnection(Who);
        SyncLatency.ExecuteQuery(Who, $"Product #{id}");

        List<Product> all = _store.ReadAll<Product>(DataFiles.Products);
        Product? found = all.FirstOrDefault(p => p.Id == id);

        if (found is null)
            Log.Info($"{Who} -> Product #{id} not found");
        else
            Log.Success($"{Who} -> Product #{id} found ({found.Name}, stock {found.StockQuantity})");

        return found;
  



























  }

}

