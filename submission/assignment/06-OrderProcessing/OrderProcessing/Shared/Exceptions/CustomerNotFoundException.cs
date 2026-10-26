namespace OrderProcessing.Shared.Exceptions;

public sealed class CustomerNotFoundException(string message) : Exception(message);
