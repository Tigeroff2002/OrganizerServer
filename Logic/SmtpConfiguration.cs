namespace Logic;

public sealed class SmtpConfiguration
{
    public string Host { get; init; } = string.Empty;

    public required string Port { get; init; }

    public required string Username { get; init; }

    public required string Password { get; init; }
}
