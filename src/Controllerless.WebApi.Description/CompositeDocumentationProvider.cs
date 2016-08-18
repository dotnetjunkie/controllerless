using System.Linq;
using System.Web.Http.Controllers;
using System.Web.Http.Description;

namespace SolidServices.Controllerless.WebApi.Description
{
    public class CompositeDocumentationProvider : IDocumentationProvider
    {
        private readonly IDocumentationProvider[] providers;

        public CompositeDocumentationProvider(params IDocumentationProvider[] providers)
        {
            Requires.IsNotNull(providers, nameof(providers));
            this.providers = providers;
        }

        public string GetDocumentation(HttpParameterDescriptor parameterDescriptor) =>
            this.providers.Select(p => p.GetDocumentation(parameterDescriptor)).FirstOrDefault(NotNull);

        public string GetDocumentation(HttpActionDescriptor actionDescriptor) =>
            this.providers.Select(p => p.GetDocumentation(actionDescriptor)).FirstOrDefault(NotNull);

        public string GetDocumentation(HttpControllerDescriptor controllerDescriptor) =>
            this.providers.Select(p => p.GetDocumentation(controllerDescriptor)).FirstOrDefault(NotNull);

        public string GetResponseDocumentation(HttpActionDescriptor actionDescriptor) =>
            this.providers.Select(p => p.GetResponseDocumentation(actionDescriptor)).FirstOrDefault(NotNull);

        private static bool NotNull(string value) => value != null;
    }
}