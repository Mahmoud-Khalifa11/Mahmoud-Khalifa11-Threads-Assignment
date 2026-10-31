using OrderProcessing.Shared.Logging;
using OrderProcessing.Shared.Models;

namespace OrderProcessing.Phase1;

public sealed class SyncOrderValidator
{
    private const string Who = nameof(SyncOrderValidator);

    private readonly SyncCustomerRepository _customers;
    private readonly SyncProductRepository _products;
    private readonly SyncCouponRepository _coupons;

    public SyncOrderValidator(
        SyncCustomerRepository customers,
        SyncProductRepository products,
        SyncCouponRepository coupons)
    {
        _customers = customers;
        _products = products;
        _coupons = coupons;
    }

    // Same logic as OrderValidator, but every wait blocks the thread.
    public bool ValidateOrder(Order order)
    {
        string? failure = FindFailure(order);

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

    private string? FindFailure(Order order)
    {
        Customer? customer = _customers.GetById(order.CustomerId);
        if (customer is null)
            return $"Customer #{order.CustomerId} not found";
        if (!customer.IsActive)
            return "Customer is inactive";

        var lines = new List<(OrderItem Item, Product Product)>();
        foreach (OrderItem item in order.Items)
        {
            Product? product = _products.GetById(item.ProductId);
            if (product is null)
                return $"Product #{item.ProductId} not found";
            lines.Add((item, product));
        }

        foreach ((OrderItem item, Product product) in lines)
        {
            if (product.StockQuantity < item.Quantity)
                return $"Insufficient stock for '{product.Name}'";
        }

        if (order.CouponCode is not null)
        {
            Coupon? coupon = _coupons.GetByCode(order.CouponCode);
            if (coupon is null)
                return $"Coupon '{order.CouponCode}' does not exist";
            if (coupon.ExpiryDate < DateTime.Today)
                return $"Coupon '{order.CouponCode}' has expired";
        }

        return null;
    }
}