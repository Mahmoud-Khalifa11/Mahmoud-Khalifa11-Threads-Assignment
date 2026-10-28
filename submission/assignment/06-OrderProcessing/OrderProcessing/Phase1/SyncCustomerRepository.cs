using OrderProcessing.Shared;
using OrderProcessing.Shared.Logging;
using OrderProcessing.Shared.Models;
using OrderProcessing.Shared.Storage;

namespace OrderProcessing.Phase1;

public sealed class SyncCustomerRepository
{
    private const string Who = nameof(SyncCustomerRepository);
    private readonly JsonFileStore _store;

    public SyncCustomerRepository(JsonFileStore store) => _store = store;

    public Customer? GetById(int id)
    {
        SyncLatency.OpenConnection(Who);
        SyncLatency.ExecuteQuery(Who, $"Customer #{id}");

        List<Customer> all = _store.ReadAll<Customer>(DataFiles.Customers);
        Customer? found = all.FirstOrDefault(c => c.Id == id);

        if (found is null)
            Log.Info($"{Who} -> Customer #{id} not found");
        else
            Log.Success($"{Who} -> Customer #{id} found ({found.Name})");

        return found;
  



























  }

}

