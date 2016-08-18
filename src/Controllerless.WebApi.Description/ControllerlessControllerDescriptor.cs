using System.Collections.ObjectModel;
using System.Web.Http.Controllers;

namespace SolidServices.Controllerless.WebApi.Description
{
    internal sealed class ControllerlessControllerDescriptor : HttpControllerDescriptor
    {
        // note you might provide some asp.net attributes here
        public override Collection<T> GetCustomAttributes<T>() => new Collection<T>();
    }
}