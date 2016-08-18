using System;
using System.Web.Http.Controllers;
using System.Web.Http.Description;

namespace SolidServices.Controllerless.WebApi.Description
{
    internal sealed class ControllerlessParameterDescriptor : HttpParameterDescriptor
    {
        private readonly string parameterName;
        private readonly Type parameterType;

        // HttpActionDescriptor actionDescriptor, 
        internal ControllerlessParameterDescriptor(string parameterName, Type parameterType)
        {
            this.parameterName = ToCamelCase(parameterName);
            this.parameterType = parameterType;
        }

        public override string ParameterName => this.parameterName;
        public override Type ParameterType => this.parameterType;

        private static string ToCamelCase(string name) => name.Substring(0, 1).ToLower() + name.Substring(1);
    }
}