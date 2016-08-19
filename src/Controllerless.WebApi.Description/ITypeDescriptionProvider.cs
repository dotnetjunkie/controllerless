using System;

namespace SolidServices.Controllerless.WebApi.Description
{
    /// <summary>
    /// Provides descriptions for requested types.
    /// </summary>
    public interface ITypeDescriptionProvider
    {
        /// <summary>Gets the type's description or null when there is no description for the given type.</summary>
        /// <param name="type">The type.</param>
        /// <returns>The description of the requested type or null.</returns>
        string GetDescription(Type type);
    }
}