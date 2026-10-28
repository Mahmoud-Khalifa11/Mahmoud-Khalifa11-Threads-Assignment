using OrderProcessing.Shared.Logging;
using OrderProcessing.Shared.Models;

namespace OrderProcessing.Phase1;

public sealed class OrderValidator
{
    private const string Who = nameof(OrderValidator);

    private readonly CustomerRepository _customers;
    private readonly ProductRepository _products;
    private readonly CouponRepository _coupons;

    public OrderValidator(
        CustomerRepository customers,
        ProductRepository products,
        CouponRepository coupons)
    {
        _customers = customers;
        _products = products;
        _coupons = coupons;
    }

    public async Task<bool> ValidateOrderAsync(Order order, CancellationToken ct)
    {
        string? failure = await FindFailureAsync(order, ct);

        if (failure is null)
        {
            order.Status = OrderStatus.Validated;
            order.FailureReason = null;
            Log.Success($"{Who} -> Order #{order.Id} validated");
            return true;
        }

        order.Status = OrderStatus.Rejected;
        order.FailureReason = failure;
        Log.Error($"{Who} -> Order #{order.Id} rejected: {failure}");
        return false;
    }

    // Returns the first failure reason, or null when every check passes.
    private async Task<string?> FindFailureAsync(Order order, CancellationToken ct)
    {
        // 1) customer exists   2) customer is active
        Customer? customer = await _customers.GetByIdAsync(order.CustomerId, ct);
        if (customer is null)
            return $"Customer #{order.CustomerId} not found";
        if (!customer.IsActive)
            return "Customer is inactive";

        var lines = new List<(OrderItem Item, Product Product)>();
        foreach (OrderItem item in order.Items)
        {
            Product? product = await _products.GetByIdAsync(item.ProductId, ct);
            if (product is null)
                return $"Product #{item.ProductId} not found";
            lines.Add((item, product));
        }

        // 4) enough stock (checked only, not reserved)
        foreach ((OrderItem item, Product product) in lines)
        {
            if (product.StockQuantity < item.Quantity)
                return $"Insufficient stock for '{product.Name}'";
        }

        if (order.CouponCode is not null)
        {
            Coupon? coupon = await _coupons.GetByCodeAsync(order.CouponCode, ct);
            if (coupon is null)
                return $"Coupon '{order.CouponCode}' does not exist";
            if (coupon.ExpiryDate < DateTime.Today)
                return $"Coupon '{order.CouponCode}' has expired";
        }

        return null;
    }
}