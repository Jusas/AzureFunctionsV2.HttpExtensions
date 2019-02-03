using System.Threading.Tasks;
using AzureFunctionsV2.HttpExtensions.Annotations;
using AzureFunctionsV2.HttpExtensions.Infrastructure;
using Microsoft.Azure.WebJobs.Host.Bindings;
using Microsoft.Azure.WebJobs.Host.Config;

namespace AzureFunctionsV2.HttpExtensions.Extensions
{
    /// <summary>
    /// The config provider that registers the different <see cref="HttpSourceAttribute"/> bindings.
    /// </summary>
    public class HttpAttributeExtensionsConfigProvider : IExtensionConfigProvider
    {
        public HttpAttributeExtensionsConfigProvider()
        {
            
        }

        public void Initialize(ExtensionConfigContext context)
        {
            var ruleFromBody = context.AddBindingRule<HttpBodyAttribute>();
            var ruleFromForm = context.AddBindingRule<HttpFormAttribute>();
            var ruleFromHeader = context.AddBindingRule<HttpHeaderAttribute>();
            var ruleFromQuery = context.AddBindingRule<HttpQueryAttribute>();
            var ruleFromAuth = context.AddBindingRule<HttpJwtAttribute>();

            ruleFromBody.BindToInput<AttributedParameter>(BodyPlaceholder);
            ruleFromForm.BindToInput<AttributedParameter>(FormPlaceholder);
            ruleFromHeader.BindToInput<AttributedParameter>(HeaderPlaceholder);
            ruleFromQuery.BindToInput<AttributedParameter>(QueryPlaceholder);
            ruleFromAuth.BindToInput<AttributedParameter>(AuthPlaceholder);

            context.AddOpenConverter<AttributedParameter, HttpParam<OpenType>>(typeof(HttpParamConverter<>));
            context.AddConverter<AttributedParameter, HttpUser>(new HttpUserConverter());
        }

        private async Task<AttributedParameter> BodyPlaceholder(HttpBodyAttribute attribute, ValueBindingContext ctx) => new AttributedParameter(attribute);
        private async Task<AttributedParameter> FormPlaceholder(HttpFormAttribute attribute, ValueBindingContext ctx) => new AttributedParameter(attribute);
        private async Task<AttributedParameter> HeaderPlaceholder(HttpHeaderAttribute attribute, ValueBindingContext ctx) => new AttributedParameter(attribute);
        private async Task<AttributedParameter> QueryPlaceholder(HttpQueryAttribute attribute, ValueBindingContext ctx) => new AttributedParameter(attribute);
        private async Task<AttributedParameter> AuthPlaceholder(HttpJwtAttribute attribute, ValueBindingContext ctx) => new AttributedParameter(attribute);
    }
}
