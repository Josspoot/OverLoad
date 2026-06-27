using System.Globalization;

namespace OverLoad.Application.Nutricion;

/// <summary>
/// Servicio de dominio (núcleo). Calcula la Tasa Metabólica Basal (TMB), el
/// gasto energético total diario (TDEE), las calorías objetivo, el desglose de
/// macronutrientes y la estimación de tiempo para un objetivo de peso.
/// Es lógica pura: no depende de ASP.NET, EF Core ni de ninguna infraestructura.
/// </summary>
public class CalculadoraMetabolica
{
    /// <summary>Energía aproximada (kcal) equivalente a 1 kg de peso corporal.</summary>
    private const double KcalPorKg = 7700;

    // Factores de actividad estándar para estimar el TDEE a partir de la TMB.
    private static readonly (NivelActividad Nivel, string Nombre, string Descripcion, double Factor)[] Niveles =
    {
        (NivelActividad.Sedentario, "Sedentario", "Poco o nada de ejercicio, trabajo de escritorio", 1.20),
        (NivelActividad.Ligero, "Ligeramente activo", "Ejercicio ligero 1-3 días por semana", 1.375),
        (NivelActividad.Moderado, "Moderadamente activo", "Ejercicio moderado 3-5 días por semana", 1.55),
        (NivelActividad.Activo, "Muy activo", "Ejercicio intenso 6-7 días por semana", 1.725),
        (NivelActividad.MuyActivo, "Extra activo", "Ejercicio muy intenso o trabajo físico", 1.90)
    };

    public ResultadoMetabolico Calcular(DatosMetabolicos datos)
    {
        var (tmb, generica, calculada, nombreFormula) = CalcularTmb(datos);

        var nivel = Niveles.First(n => n.Nivel == datos.NivelActividad);
        var tdee = tmb * nivel.Factor;

        var (ajusteDiario, nombreObjetivo, descripcionObjetivo) = AjustePorObjetivo(datos.Objetivo);
        var caloriasObjetivo = tdee + ajusteDiario;

        // "Categorías de mantenimiento": calorías de mantenimiento en cada nivel de actividad.
        var mantenimiento = Niveles
            .Select(n => new CategoriaMantenimiento(n.Nombre, n.Descripcion, n.Factor, Math.Round(tmb * n.Factor)))
            .ToList();

        var macros = CalcularMacros(caloriasObjetivo, datos.PesoKg);
        var tiempo = EstimarTiempo(datos, ajusteDiario);

        return new ResultadoMetabolico(
            Tmb: Math.Round(tmb),
            Tdee: Math.Round(tdee),
            CaloriasObjetivo: Math.Round(caloriasObjetivo),
            AjusteDiario: ajusteDiario,
            NombreFormula: nombreFormula,
            FormulaGenerica: generica,
            FormulaCalculada: calculada,
            NombreObjetivo: nombreObjetivo,
            DescripcionObjetivo: descripcionObjetivo,
            NombreActividad: nivel.Nombre,
            FactorActividad: nivel.Factor,
            Mantenimiento: mantenimiento,
            Macros: macros,
            Tiempo: tiempo);
    }

