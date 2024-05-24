namespace Logic;

public class AdsPushFirebaseSettingsConfiguration
{
    /// <summary>
    /// type filed in service_account.json
    /// </summary>
    public string Type { get; set; }

    /// <summary>
    /// project_id filed in service_account.json
    /// </summary>
    public string ProjectId { get; set; }

    /// <summary>
    /// private_key_id filed in service_account.json
    /// </summary>
    public string PrivateKeyId { get; set; }

    /// <summary>
    /// private_key filed in service_account.json
    /// </summary>
    public string PrivateKey { get; set; }

    /// <summary>
    /// client_email filed in service_account.json
    /// </summary>
    public string ClientEmail { get; set; }

    /// <summary>
    /// client_id filed in service_account.json
    /// </summary>
    public string ClientId { get; set; }

    /// <summary>
    /// auth_uri filed in service_account.json
    /// </summary>
    public string AuthUri { get; set; }

    /// <summary>
    /// token_uri filed in service_account.json
    /// </summary>
    public string TokenUri { get; set; }

    /// <summary>
    /// auth_provider_x509_cert_url filed in service_account.json
    /// </summary>
    public string AuthProviderX509CertUrl { get; set; }

    /// <summary>
    /// client_x509_cert_url filed in service_account.json
    /// </summary>
    public string ClientX509CertUrl { get; set; }
}
