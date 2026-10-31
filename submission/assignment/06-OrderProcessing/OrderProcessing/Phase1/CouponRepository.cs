using OrderProcessing.Shared;
using OrderProcessing.Shared.Logging;
using OrderProcessing.Shared.Models;
using OrderProcessing.Shared.Storage;

namespace OrderProcessing.Phase1;

public sealed class CouponRepository
{
    private const string Who = nameof(CouponRepository);
    private readonly JsonFileStore _store;

    public CouponRepository(JsonFileStore store) => _store = store;

    public async Task<Coupon?> GetByCodeAsync(string code, CancellationToken ct)
    {
        await Latency.OpenConnectionAsync(Who, ct);
        await Latency.ExecuteQueryAsync(Who, $"Coupon '{code}'", ct);

        List<Coupon> all = await _store.ReadAllAsync<Coupon>(DataFiles.Coupons, ct);
        Coupon? found = all.FirstOrDefault(c =>
            string.Equals(c.Code, code, StringComparison.OrdinalIgnoreCase));

        if (found is null)
            Log.Info($"{Who} -> Coupon '{code}' not found");
        else
            Log.Success($"{Who} -> Coupon '{code}' found ({found.DiscountPercent}%)");

        return found;
  




























  }

}

