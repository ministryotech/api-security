using System.Diagnostics.CodeAnalysis;

namespace Ministry.Web.ApiSecurity.Options
{
    /// <summary>
    /// Definition of configuration for secrets used during message signature validation.
    /// </summary>
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    public class MessageSignatureOptions
    {
        /// <summary>
        /// Gets or sets the secrets.
        /// </summary>
        public SignatureSecret[] Secrets { get; set; }
    }

    /// <summary>
    /// Definition of configuration for secrets used during message signature validation.
    /// </summary>
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    public class SignatureSecret
    {
        /// <summary>
        /// Gets or sets the key.
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// Gets or sets the secret.
        /// </summary>
        public string Secret { get; set; }
    }
}
