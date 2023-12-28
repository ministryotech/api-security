using Ministry.Web.ApiSecurity.Options;

namespace Ministry.Web.ApiSecurity.Filters;

/// <summary>
/// Ensures that the method receives an API key header that matches the given configured static key for the application.
/// </summary>
/// <seealso cref="TypeFilterAttribute" />
[SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "Library")]
[SuppressMessage("ReSharper", "UnusedType.Global", Justification = "Library")]
public class NamedApiKeyAttribute : TypeFilterAttribute
{
    #region | Construction |

    /// <summary>
    /// Initializes a new instance of the <see cref="NamedApiKeyAttribute" /> class.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <remarks>
    /// Will look for a key value pair matching the provided name in the configuration's 'Named' array.
    /// </remarks>
    public NamedApiKeyAttribute(string key)
        : base(typeof(NamedStaticApiKeyAttributeImpl))
    {
        Arguments = new object[] { key };
    }

    #endregion

    /// <summary>
    /// Ensures that the method receives an API key header that matches the given configured static key for the application.
    /// </summary>
    /// <seealso cref="ActionFilterAttribute" />
    private class NamedStaticApiKeyAttributeImpl : IResourceFilter
    {
        private readonly ApiKeyOptions? options;
        private readonly string? key;

        #region | Construction |

        /// <summary>
        /// Instantiates a new <see cref="NamedApiKeyAttribute"/>.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <param name="key">The key.</param>
        public NamedStaticApiKeyAttributeImpl(
            IOptions<ApiKeyOptions> options,
            string? key)
        {
            this.options = options.Value;
            this.key = key;
        }

        #endregion

        /// <summary>
        /// Executes the resource filter. Called before execution of the remainder of the pipeline.
        /// </summary>
        /// <param name="context">The <see cref="Microsoft.AspNetCore.Mvc.Filters.ResourceExecutingContext" />.</param>
        public void OnResourceExecuting(ResourceExecutingContext context)
        {
            var configuredKey = options?.Named.FirstOrDefault(k => string.Equals(k.Key, key, StringComparison.InvariantCultureIgnoreCase));

            if (string.IsNullOrEmpty(key))
                context.Result = new ObjectResult(
                        new { error = "The endpoint is missing a key to look for configured API keys." })
                    { StatusCode = 500 }; 

            else if (options != null && !options.Named.Any())
                context.Result = new ObjectResult(
                        new { error = "Access to this resource requires a configured API Key. The configuration on the server is missing." })
                    { StatusCode = 500 };

            else if (configuredKey == null)
                context.Result = new ObjectResult(
                        new { error = $"Access to this method requires a value for secret key '{key}' in configuration." })
                    { StatusCode = 500 };

            else if (context.HttpContext.Request.Headers["X-ApiKey"].Count == 0)
                context.Result = new BadRequestObjectResult(new { error = "Access to this resource requires a key to be provided." });

            else if (context.HttpContext.Request.Headers["X-ApiKey"][0] != configuredKey.Value)
                context.Result = new BadRequestObjectResult(new { error = "The key provided for access to this resource is invalid." });
        }

        /// <summary>
        /// Executes the resource filter. Called after execution of the remainder of the pipeline.
        /// </summary>
        /// <param name="context">The <see cref="Microsoft.AspNetCore.Mvc.Filters.ResourceExecutedContext" />.</param>
        public void OnResourceExecuted(ResourceExecutedContext context)
        { }
    }
}