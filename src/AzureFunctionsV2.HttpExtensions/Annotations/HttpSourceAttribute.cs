using System;
using AzureFunctionsV2.HttpExtensions.Infrastructure;

namespace AzureFunctionsV2.HttpExtensions.Annotations
{
    /// <summary>
    /// The parent class for all custom HttpRequest source attributes.
    /// Those attributes signify from what data source the parameter (<see cref="HttpParam{T}"/>) value
    /// should be assigned from.
    /// </summary>
    public abstract class HttpSourceAttribute : Attribute
    {
    }
}
