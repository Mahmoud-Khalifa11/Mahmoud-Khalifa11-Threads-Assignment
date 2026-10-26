namespace OrderProcessing.Shared.Exceptions;

public sealed class SmtpUnavailableException(string message) : Exception(message);
