using OverLoad.Models;

namespace OverLoad.Application.Ports;

/// <summary>
/// Puerto de salida para persistir y recuperar el perfil metabólico activo de un
/// usuario (un único perfil por usuario, se sobrescribe con cada nuevo cálculo).
/// </summary>
public interface IPerfilMetabolicoRepository
{
    Task<PerfilMetabolico?> ObtenerAsync(string userId);
    Task GuardarAsync(PerfilMetabolico perfil);
}
