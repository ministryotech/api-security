using System.Security.Cryptography;

namespace Ministry.Web.ApiSecurity;

/// <summary>
/// The Hash manager.
/// </summary>
[SuppressMessage("ReSharper", "MemberCanBePrivate.Global", Justification = "Library")]
public class HashManager
{
    private readonly Func<byte[], HMAC> hashProviderFunc;

    #region | Construction |

    /// <summary>
    /// Initializes a new instance of the <see cref="HashManager"/> class.
    /// </summary>
    /// <param name="providerType">Type of the provider.</param>
    public HashManager(string providerType)
    {
        HashType = CleanType(providerType);
        hashProviderFunc = GetHmacProviderFunction(HashType);
    }

    #endregion

    /// <summary>
    /// Gets the type of the hash.
    /// </summary>
    public string HashType { get; }

    /// <summary>
    /// Gets the hash provider.
    /// </summary>
    /// <param name="secret">The secret.</param>
    /// <returns>A hash provider.</returns>
    public HMAC GetHashProvider(string secret) 
        => hashProviderFunc(Encoding.UTF8.GetBytes(secret));

    /// <summary>
    /// Hashes the specified clear value using the provided secret.
    /// </summary>
    /// <param name="clear">The clear value.</param>
    /// <param name="secret">The secret.</param>
    /// <returns>A hash string.</returns>
    public string Hash(string clear, string secret)
    {
        using var provider = GetHashProvider(secret);
        var hash = provider.ComputeHash(Encoding.UTF8.GetBytes(clear.Replace("\n", "")));
        var hashString = ToHexString(hash);

        return hashString;
    }

    /// <summary>
    /// Hashed value matches the clear value (when hashed with the provided secret).
    /// </summary>
    /// <param name="clear">The clear.</param>
    /// <param name="hashed">The hashed.</param>
    /// <param name="secret">The secret.</param>
    /// <returns>A flag.</returns>
    public bool MatchesHashed(string clear, string hashed, string secret)
        => Hash(clear, secret).Equals(hashed);

    #region | Private Methods |

    /// <summary>
    /// Cleans the type.
    /// </summary>
    /// <param name="providerType">Type of the provider.</param>
    /// <returns>An upper cased string with '=' removed.</returns>
    private static string CleanType(string providerType)
        => providerType.ToUpperInvariant().Replace("=", "");

    /// <summary>
    /// Gets the HMAC provider function.
    /// </summary>
    /// <param name="providerType">Type of the provider.</param>
    /// <returns>A function.</returns>
    /// <exception cref="ArgumentOutOfRangeException">providerType</exception>
    private static Func<byte[], HMAC> GetHmacProviderFunction(string providerType)
    {
        return providerType switch
        {
            #pragma warning disable CA5350
            "SHA1" => secretBytes => new HMACSHA1(secretBytes),
            #pragma warning restore CA5350
            "SHA256" => secretBytes => new HMACSHA256(secretBytes),
            _ => throw new ArgumentOutOfRangeException(nameof(providerType),
                $"The specified hash provider '{providerType}' is not supported.")
        };
    }

    /// <summary>
    /// To the hexadecimal string.
    /// </summary>
    /// <param name="bytes">The bytes.</param>
    /// <returns>A Hex String.</returns>
    private static string ToHexString(IReadOnlyCollection<byte> bytes)
    {
        var builder = new StringBuilder(bytes.Count * 2);

        foreach (var b in bytes)
            builder.AppendFormat("{0:x2}", b);

        return builder.ToString();
    }

    #endregion
}