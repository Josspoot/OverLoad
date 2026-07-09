using OverLoad.Models;

namespace OverLoad.Application.Ports;

/// <summary>Puerto de salida para el registro diario de alimentos de la Bitácora.</summary>
public interface IRegistroAlimentoRepository
{
    Task<IReadOnlyList<RegistroAlimento>> ObtenerDelDiaAsync(string userId, DateOnly fecha);
    Task AgregarAsync(RegistroAlimento registro);
    Task EliminarAsync(int id, string userId);
}
