using System;

namespace SolidServices.Controllerless.WebApi.Description
{
    public interface IDescriptionProvider
    {
        string GetDescription(Type type);
    }
}