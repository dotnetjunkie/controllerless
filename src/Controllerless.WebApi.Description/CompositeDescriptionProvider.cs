using System;

namespace SolidServices.Controllerless.WebApi.Description
{
    public sealed class CompositeDescriptionProvider : IDescriptionProvider
    {
        private readonly IDescriptionProvider[] providers;

        public CompositeDescriptionProvider(params IDescriptionProvider[] providers)
        {
            Requires.IsNotNull(providers, nameof(providers));
            this.providers = providers;
        }

        public string GetDescription(Type type)
        {
            foreach (var provider in this.providers)
            {
                string description = provider.GetDescription(type);

                if (!string.IsNullOrEmpty(description)) return description;
            }

            return null;
        }
    }
}