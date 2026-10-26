namespace OrderProcessing.Shared.Exceptions;

public sealed class ShippingProviderUnavailableException(string message) : Exception(message);
