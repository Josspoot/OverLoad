using System.Globalization;
using System.Text;
using OverLoad.Application.Ports;
using OverLoad.Models;

namespace OverLoad.Application.Libreria;

/// <summary>
/// Servicio de aplicación de la librería. Combina el catálogo estático de
/// ejercicios con los ejercicios personalizados del usuario autenticado y
/// gestiona el alta de estos últimos.
/// </summary>
public class LibreriaService(
    CatalogoEjercicios catalogo,
    IEjercicioPersonalizadoRepository personalizados,
    IUsuarioActual usuario)
{
    /// <summary>Grupos de la librería: catálogo base + ejercicios del usuario.</summary>
    public async Task<IReadOnlyList<GrupoEjercicios>> ObtenerGruposAsync()
    {
        // Copia mutable del catálogo estático (no se altera el original).
        var grupos = catalogo.ObtenerGrupos()
            .ToDictionary(g => g.Nombre, g => g.Ejercicios.ToList());

        if (usuario.Autenticado)
        {
            var mios = await personalizados.ObtenerDeUsuarioAsync(usuario.Id!);
            foreach (var e in mios)
            {
                var info = new EjercicioInfo(e.Slug, e.Nombre, e.Grupo,
                    e.ComoHacerlo, e.QueSeDebeSentir, e.Recomendaciones, e.EquipoExtra);

                if (grupos.TryGetValue(e.Grupo, out var lista))
                    lista.Add(info);
                else
                    grupos[e.Grupo] = [info];
            }
        }

        return grupos.Select(kv => new GrupoEjercicios(kv.Key, kv.Value)).ToList();
    }

    /// <summary>Da de alta un ejercicio personalizado para el usuario actual.</summary>
    public async Task AgregarAsync(string nombre, string grupo, string comoHacerlo,
        string queSeDebeSentir, string recomendaciones, string equipoExtra)
    {
        if (!usuario.Autenticado) return;

        await personalizados.AgregarAsync(new EjercicioPersonalizado
        {
            UserId = usuario.Id!,
            Slug = GenerarSlug(nombre),
            Nombre = nombre,
            Grupo = grupo,
            ComoHacerlo = comoHacerlo,
            QueSeDebeSentir = queSeDebeSentir,
            Recomendaciones = recomendaciones,
            EquipoExtra = equipoExtra
        });
    }

    // Convierte "Curl de bíceps" en "curl-de-biceps" (sin acentos ni símbolos).
    private static string GenerarSlug(string texto)
    {
        var normalizado = texto.Trim().ToLowerInvariant().Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder();
        foreach (var c in normalizado)
        {
            var cat = CharUnicodeInfo.GetUnicodeCategory(c);
            if (cat == UnicodeCategory.NonSpacingMark) continue;
            if (char.IsLetterOrDigit(c)) sb.Append(c);
            else if (c is ' ' or '-' or '_') sb.Append('-');
        }

        var slug = sb.ToString();
        while (slug.Contains("--")) slug = slug.Replace("--", "-");
        return slug.Trim('-') is { Length: > 0 } limpio ? limpio : "ejercicio";
    }
}
