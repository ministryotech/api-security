using System.Diagnostics.CodeAnalysis;

namespace Ministry.Web.ApiSecurity.Options
{
    /// <summary>
    /// Configuration options for static API Keys used in the application.
    /// </summary>
    public class ApiKeyOptions
    {
        /// <summary>
        /// Gets or sets the named keys.
        /// </summary>
        public NamedApiKey[] Named { get; set; }

        /// <summary>
        /// Gets or sets the hashed keys.
        /// </summary>
        public HashedApiKey[] Secrets { get; set; }
    }

    /// <summary>
    /// Definition of configuration for named API Keys.
    /// </summary>
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    public class NamedApiKey
    {
        /// <summary>
        /// Gets or sets the key name.
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// Gets or sets the value of the key.
        /// </summary>
        public string Value { get; set; }
    }

    /// <summary>
    /// Definition of configuration for hashed API Keys.
    /// </summary>
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    public class HashedApiKey : NamedApiKey
    {
        /// <summary>
        /// Gets or sets the secret used to hash with.
        /// </summary>
        public string Secret { get; set; }
    }
}