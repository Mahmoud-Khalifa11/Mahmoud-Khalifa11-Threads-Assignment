using OrderProcessing.Shared;
using OrderProcessing.Shared.Logging;
using OrderProcessing.Shared.Models;
using OrderProcessing.Shared.Storage;

namespace OrderProcessing.Phase1;

public sealed class SyncCouponRepository
{
    private const string Who = nameof(SyncCouponRepository);
    private readonly JsonFileStore _store;

    public SyncCouponRepository(JsonFileStore store) => _store = store;

    public Coupon? GetByCode(string code)
    {
        SyncLatency.OpenConnection(Who);
        SyncLatency.ExecuteQuery(Who, $"Coupon '{code}'");

        List<Coupon> all = _store.ReadAll<Coupon>(DataFiles.Coupons);
        Coupon? found = all.FirstOrDefault(c =>
            string.Equals(c.Code, code, StringComparison.OrdinalIgnoreCase));

        if (found is null)
            Log.Info($"{Who} -> Coupon '{code}' not found");
        else
            Log.Success($"{Who} -> Coupon '{code}' found ({found.DiscountPercent}%)");



























        return found;
    }
}
