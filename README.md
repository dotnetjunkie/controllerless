# Controllerless
### SOLID designed applications don't need controllers. Remove the cruft; become controllerless.


[![NuGet](https://img.shields.io/nuget/v/SolidServices.Controllerless.WebApi.Description.svg)](https://www.nuget.org/packages/SolidServices.Controllerless.WebApi.Description/)


If you're writing SOLID message-based applications, you came to the right place. If not, please [go here](https://github.com/dotnetjunkie/solidservices) to understand what such application design can bring you and your team.

**Controllerless** is a sample project that shows how to build applications based on message-driven architectures that don't require the cruft of defining the service or presentation-layer classes like Web API controllers or WCF services. The projects and samples in this repository are expected to grow over time. For now, don't expect this code to be hardened production-ready code.

Currently the only supported project is the Controllerless Web API documentation generation project:

## Controllerless Web API documentation generation

*Controllerless Web API documentation generation* contains an alternative Web API `ApiExplorer` implementation that makes it possible to build documentation based on the messages you defined in your application, instead of controllers and actions, since we promote a happy world without the cruft that controllers bring.

On top of *Controllerless Web API documentation generation*, Swagger documentation can be easily generated using [Swashbuckle](https://github.com/domaindrivendev/Swashbuckle).

The main type in the *Controllerless Web API documentation generation* is the `ControllerlessApiExplorer`. The `ControllerlessApiExplorer` generates a list of `ApiDescriptions` that can be used to generate documentation (for instance using Swagger).

### Short example
The following example shows the creation of a `ControllerlessApiExplorer` with a default configuration:

``` c#
// Create a ControllerlessApiExplorer
var messageApiExplorer = new ControllerlessApiExplorer(
    // List of 'action' messages that this explorer uses.
    messageTypes: new[] { typeof(GetOrderById), typeof(ShipOrder) },
    // Delegate that produces the return type for a given message type.
    responseTypeSelector: messageType => GetResponseType(messageType));

// Replace the original explorer with our new one
config.Services.Replace(typeof(IApiExplorer), messageApiExplorer);
```

This `ControllerlessApiExplorer` returns documentation for the two supplied request messages and produces descriptions for the following relative paths:

 - [POST] api/messages/GetOrderById
 - [POST] api/messages/ShipOrder

### Full example

`ControllerlessApiExplorer` is fully configurable and can be adapter to your needs. The following example shows an `ControllerlessApiExplorer` that is used for generation of the command API of an application, where commands don't return anything (although the infrastructure should obviously still communicate HTTP error codes back to the client):

``` c#

var commandApiExplorer = new ControllerlessApiExplorer(
    messageTypes: new[] { typeof(CreateOrder), typeof(ShipOrder) },
    responseTypeSelector: messageType => typeof(void))
{
    // Prefix of your API. Defaults to "api/"
    ApiPrefix = "api/",
    // Name of the controller. Typically "commands" or "queries".
    ControllerName = "commands",
    // Parameter name for the action's parameter. The default value is 'message'.
    ParameterName = "command",
    // Delegate that returns the name of the action. The default returns 'messageType.Name'.
    ActionNameSelector = messageType => messageType.Name,
    // Delete that returns the HttpMethod for a messageType. The default returns HttpMethod.Post.
    HttpMethodSelector = messageType => HttpMethod.Post,
    // Delegate that defines the parameter source for a given message type. Typically
    // FromBody for commands and FromUri or FromBody for queries. Default is FromBody.
    ParameterSourceSelector = messageType => ApiParameterSource.FromBody,
    // Builds the relative path based on the action name. 
    // By default it returns ApiPrefix + controllerName + "/" + actionName.
    RelativePathSelector = actionName => "api/commands/" + actionName,
    // The list of supported request body formatters. By default it contains only the json formatter.
    SupportedRequestBodyFormatters = new Collection<MediaTypeFormatter> { new JsonMediaTypeFormatter() },
};

// Combine the original explorer, plus your custom explorers in one composite explorer.
config.Services.Replace(typeof(IApiExplorer),
	new CompositeApiExplorer(
		config.Services.GetApiExplorer(),
		commandApiExplorer,
		queryApiExplorer));
```

#### Integrate with Swashbuckle

The previous example works as is. After you plugin [Swashbuckle](https://github.com/domaindrivendev/Swashbuckle) to generate Swagger documentation you're almost done. To allow Swashbuckle to generate descriptions for parameters and message and return types, you need to point Swashbuckle to the compiler generated .XML documentation file(s) of your message types:

``` c#
// Part of your Swashbuckle config
string xmlCommentsPath = HostingEnvironment.MapPath("~/App_Data/Contract.xml");

swaggerDocsConfig.IncludeXmlComments(xmlCommentsPath);
```

This allows you to use XML documentation on your message such as follows:

``` c#
/// <summary>Commands an order to be shipped.</summary>
public class ShipOrderCommand
{
    /// <summary>The id of the order.</summary>
    public Guid OrderId { get; set; }
}
```

On top of that you need to implement a custom `IOperationFilter` to allow your actions to have a description:

``` c#

sealed class ControllerlessActionOperationFilter : IOperationFilter
{
    private readonly ITypeDescriptionProvider provider;

    public ControllerlessActionOperationFilter(string xmlCommentsPath)
    {
        this.provider = new XmlDocumentationTypeDescriptionProvider(xmlCommentsPath);
    }

    public void Apply(Operation operation, SchemaRegistry sr, ApiDescription desc)
    {
        var descriptor = desc.ActionDescriptor as ControllerlessActionDescriptor;
        operation.summary = descriptor != null
            ? this.provider.GetDescription(descriptor.MessageType)
            : operation.summary;
    }
}
```

This operation filter makes use of **Controllerless** to retrieve the description of a message type from the created `XmlDocumentationTypeDescriptionProvider`. `XmlDocumentationTypeDescriptionProvider` is a type provided by **Controllerless** and allows parsing an XML comment file. You can configure Swashbuckle to use this class as follows:

``` c#
var filter = new ControllerlessActionOperationFilter(xmlCommentsPath);
c.OperationFilter(() => filter);
```

The above results in the following Swagger documentation:

![Alt text](swaggerexample.png?raw=true "Swagger example")
