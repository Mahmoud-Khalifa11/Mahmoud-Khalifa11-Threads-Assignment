using OrderProcessing.Shared;
using OrderProcessing.Shared.Logging;
using OrderProcessing.Shared.Models;
using OrderProcessing.Shared.Storage;

namespace OrderProcessing.Phase1;

public sealed class CustomerRepository
{
    private const string Who = nameof(CustomerRepository);
    private readonly JsonFileStore _store;

    public CustomerRepository(JsonFileStore store) => _store = store;

    public async Task<Customer?> GetByIdAsync(int id, CancellationToken ct)
    {
        await Latency.OpenConnectionAsync(Who, ct);
        await Latency.ExecuteQueryAsync(Who, $"Customer #{id}", ct);

        List<Customer> all = await _store.ReadAllAsync<Customer>(DataFiles.Customers, ct);
        Customer? found = all.FirstOrDefault(c => c.Id == id);

        if (found is null)
            Log.Info($"{Who} -> Customer #{id} not found");
        else
            Log.Success($"{Who} -> Customer #{id} found ({found.Name})");

        return found;
    }
}
