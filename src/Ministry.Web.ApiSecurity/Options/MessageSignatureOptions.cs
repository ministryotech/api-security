namespace Ministry.Web.ApiSecurity.Options;

/// <summary>
/// Definition of configuration for secrets used during message signature validation.
/// </summary>
[SuppressMessage("ReSharper", "AutoPropertyCanBeMadeGetOnly.Global", Justification = "Options Object")]
public class MessageSignatureOptions
{
    /// <summary>
    /// Gets or sets the secrets.
    /// </summary>
    public SignatureSecret[] Secrets { get; set; } = Array.Empty<SignatureSecret>();
}

/// <summary>
/// Definition of configuration for secrets used during message signature validation.
/// </summary>
[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global", Justification = "Options Object")]
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global", Justification = "Options Object")]
[SuppressMessage("ReSharper", "AutoPropertyCanBeMadeGetOnly.Global", Justification = "Options Object")]
public class SignatureSecret
{
    /// <summary>
    /// Gets or sets the key.
    /// </summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the secret.
    /// </summary>
    public string Secret { get; set; } = string.Empty;
}