namespace OrderProcessing.Shared.Exceptions;

public sealed class InvalidCouponException(string message) : Exception(message);
