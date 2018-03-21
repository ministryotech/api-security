using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;
using Ministry.Web.ApiSecurity.Options;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

namespace Ministry.Web.ApiSecurity.Filters
{
    /// <summary>
    /// Ensures that the method receives a hashed API key header that matches a hash of the named key in configuration.
    /// </summary>
    /// <seealso cref="TypeFilterAttribute" />
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public class HashedApiKeyAttribute : TypeFilterAttribute
    {
        #region | Construction |

        /// <summary>
        /// Initializes a new instance of the <see cref="HashedApiKeyAttribute" /> class.
        /// </summary>
        /// <param name="header">The header.</param>
        /// <param name="key">The key.</param>
        /// <remarks>
        /// Will look for a key value pair matching the provided name in the configuration's 'Named' array.
        /// </remarks>
        public HashedApiKeyAttribute(string header, string key)
            : base(typeof(HashedApiKeyAttributeImpl))
        {
            Arguments = new object[] { header, key };
        }

        #endregion | Construction |

        /// <summary>
        /// Ensures that the method receives a hashed API key header that matches a hash of the named key in configuration.
        /// </summary>
        /// <seealso cref="ActionFilterAttribute" />
        private class HashedApiKeyAttributeImpl : IAsyncResourceFilter
        {
            private readonly string header;
            private readonly string key;
            private readonly HashedApiKey hashedKey;

            #region | Construction |

            /// <summary>
            /// Instantiates a new HashedApiKeyAttribute.
            /// </summary>
            /// <param name="options">The options.</param>
            /// <param name="header">The header.</param>
            /// <param name="key">The key.</param>
            public HashedApiKeyAttributeImpl(
                IOptions<ApiKeyOptions> options,
                string header,
                string key)
            {
                this.header = header;
                this.key = key;
                hashedKey = options.Value.Secrets.FirstOrDefault(op => op.Key == key);
            }

            #endregion

            /// <summary>
            /// Called asynchronously before the rest of the pipeline.
            /// </summary>
            /// <param name="context">The <see cref="T:Microsoft.AspNetCore.Mvc.Filters.ResourceExecutingContext" />.</param>
            /// <param name="next">The <see cref="T:Microsoft.AspNetCore.Mvc.Filters.ResourceExecutionDelegate" />. Invoked to execute the next resource filter or the remainder
            /// of the pipeline.</param>
            /// <returns>
            /// A <see cref="T:System.Threading.Tasks.Task" /> which will complete when the remainder of the pipeline completes.
            /// </returns>
            /// <exception cref="System.NotImplementedException"></exception>
            public async Task OnResourceExecutionAsync(ResourceExecutingContext context, ResourceExecutionDelegate next)
            {
                try
                {
                    var hashWithPrefix = context.HttpContext.Request.Headers[header];

                    if (hashedKey == null)
                        context.Result = new ObjectResult(
                            new { error = $"Access to this method requires a value for secret key '{key}' in configuration." })
                        { StatusCode = 500 };

                    else if (hashWithPrefix.Count == 0)
                        context.Result = new BadRequestObjectResult(
                            new { error = $"Access to this method requires a header of '{header}' to be provided." });

                    else if (!ApiKeyIsValid(hashedKey, hashWithPrefix))
                        context.Result = new BadRequestObjectResult(new { error = "Key Validation failed." });
                }
                catch (Exception ex)
                {
                    context.Result =
                        new ObjectResult(new { error = $"Key Validation failed: {ex.Message}" }) { StatusCode = 500 };
                }
                finally
                {
                    if (context.Result == null) await next();
                }
            }

            #region | Private Methods |

            /// <summary>
            /// Checks that the Api Key is valid.
            /// </summary>
            /// <param name="hashedKey">The hashed key.</param>
            /// <param name="hashWithPrefix">The hash with prefix.</param>
            /// <returns></returns>
            private static bool ApiKeyIsValid(HashedApiKey hashedKey, string hashWithPrefix)
                => new HashManager(hashWithPrefix.ThrowIfNull(nameof(hashWithPrefix)).Split('=')[0])
                    .MatchesHashed(hashedKey.Value, hashWithPrefix.Split('=')[1], hashedKey.Secret);

            #endregion | Private Methods |
        }
    }
}
