using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Web.Http.Description;

namespace SolidServices.Controllerless.WebApi.Description
{
    /// <summary>
    /// Wraps multiple <see cref="IApiExplorer"/> instances and combines them into one collection of api descriptions.
    /// The results will be cached.
    /// </summary>
    public sealed class CompositeApiExplorer : IApiExplorer
    {
        private readonly Lazy<Collection<ApiDescription>> descriptions;

        /// <summary>Constructs a new instance of <see cref="CompositeApiExplorer"/>.</summary>
        /// <param name="apiExplorers"></param>
        public CompositeApiExplorer(params IApiExplorer[] apiExplorers)
        {
            Requires.IsNotNull(apiExplorers, nameof(apiExplorers));

            this.descriptions = new Lazy<Collection<ApiDescription>>(() =>
                new Collection<ApiDescription>(apiExplorers.SelectMany(x => x.ApiDescriptions).ToList()));
        }

        /// <summary>Gets the API descriptions.</summary>
        public Collection<ApiDescription> ApiDescriptions => this.descriptions.Value;
    }
}