    // TMB según la fórmula y el sexo. Devuelve también el texto de la fórmula
    // genérica y la fórmula con los valores ya sustituidos (para mostrarla).
    private static (double Tmb, string Generica, string Calculada, string Nombre) CalcularTmb(DatosMetabolicos d)
    {
        double p = d.PesoKg, a = d.AlturaCm;
        int e = d.Edad;
        string Num(double x) => x.ToString("0.##", CultureInfo.InvariantCulture);

        if (d.Formula == FormulaTmb.MifflinStJeor)
        {
            if (d.Sexo == Sexo.Hombre)
            {
                var tmb = 10 * p + 6.25 * a - 5 * e + 5;
                return (tmb,
                    "TMB = 10 · peso(kg) + 6.25 · altura(cm) − 5 · edad + 5",
                    $"TMB = 10·{Num(p)} + 6.25·{Num(a)} − 5·{e} + 5 = {Num(Math.Round(tmb))} kcal",
                    "Mifflin-St Jeor");
            }
            else
            {
                var tmb = 10 * p + 6.25 * a - 5 * e - 161;
                return (tmb,
                    "TMB = 10 · peso(kg) + 6.25 · altura(cm) − 5 · edad − 161",
                    $"TMB = 10·{Num(p)} + 6.25·{Num(a)} − 5·{e} − 161 = {Num(Math.Round(tmb))} kcal",
                    "Mifflin-St Jeor");
            }
        }

        // Harris-Benedict (revisada por Roza y Shizgal, 1984).
        if (d.Sexo == Sexo.Hombre)
        {
            var tmb = 88.362 + 13.397 * p + 4.799 * a - 5.677 * e;
            return (tmb,
                "TMB = 88.362 + 13.397 · peso + 4.799 · altura − 5.677 · edad",
                $"TMB = 88.362 + 13.397·{Num(p)} + 4.799·{Num(a)} − 5.677·{e} = {Num(Math.Round(tmb))} kcal",
                "Harris-Benedict (revisada)");
        }
        else
        {
            var tmb = 447.593 + 9.247 * p + 3.098 * a - 4.330 * e;
            return (tmb,
                "TMB = 447.593 + 9.247 · peso + 3.098 · altura − 4.330 · edad",
                $"TMB = 447.593 + 9.247·{Num(p)} + 3.098·{Num(a)} − 4.330·{e} = {Num(Math.Round(tmb))} kcal",
                "Harris-Benedict (revisada)");
        }
    }

    // Ajuste calórico diario (sobre el TDEE) según el objetivo.
    private static (int Ajuste, string Nombre, string Descripcion) AjustePorObjetivo(ObjetivoNutricional o) => o switch
    {
        ObjetivoNutricional.PerderPeso =>
            (-500, "Perder peso", "Déficit calórico moderado de 500 kcal/día (aprox. 0.45 kg por semana)."),
        ObjetivoNutricional.GanarMusculo =>
            (300, "Ganar músculo", "Superávit calórico controlado de 300 kcal/día para minimizar la ganancia de grasa."),
        ObjetivoNutricional.SubirPeso =>
            (500, "Subir de peso", "Superávit calórico de 500 kcal/día (aprox. 0.45 kg por semana)."),
        _ =>
            (0, "Mantenimiento", "Mantener el peso actual consumiendo aproximadamente las calorías de tu TDEE.")
    };

    // Reparto de macronutrientes: proteína fija por kg, grasa al 25% de las
    // calorías y el resto en carbohidratos.
    private static DesgloseMacros CalcularMacros(double calorias, double pesoKg)
    {
        double proteinaG = 2.0 * pesoKg;
        double proteinaKcal = proteinaG * 4;

        double grasaKcal = calorias * 0.25;
        double grasaG = grasaKcal / 9;

        double carboKcal = Math.Max(0, calorias - proteinaKcal - grasaKcal);
        double carboG = carboKcal / 4;

        return new DesgloseMacros(
            ProteinaG: (int)Math.Round(proteinaG), ProteinaKcal: (int)Math.Round(proteinaKcal),
            GrasaG: (int)Math.Round(grasaG), GrasaKcal: (int)Math.Round(grasaKcal),
            CarbohidratoG: (int)Math.Round(carboG), CarboKcal: (int)Math.Round(carboKcal));
    }

    // Estima el tiempo para alcanzar el cambio de peso deseado a partir del
    // déficit/superávit diario y la equivalencia ~7700 kcal por kg.
    private static EstimacionTiempo? EstimarTiempo(DatosMetabolicos d, int ajusteDiario)
    {
        if (ajusteDiario == 0 || d.CambioPesoKg <= 0) return null;

        double ritmoSemanalKg = Math.Abs(ajusteDiario) * 7 / KcalPorKg;
        int semanas = (int)Math.Ceiling(d.CambioPesoKg / ritmoSemanalKg);
        double meses = Math.Round(semanas / 4.345, 1);

        string nota = d.Objetivo == ObjetivoNutricional.GanarMusculo
            ? "La ganancia de músculo real es más lenta que el cambio de peso: parte del aumento será agua y algo de grasa. " +
              "Un ritmo natural ronda 0.25-0.5 kg de músculo al mes."
            : "Estimación basada en que ~7700 kcal equivalen a 1 kg de peso corporal. " +
              "El ritmo real varía según adherencia, metabolismo y composición corporal.";

        return new EstimacionTiempo(d.CambioPesoKg, Math.Round(ritmoSemanalKg, 2), semanas, meses, nota);
    }
}
