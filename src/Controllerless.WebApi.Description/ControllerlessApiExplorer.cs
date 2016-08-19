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
    /// <summary>
    /// Api Explorer for creating a documentation for a set of messages under one specific part of your application.
    /// </summary>
    public sealed class ControllerlessApiExplorer : IApiExplorer
    {
        private readonly IEnumerable<Type> messageTypes;
        private readonly Lazy<Collection<ApiDescription>> descriptions;
        private readonly Func<Type, string> typeDescriptionSelector;
        private readonly Func<Type, Type> responseTypeSelector;

        /// <summary>Constructs a new <see cref="ControllerlessApiExplorer"/>.</summary>
        /// <param name="controllerName">The name of the controller. Typically 'controllers' or 'queries'.</param>
        /// <param name="messageTypes">The list of messages that this explorer documents.</param>
        /// <param name="typeDescriptionSelector">Retrieves the description for a given type.</param>
        /// <param name="responseTypeSelector"></param>
        public ControllerlessApiExplorer(
            string controllerName,
            IEnumerable<Type> messageTypes,
            Func<Type, string> typeDescriptionSelector,
            Func<Type, Type> responseTypeSelector)
        {
            Requires.IsNotNull(controllerName, nameof(controllerName));
            Requires.IsNotNull(messageTypes, nameof(messageTypes));
            Requires.IsNotNull(typeDescriptionSelector, nameof(typeDescriptionSelector));
            Requires.IsNotNull(responseTypeSelector, nameof(responseTypeSelector));
            
            this.messageTypes = messageTypes.ToArray();
            this.typeDescriptionSelector = typeDescriptionSelector;
            this.responseTypeSelector = responseTypeSelector;
            this.descriptions = new Lazy<Collection<ApiDescription>>(this.GetDescriptions);
            this.ControllerDescriptor = new ControllerlessControllerDescriptor { ControllerName = controllerName };
            this.RelativePathSelector = actionName => this.ApiPrefix + this.ControllerDescriptor.ControllerName + "/" + actionName;
        }

        /// <summary>Gets or sets the prefix for the API. Default value is "api/".</summary>
        public string ApiPrefix { get; set; } = "api/";

        /// <summary>Gets or sets the parameter source selector. The default always returns FromBody, which means </summary>
        public Func<Type, ApiParameterSource> ParameterSourceSelector { get; set; } = type => ApiParameterSource.FromBody;

        /// <summary>
        /// Gets or sets the parameter name that is used when the parameter source is set to FromBody for a 
        /// given message type. The default value is 'message'.</summary>
        public string ParameterName { get; set; } = "message";

        /// <summary>
        /// Gets or sets the <see cref="HttpControllerDescriptor"/> that is added to the list of <see cref="ApiDescription"/>
        /// elements that is produced by this Api Explorer.
        /// </summary>
        public HttpControllerDescriptor ControllerDescriptor { get; set; }

        /// <summary>Gets or sets the action name selector. The default returns the the short name of the supplied message type.</summary>
        public Func<Type, string> ActionNameSelector { get; set; } = type => type.Name;

        /// <summary>Gets or sets the http method selector. The default return HttpMethod.Post for every supplied message type.</summary>
        public Func<Type, HttpMethod> HttpMethodSelector { get; set; } = type => HttpMethod.Post;

        /// <summary>
        /// Gets or sets the relative path selector. Builds a relative path based on the action name as returned from
        /// the <see cref="ActionNameSelector"/>. The default returns the action name prefixed by the 
        /// <see cref="ApiPrefix"/> and supplied controller name.</summary>
        public Func<string, string> RelativePathSelector { get; set; }

        /// <summary>
        /// Gets or sets the collection of supported <see cref="MediaTypeFormatter"/>s for decoding request bodies.
        /// The default only contains the <see cref="JsonMediaTypeFormatter"/>.
        /// </summary>
        public Collection<MediaTypeFormatter> SupportedRequestBodyFormatters { get; set; } = new Collection<MediaTypeFormatter>
        {
            new JsonMediaTypeFormatter(),
        };

        /// <summary>
        /// Gets the list of <see cref="ApiDescription"/> instances for the given set of message types.
        /// The collection is cached.
        /// </summary>
        public Collection<ApiDescription> ApiDescriptions => descriptions.Value;

        private Collection<ApiDescription> GetDescriptions() =>
            new Collection<ApiDescription>(this.messageTypes.Select(CreateApiDescription).ToList());

        private static readonly PropertyInfo ResponseDescriptionProperty = typeof(ApiDescription).GetProperty("ResponseDescription");

        private ApiDescription CreateApiDescription(Type messageType)
        {
            string actionName = this.ActionNameSelector(messageType);
            var responseType = this.responseTypeSelector(messageType);

            var desc = new ApiDescription
            {
                HttpMethod = this.HttpMethodSelector(messageType),
                RelativePath = this.RelativePathSelector(actionName),
                Documentation = this.typeDescriptionSelector(messageType),
            };

            var parameterSource = this.ParameterSourceSelector(messageType);

            if (desc.HttpMethod == HttpMethod.Get && parameterSource == ApiParameterSource.FromBody)
            {
                throw new InvalidOperationException("For the given message type " + messageType.FullName +
                    ", the provided HttpMethodSelector returned GET, while the ParameterSourceProvider " +
                    "returned FromBody. This is an invalid combination, because GET requests don't have " +
                    "a body.");
            }

            desc.ActionDescriptor = new ControllerlessActionDescriptor(desc, messageType, actionName,
                responseType, BuildParameters(desc, messageType, parameterSource))
            {
                ControllerDescriptor = this.ControllerDescriptor
            };

            ResponseDescriptionProperty.SetValue(desc, new ResponseDescription
            {
                DeclaredType = responseType,
                ResponseType = responseType,
                Documentation = this.typeDescriptionSelector(responseType),
            });

            desc.SupportedRequestBodyFormatters.AddRange(this.SupportedRequestBodyFormatters);

            desc.ParameterDescriptions.AddRange(
                from parameter in desc.ActionDescriptor.GetParameters()
                select ToApiParameterDescription(parameter, parameterSource));

            return desc;
        }

        private IEnumerable<ControllerlessParameterDescriptor> BuildParameters(ApiDescription description, 
            Type messageType, ApiParameterSource parameterSource)
        {
            if (parameterSource == ApiParameterSource.FromUri)
            {
                return
                    from prop in messageType.GetProperties(BindingFlags.Instance | BindingFlags.Public)
                    select new ControllerlessParameterDescriptor(prop.Name, prop.PropertyType);
            }
            else
            {
                return new[] { new ControllerlessParameterDescriptor(this.ParameterName, messageType) };
            }
        }

        private ApiParameterDescription ToApiParameterDescription(HttpParameterDescriptor descriptor,
            ApiParameterSource parameterSource)
        {
            return new ApiParameterDescription
            {
                Documentation = this.typeDescriptionSelector(descriptor.ParameterType),
                Name = descriptor.ParameterName,
                ParameterDescriptor = descriptor,
                Source = parameterSource
            };
        }
    }
}