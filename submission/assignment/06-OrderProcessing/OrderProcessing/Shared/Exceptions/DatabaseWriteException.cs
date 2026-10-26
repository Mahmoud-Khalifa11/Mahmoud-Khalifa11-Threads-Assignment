namespace OrderProcessing.Shared.Exceptions;

public sealed class DatabaseWriteException(string message) : Exception(message);
