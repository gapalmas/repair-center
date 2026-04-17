using IdentityCenter.API.Security;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace IdentityCenter.API.Swagger;

public sealed class AuthCookieOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var path = context.ApiDescription.RelativePath?.TrimEnd('/');
        if (string.IsNullOrWhiteSpace(path))
            return;

        if (path.Equals("api/Auth/login", StringComparison.OrdinalIgnoreCase))
        {
            operation.Summary = "Inicia sesión y emite access token";
            operation.Description =
                "Devuelve el access token en el body. El refresh token se guarda en una cookie Secure + HttpOnly, el token CSRF se devuelve en el body y en una cookie segura no HttpOnly, y la sesión queda ligada al fingerprint calculado desde X-Device-Id + User-Agent.";

            operation.Parameters ??= [];
            operation.Parameters.Add(new OpenApiParameter
            {
                Name = ClientSessionService.DeviceIdHeaderName,
                In = ParameterLocation.Header,
                Required = false,
                Description = "Identificador estable del dispositivo o cliente. Swagger UI lo inyecta automáticamente.",
                Schema = new OpenApiSchema { Type = JsonSchemaType.String }
            });

            EnsureJsonResponseDescription(operation, "Respuesta con access token y csrfToken. El refresh token no se expone en el body.");
            return;
        }

        if (path.Equals("api/Auth/refresh", StringComparison.OrdinalIgnoreCase) ||
            path.Equals("api/Auth/revoke", StringComparison.OrdinalIgnoreCase))
        {
            operation.Description =
                "Requiere cookie segura de refresh token, cabecera X-CSRF-TOKEN y Origin/Referer permitido. El refresh token no se envía en el body.";

            operation.Parameters ??= [];
            operation.Parameters.Add(new OpenApiParameter
            {
                Name = AuthCookieService.CsrfHeaderName,
                In = ParameterLocation.Header,
                Required = true,
                Description = "Token CSRF de doble submit. Debe coincidir con la cookie ic_csrf_token.",
                Schema = new OpenApiSchema { Type = JsonSchemaType.String }
            });

            operation.Parameters.Add(new OpenApiParameter
            {
                Name = ClientSessionService.DeviceIdHeaderName,
                In = ParameterLocation.Header,
                Required = false,
                Description = "Debe coincidir con el valor usado al crear la sesión. Swagger UI lo reenvía automáticamente.",
                Schema = new OpenApiSchema { Type = JsonSchemaType.String }
            });

            operation.Parameters.Add(new OpenApiParameter
            {
                Name = "Origin",
                In = ParameterLocation.Header,
                Required = false,
                Description = "Origen permitido para endurecer protección CSRF. En navegador suele enviarse automáticamente.",
                Schema = new OpenApiSchema { Type = JsonSchemaType.String }
            });

            operation.Responses ??= [];
            operation.Responses.TryAdd("403", new OpenApiResponse
            {
                Description = "CSRF inválido u Origin/Referer no permitido."
            });

            if (path.Equals("api/Auth/refresh", StringComparison.OrdinalIgnoreCase))
            {
                operation.Summary = "Rota refresh token usando cookie segura";
                EnsureJsonResponseDescription(operation, "Respuesta con nuevo access token y nuevo csrfToken. La cookie de refresh también se rota.");
            }
            else
            {
                operation.Summary = "Revoca el refresh token actual";
                operation.Responses.TryAdd("204", new OpenApiResponse
                {
                    Description = "Refresh token revocado y cookies eliminadas."
                });
            }
        }
    }

    private static void EnsureJsonResponseDescription(OpenApiOperation operation, string description)
    {
        operation.Responses ??= [];

        if (operation.Responses.TryGetValue("200", out var okResponse))
        {
            okResponse.Description = description;
        }
        else
        {
            operation.Responses["200"] = new OpenApiResponse { Description = description };
        }
    }
}