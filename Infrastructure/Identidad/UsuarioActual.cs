using System.Security.Claims;
using OverLoad.Application.Ports;

namespace OverLoad.Infrastructure.Identidad;

/// <summary>
/// Adaptador del puerto <see cref="IUsuarioActual"/>. Obtiene el id del usuario
/// autenticado desde el <c>HttpContext</c> (claim NameIdentifier de ASP.NET Identity).
/// </summary>
public class UsuarioActual(IHttpContextAccessor accessor) : IUsuarioActual
{
    public string? Id => accessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);

    public bool Autenticado => Id is not null;
}
