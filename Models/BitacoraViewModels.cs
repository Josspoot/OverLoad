using OverLoad.Application.Nutricion;

namespace OverLoad.Models;

/// <summary>Vista principal de la Bitácora: registros del día, totales y progreso vs objetivo.</summary>
public class BitacoraViewModel
{
    public DateOnly Fecha { get; set; }
    public PerfilMetabolico? Perfil { get; set; }
    public IReadOnlyList<RegistroAlimento> Registros { get; set; } = [];

    public double CaloriasConsumidas { get; set; }
    public double ProteinaConsumida { get; set; }
    public double CarboConsumido { get; set; }
    public double GrasaConsumida { get; set; }

    public double CaloriasObjetivo => Perfil?.CaloriasObjetivo ?? 0;
    public bool TienePerfil => Perfil is not null && CaloriasObjetivo > 0;

    /// <summary>Porcentaje de las calorías objetivo ya consumidas (0-100 para la barra).</summary>
    public int PorcentajeCalorias =>
        CaloriasObjetivo > 0 ? (int)Math.Clamp(Math.Round(CaloriasConsumidas / CaloriasObjetivo * 100), 0, 100) : 0;

    /// <summary>Porcentaje real (puede pasar de 100 si hay exceso), para el texto.</summary>
    public int PorcentajeCaloriasReal =>
        CaloriasObjetivo > 0 ? (int)Math.Round(CaloriasConsumidas / CaloriasObjetivo * 100) : 0;

    public double CaloriasRestantes => Math.Max(0, CaloriasObjetivo - CaloriasConsumidas);
    public bool HayExceso => CaloriasObjetivo > 0 && CaloriasConsumidas > CaloriasObjetivo;

    public int PorcentajeMacro(double consumido, int objetivo) =>
        objetivo > 0 ? (int)Math.Clamp(Math.Round(consumido / objetivo * 100), 0, 100) : 0;
}

/// <summary>Vista de la sección de búsqueda de alimentos (conectada a Open Food Facts).</summary>
public class BuscarAlimentoViewModel
{
    public string Termino { get; set; } = string.Empty;
    public bool Buscado { get; set; }
    public IReadOnlyList<AlimentoEncontrado> Resultados { get; set; } = [];
}

/// <summary>Datos que envía el formulario "añadir alimento" al registrarlo en el día.</summary>
public class AgregarAlimentoInput
{
    public string Nombre { get; set; } = string.Empty;
    public string Marca { get; set; } = string.Empty;
    public string CodigoBarras { get; set; } = string.Empty;
    public double CaloriasPor100g { get; set; }
    public double ProteinaPor100g { get; set; }
    public double CarboPor100g { get; set; }
    public double GrasaPor100g { get; set; }
    public double Gramos { get; set; } = 100;
}
