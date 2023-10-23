namespace Logic;

public sealed class SmtpConfiguration
{
    public required string Host { get; init; }

    public required int Port { get; init; }

    public required string Topic { get; init; }

    public required string FromAdress { get; init; }
}
