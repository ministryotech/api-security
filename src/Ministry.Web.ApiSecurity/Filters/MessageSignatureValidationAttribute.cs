using Microsoft.AspNetCore.Http.Internal;
using Ministry.Web.ApiSecurity.Options;

namespace Ministry.Web.ApiSecurity.Filters;

/// <summary>
/// Ensures that the method receives a hash of the message body using a provided secret.
/// </summary>
/// <seealso cref="TypeFilterAttribute" />
[SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "Library")]
[SuppressMessage("ReSharper", "UnusedType.Global", Justification = "Library")]
public class MessageSignatureValidationAttribute : TypeFilterAttribute
{
    #region | Constructor |

    /// <summary>
    /// Initializes a new instance of the <see cref="MessageSignatureValidationAttribute" /> class.
    /// </summary>
    /// <param name="header">The header.</param>
    /// <param name="secretKey">The secret key.</param>
    public MessageSignatureValidationAttribute(string header, string secretKey)
        : base(typeof(MessageSignatureValidationAttributeImpl))
    {
        Arguments = new object[] { header, secretKey };
    }

    #endregion

    /// <summary>
    /// Ensures that the method receives an API key header that matches the given configured static key for the application.
    /// </summary>
    /// <seealso cref="ActionFilterAttribute" />
    private class MessageSignatureValidationAttributeImpl : IAsyncResourceFilter
    {
        private readonly string header;
        private readonly string secretKey;
        private readonly string secret;

        #region | Construction |

        /// <summary>
        /// Instantiates a new <see cref="MessageSignatureValidationAttribute"/>.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <param name="header">The header.</param>
        /// <param name="secretKey">The secret key.</param>
        public MessageSignatureValidationAttributeImpl(IOptions<MessageSignatureOptions> options,
            string header, string secretKey)
        {
            this.header = header;
            this.secretKey = secretKey;
            var pair = options.Value.Secrets.FirstOrDefault(op => op.Key == secretKey);
            secret = pair != null ? pair.Secret : string.Empty;
        }

        #endregion

        /// <summary>
        /// Called asynchronously before the rest of the pipeline.
        /// </summary>
        /// <param name="context">The <see cref="Microsoft.AspNetCore.Mvc.Filters.ResourceExecutingContext" />.</param>
        /// <param name="next">The <see cref="Microsoft.AspNetCore.Mvc.Filters.ResourceExecutionDelegate" />. Invoked to execute the next resource filter or the remainder
        /// of the pipeline.</param>
        /// <returns>
        /// A <see cref="System.Threading.Tasks.Task" /> which will complete when the remainder of the pipeline completes.
        /// </returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task OnResourceExecutionAsync(ResourceExecutingContext context, ResourceExecutionDelegate next)
        {
            try
            {
                var signatureWithPrefix = context.HttpContext.Request.Headers[header];

                if (string.IsNullOrEmpty(secret))
                    context.Result = new ObjectResult(
                            new { error = $"Access to this method requires a value for secret key '{secretKey}' in configuration." })
                        { StatusCode = 500 };

                else if (signatureWithPrefix.Count == 0)
                    context.Result = new BadRequestObjectResult(
                        new { error = $"Access to this method requires a header of '{header}' to be provided." });

                else if (!MessageSignatureIsValid(await GetMessageBodySafelyAsync(context), signatureWithPrefix, secret))
                    context.Result = new BadRequestObjectResult(new { error = "Signature Validation failed." });
            }
            catch (Exception ex)
            {
                context.Result =
                    new ObjectResult(new { error = $"Signature Validation failed: {ex.Message}" }) { StatusCode = 500 };
            }
            finally
            {
                if (context.Result == null) await next();
            }
        }

        #region | Private Methods |

        /// <summary>
        /// Gets the message body safely.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        private static async Task<string> GetMessageBodySafelyAsync(ActionContext context)
        {
            var contextRequest = context.HttpContext.Request;
            contextRequest.EnableRewind();

            string messageBody;

            using (var reader = new StreamReader(contextRequest.Body, Encoding.UTF8, true, 1024, true))
                messageBody = await reader.ReadToEndAsync();

            contextRequest.Body.Position = 0;

            if (string.IsNullOrEmpty(messageBody))
                throw new InvalidDataException("The message body is empty.");

            return messageBody;
        }

        /// <summary>
        /// Checks that the message signature is valid.
        /// </summary>
        /// <param name="payload">The payload.</param>
        /// <param name="signatureWithPrefix">The signature with prefix.</param>
        /// <param name="secret">The secret.</param>
        /// <returns>A flag</returns>
        private static bool MessageSignatureIsValid(string payload, string signatureWithPrefix, string secret)
            => new HashManager(signatureWithPrefix.ThrowIfNull(nameof(signatureWithPrefix)).Split('=')[0])
                .MatchesHashed(payload, signatureWithPrefix.Split('=')[1], secret);

        #endregion
    }
}