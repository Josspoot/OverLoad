namespace OverLoad.Application.Progresion;

/// <summary>
/// Política de dominio para el recordatorio de sobrecarga progresiva.
///
/// La evidencia recomienda aplicar/reevaluar la sobrecarga progresiva en una
/// ventana de 1 a 2 semanas por ejercicio (los principiantes progresan casi cada
/// sesión; los intermedios cada 1-2 semanas), con reevaluación/deload cada 4-6
/// semanas. Se toma 14 días (2 semanas) como periodo por defecto del recordatorio.
///
/// Fuentes: Stronger by Science (Progressive Overload Strategies),
/// Cleveland Clinic (Progressive Overload), NASM (Progressive Overload Explained).
/// </summary>
public static class PoliticaProgresion
{
    /// <summary>Periodo, en días, tras el cual se sugiere progresar un ejercicio.</summary>
    public const int DiasPeriodo = 14;

    /// <summary>Fecha en que corresponde revisar/progresar el ejercicio.</summary>
    public static DateTime ProximaRevision(DateTime ultimaActualizacion) =>
        ultimaActualizacion.AddDays(DiasPeriodo);

    /// <summary>True si ya se cumplió (o pasó) el periodo para progresar.</summary>
    public static bool TocaProgresar(DateTime ultimaActualizacion, DateTime ahora) =>
        ahora >= ProximaRevision(ultimaActualizacion);

    /// <summary>Días que faltan para la próxima revisión (0 o menos si ya toca).</summary>
    public static int DiasRestantes(DateTime ultimaActualizacion, DateTime ahora) =>
        (int)Math.Ceiling((ProximaRevision(ultimaActualizacion) - ahora).TotalDays);
}
