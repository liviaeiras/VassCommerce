using System.Text;

namespace VassCommerce.Api.Services;

public static class JwtConfigurationValidator
{
    public static void Validate(IConfigurationSection jwt)
    {
        var key = jwt["Key"];

        if (string.IsNullOrWhiteSpace(key))
        {
            throw new InvalidOperationException(
                "Configure Jwt:Key usando User Secrets ou uma variável de ambiente."
            );
        }

        if (Encoding.UTF8.GetByteCount(key) < 32)
        {
            throw new InvalidOperationException(
                "Jwt:Key deve conter pelo menos 32 bytes. Configure-a usando User Secrets ou uma variável de ambiente."
            );
        }

        if (string.IsNullOrWhiteSpace(jwt["Issuer"]))
        {
            throw new InvalidOperationException(
                "Configure Jwt:Issuer antes de iniciar a API."
            );
        }

        if (string.IsNullOrWhiteSpace(jwt["Audience"]))
        {
            throw new InvalidOperationException(
                "Configure Jwt:Audience antes de iniciar a API."
            );
        }

        if (jwt.GetValue<int?>("ExpiresMinutes") is null or <= 0)
        {
            throw new InvalidOperationException(
                "Jwt:ExpiresMinutes deve ser um número maior que zero."
            );
        }
    }
}
