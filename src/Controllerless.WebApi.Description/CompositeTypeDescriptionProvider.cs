using System;

namespace SolidServices.Controllerless.WebApi.Description
{
    /// <summary>
    /// Wraps a set of <see cref="ITypeDescriptionProvider"/> instances and the first found description of a requested type.
    /// </summary>
    public sealed class CompositeTypeDescriptionProvider : ITypeDescriptionProvider
    {
        private readonly ITypeDescriptionProvider[] providers;

        /// <summary>
        /// Constructs a new instance of <see cref="CompositeTypeDescriptionProvider"/> .
        /// </summary>
        /// <param name="providers"></param>
        public CompositeTypeDescriptionProvider(params ITypeDescriptionProvider[] providers)
        {
            Requires.IsNotNull(providers, nameof(providers));
            this.providers = providers;
        }

        /// <summary>Gets the type's description or null when there is no description for the given type.</summary>
        /// <param name="type">The type.</param>
        /// <returns>The description of the requested type or null.</returns>
        public string GetDescription(Type type)
        {
            foreach (var provider in this.providers)
            {
                string description = provider.GetDescription(type);

                if (!string.IsNullOrEmpty(description))
                {
                    return description;
                }
            }

            return null;
        }
    }
}