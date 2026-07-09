using OverLoad.Models;

namespace OverLoad.Application.Ports;

/// <summary>
/// Puerto de salida para los ejercicios de la librería creados por el usuario.
/// </summary>
public interface IEjercicioPersonalizadoRepository
{
    Task<IReadOnlyList<EjercicioPersonalizado>> ObtenerDeUsuarioAsync(string userId);
    Task AgregarAsync(EjercicioPersonalizado ejercicio);
}
