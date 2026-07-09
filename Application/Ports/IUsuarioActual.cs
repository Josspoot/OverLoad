namespace OverLoad.Application.Ports;

/// <summary>
/// Puerto que expone la identidad del usuario autenticado al núcleo sin acoplarlo
/// a ASP.NET. El adaptador web lo resuelve a partir del <c>HttpContext</c>.
/// </summary>
public interface IUsuarioActual
{
    /// <summary>Id del usuario autenticado, o <c>null</c> si es anónimo.</summary>
    string? Id { get; }

    /// <summary>Indica si hay un usuario autenticado en la petición actual.</summary>
    bool Autenticado { get; }
}
