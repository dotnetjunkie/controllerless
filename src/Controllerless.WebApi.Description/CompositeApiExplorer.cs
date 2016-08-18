using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Web.Http.Description;

namespace SolidServices.Controllerless.WebApi.Description
{
    public sealed class CompositeApiExplorer : IApiExplorer
    {
        private readonly Lazy<Collection<ApiDescription>> descriptions;

        public CompositeApiExplorer(params IApiExplorer[] apiExplorers)
        {
            Requires.IsNotNull(apiExplorers, nameof(apiExplorers));

            this.descriptions = new Lazy<Collection<ApiDescription>>(() => GetApiDescriptions(apiExplorers));
        }

        public Collection<ApiDescription> ApiDescriptions => this.descriptions.Value;

        private static Collection<ApiDescription> GetApiDescriptions(IApiExplorer[] apiExplorers) =>
            new Collection<ApiDescription>(apiExplorers.SelectMany(x => x.ApiDescriptions).ToList());
    }
}