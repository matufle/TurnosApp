using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace TurnosApp.Presentation.WebAPI.Filters;

public sealed class TenantHeaderFilter : IOperationFilter
{
    private const string HeaderName = "X-Tenant-Id";

    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        // En OpenAPI.NET 2.x, Parameters es IList<IOpenApiParameter>.
        // List<IOpenApiParameter> es la implementación concreta correcta —
        // List<OpenApiParameter> no es asignable a IList<IOpenApiParameter>
        // aunque OpenApiParameter implemente IOpenApiParameter.
        if (operation.Parameters is null)
            operation.Parameters = new List<IOpenApiParameter>();

        operation.Parameters.Add(new OpenApiParameter
        {
            Name = HeaderName,
            In = ParameterLocation.Header,
            Required = true,
            Description = "ID numérico del tenant activo. Ej: 1",
            Schema = new OpenApiSchema
            {
                // JsonSchemaType es un flags enum — reemplaza al string "integer" de v1.
                Type = JsonSchemaType.Integer
            }
        });
    }
}