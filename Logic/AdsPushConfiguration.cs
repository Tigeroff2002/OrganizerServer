namespace Logic;

public sealed class AdsPushConfiguration
{
    public required string Type { get; init; }

    public required string ProjectId { get; init; }

    public required string PrivateKey { get; init; }

    public required string PrivateKeyId { get; init; }

    public required string ClientId { get; init; }

    public required string ClientEmail { get; init; }

    public required string AuthUri { get; init; }

    public required string AuthProviderX509CertUrl { get; init; }

    public required string TokenUri { get; init; }

    public required string ClientX509CertUrl { get; init; }
}
