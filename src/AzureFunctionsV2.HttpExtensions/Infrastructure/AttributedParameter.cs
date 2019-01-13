using System;

namespace AzureFunctionsV2.HttpExtensions.Infrastructure
{
    public class AttributedParameter
    {
        public Attribute SourceAttribute { get; set; }

        public AttributedParameter(Attribute sourceAttribute)
        {
            SourceAttribute = sourceAttribute;
        }
    }
}
