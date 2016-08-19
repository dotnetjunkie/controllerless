using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using System.Web.Http.Description;

namespace SolidServices.Controllerless.WebApi.Description
{
    /// <summary>
    /// An <see cref="HttpActionDescriptor"/> used by Controllerless for constructing APIs for Actions.
    /// </summary>
    public sealed class ControllerlessActionDescriptor : HttpActionDescriptor
    {
        private readonly Collection<HttpParameterDescriptor> parameters;

        internal ControllerlessActionDescriptor(ApiDescription description, Type messageType, string actionName,
            Type returnType, IEnumerable<HttpParameterDescriptor> parameters)
        {
            this.ApiDescription = description;
            this.MessageType = messageType;
            this.ActionName = actionName;
            this.ReturnType = returnType;
            this.parameters = new Collection<HttpParameterDescriptor>(parameters.ToList());
            this.parameters.ToList().ForEach(p => p.ActionDescriptor = this);
        }

        /// <summary>Gets the parent <see cref="ApiDescription"/>.</summary>
        public ApiDescription ApiDescription { get; }

        /// <summary>The type of the message that this action processes.</summary>
        public Type MessageType { get; }

        /// <summary>The name of the action.</summary>
        public override string ActionName { get; }

        /// <summary>Gets the return type of the descriptor..</summary>
        public override Type ReturnType { get; }

        /// <summary>Retrieves the parameters for the action descriptor.</summary>
        /// <returns>The parameters for the action descriptor.</returns>
        public override Collection<HttpParameterDescriptor> GetParameters() => this.parameters;

        /// <summary>
        /// Gets the custom attributes for the <see cref="MessageType"/>.
        /// </summary>
        /// <typeparam name="T">The type of attribute to search for.</typeparam>
        /// <param name="inherit">true to search this action's inheritance chain to find the attributes; otherwise, false.</param>
        /// <returns>The collection of custom attributes applied to this action.</returns>
        public override Collection<T> GetCustomAttributes<T>(bool inherit) =>
            new Collection<T>(this.MessageType.GetCustomAttributes(typeof(T), inherit).OfType<T>().ToArray());

        /// <summary>Throws an exception.</summary>
        /// <param name="controllerContext"></param>
        /// <param name="arguments"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public override Task<object> ExecuteAsync(HttpControllerContext controllerContext,
            IDictionary<string, object> arguments, CancellationToken cancellationToken)
        {
            // Never called.
            throw new NotImplementedException();
        }
    }
}