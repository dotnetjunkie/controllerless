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
    public class ControllerlessActionDescriptor : HttpActionDescriptor
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

        public ApiDescription ApiDescription { get; }
        public Type MessageType { get; }
        public override string ActionName { get; }
        public override Type ReturnType { get; }

        public override Collection<HttpParameterDescriptor> GetParameters() => this.parameters;

        public override Collection<T> GetCustomAttributes<T>(bool inherit) =>
            new Collection<T>(this.MessageType.GetCustomAttributes(typeof(T), inherit).OfType<T>().ToArray());

        public override Task<object> ExecuteAsync(HttpControllerContext controllerContext,
            IDictionary<string, object> arguments, CancellationToken cancellationToken)
        {
            // Never called.
            throw new NotImplementedException();
        }
    }
}