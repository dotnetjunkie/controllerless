using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Reflection;
using System.Web.Http.Controllers;
using System.Web.Http.Description;

namespace SolidServices.Controllerless.WebApi.Description
{
    public sealed class ControllerlessApiExplorer : IApiExplorer
    {
        private readonly IEnumerable<Type> messageTypes;
        private readonly Lazy<Collection<ApiDescription>> descriptions;
        private readonly Func<Type, string> actionNameSelector;
        private readonly Func<Type, string> descriptionSelector;
        private readonly Func<Type, Type> responseTypeSelector;

        public ControllerlessApiExplorer(
            IEnumerable<Type> messageTypes,
            string controllerName,
            Func<Type, string> actionNameSelector,
            Func<Type, string> descriptionSelector,
            Func<Type, Type> responseTypeSelector)
        {
            Requires.IsNotNull(messageTypes, nameof(messageTypes));
            Requires.IsNotNull(controllerName, nameof(controllerName));
            Requires.IsNotNull(actionNameSelector, nameof(actionNameSelector));
            Requires.IsNotNull(responseTypeSelector, nameof(responseTypeSelector));

            this.messageTypes = messageTypes;
            this.actionNameSelector = actionNameSelector;
            this.descriptionSelector = descriptionSelector;
            this.responseTypeSelector = responseTypeSelector;
            this.descriptions = new Lazy<Collection<ApiDescription>>(this.GetDescriptions);
            this.ControllerDescriptor = new ControllerlessControllerDescriptor { ControllerName = controllerName };
            this.RelativePathSelector = actionName => this.ApiPrefix + this.ControllerDescriptor.ControllerName + "/" + actionName;
        }

        public string ApiPrefix { get; set; } = "api/";
        public ApiParameterSource ParameterSource { get; set; } = ApiParameterSource.FromBody;
        public string ParameterName { get; set; } = "message";
        public HttpControllerDescriptor ControllerDescriptor { get; set; }
        public Func<Type, HttpMethod> HttpMethodSelector { get; set; } = type => HttpMethod.Post;
        public Func<string, string> RelativePathSelector { get; set; }
        public Collection<MediaTypeFormatter> SupportedRequestBodyFormatters { get; set; } = new Collection<MediaTypeFormatter>
        {
            new JsonMediaTypeFormatter(),
        };

        public Collection<ApiDescription> ApiDescriptions => descriptions.Value;

        private Collection<ApiDescription> GetDescriptions() =>
            new Collection<ApiDescription>(this.messageTypes.Select(CreateApiDescription).ToList());

        private static readonly PropertyInfo ResponseDescriptionProperty = typeof(ApiDescription).GetProperty("ResponseDescription");

        public ApiDescription CreateApiDescription(Type messageType)
        {
            string actionName = this.actionNameSelector(messageType);
            var responseType = this.responseTypeSelector(messageType);

            var desc = new ApiDescription
            {
                HttpMethod = this.HttpMethodSelector(messageType),
                RelativePath = this.RelativePathSelector(actionName),
                Documentation = this.descriptionSelector(messageType),
            };

            desc.ActionDescriptor = new ControllerlessActionDescriptor(desc, messageType, actionName,
                responseType, BuildParameters(messageType))
            {
                ControllerDescriptor = this.ControllerDescriptor
            };

            ResponseDescriptionProperty.SetValue(desc, new ResponseDescription
            {
                DeclaredType = responseType,
                ResponseType = responseType,
                Documentation = this.descriptionSelector(responseType),
            });

            desc.SupportedRequestBodyFormatters.AddRange(this.SupportedRequestBodyFormatters);

            desc.ParameterDescriptions.AddRange(desc.ActionDescriptor.GetParameters().Select(ToApiParameterDescription));

            return desc;
        }

        private IEnumerable<ControllerlessParameterDescriptor> BuildParameters(Type messageType)
        {
            IEnumerable<ControllerlessParameterDescriptor> parameters;

            if (this.ParameterSource == ApiParameterSource.FromUri)
            {
                parameters = // desc.ActionDescriptor.GetParameters().AddRange(
                    from prop in messageType.GetProperties(BindingFlags.Instance | BindingFlags.Public)
                    select new ControllerlessParameterDescriptor(prop.Name, prop.PropertyType);
            }
            else
            {
                parameters = new[]
                {
                    new ControllerlessParameterDescriptor(this.ParameterName, messageType)
                };
            }

            return parameters;
        }

        private ApiParameterDescription ToApiParameterDescription(HttpParameterDescriptor descriptor)
        {
            return new ApiParameterDescription
            {
                Documentation = this.descriptionSelector(descriptor.ParameterType),
                Name = descriptor.ParameterName,
                ParameterDescriptor = descriptor,
                Source = this.ParameterSource
            };
        }
    }
}