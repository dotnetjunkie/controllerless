using System;

namespace SolidServices.Controllerless.WebApi.Description
{
    internal static class Requires
    {
        public static void IsNotNull(object instance, string paramName)
        {
            if (object.ReferenceEquals(instance, null))
            {
                throw new ArgumentNullException(paramName);
            }
        }
    }
}