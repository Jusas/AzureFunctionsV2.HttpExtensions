using System;
using AzureFunctionsV2.HttpExtensions.Annotations;

namespace AzureFunctionsV2.HttpExtensions.Infrastructure
{
    /// <summary>
    /// A placeholder class for temporarily storing <see cref="HttpSourceAttribute"/> for <see cref="HttpParam{T}"/> instances
    /// that get instantiated a bit later once the <see cref="HttpParamConverter{T}"/> gets invoked.
    /// </summary>
    public class AttributedParameter
    {
        public Attribute SourceAttribute { get; set; }

        public AttributedParameter(Attribute sourceAttribute)
        {
            SourceAttribute = sourceAttribute;
        }
    }
}
