using Microsoft.AspNetCore.Mvc;
using OverLoad.Application.Ports;
using OverLoad.Controllers.Api.Contracts;

namespace OverLoad.Controllers.Api;

/// <summary>
/// Adaptador de entrada REST. Expone los casos de uso del núcleo
/// (<see cref="IEjercicioService"/>) como una API JSON, sin conocer EF Core
/// ni la base de datos. Es un canal de entrada alterno al controlador web MVC.
/// </summary>
[ApiController]
[Route("api/v1/ejercicios")]
[Produces("application/json")]
public class EjerciciosApiController(IEjercicioService ejercicios) : ControllerBase
{
    /// <summary>Lista todos los ejercicios registrados.</summary>
    /// <response code="200">Devuelve la colección de ejercicios.</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<EjercicioResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<EjercicioResponse>>> Listar()
    {
        var lista = await ejercicios.ListarAsync();
        return Ok(lista.Select(EjercicioResponse.From));
    }

    /// <summary>Obtiene un ejercicio por su identificador.</summary>
    /// <param name="id">Identificador del ejercicio.</param>
    /// <response code="200">Ejercicio encontrado.</response>
    /// <response code="404">No existe un ejercicio con ese id.</response>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(EjercicioResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EjercicioResponse>> ObtenerPorId(int id)
    {
        var ejercicio = await ejercicios.ObtenerAsync(id);
        return ejercicio is null ? NotFound() : Ok(EjercicioResponse.From(ejercicio));
    }

    /// <summary>Registra un nuevo ejercicio.</summary>
    /// <response code="201">Ejercicio creado; la cabecera Location apunta al recurso.</response>
    /// <response code="400">Los datos enviados no son válidos.</response>
    [HttpPost]
    [ProducesResponseType(typeof(EjercicioResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<EjercicioResponse>> Crear(CrearEjercicioRequest request)
    {
        var ejercicio = request.ToEntity();
        await ejercicios.RegistrarAsync(ejercicio);
        return CreatedAtAction(nameof(ObtenerPorId), new { id = ejercicio.Id }, EjercicioResponse.From(ejercicio));
    }

    /// <summary>Actualiza la carga (series, repeticiones y peso) de un ejercicio.</summary>
    /// <param name="id">Identificador del ejercicio.</param>
    /// <param name="request">Nuevos valores de carga.</param>
    /// <response code="204">Carga actualizada correctamente.</response>
    /// <response code="404">No existe un ejercicio con ese id.</response>
    [HttpPut("{id:int}/carga")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ActualizarCarga(int id, ActualizarCargaRequest request)
    {
        if (await ejercicios.ObtenerAsync(id) is null) return NotFound();

        await ejercicios.ActualizarCargaAsync(id, request.Series, request.Repeticiones, request.Peso);
        return NoContent();
    }

    /// <summary>Sugiere la carga para la próxima sesión de un ejercicio.</summary>
    /// <remarks>
    /// Aplica una estrategia de progresión (patrón Strategy, ver ADR-04).
    /// Estrategias válidas: <c>peso</c>, <c>repeticiones</c>, <c>series</c>, <c>doble</c>.
    /// </remarks>
    /// <param name="id">Identificador del ejercicio.</param>
    /// <param name="estrategia">Clave de la estrategia de progresión a aplicar.</param>
    /// <response code="200">Devuelve la carga sugerida y su justificación.</response>
    /// <response code="400">La estrategia indicada no es válida.</response>
    /// <response code="404">No existe un ejercicio con ese id.</response>
    [HttpGet("{id:int}/sugerencia")]
    [ProducesResponseType(typeof(SugerenciaProgresionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SugerenciaProgresionResponse>> Sugerir(int id, [FromQuery] string estrategia = "doble")
    {
        var disponibles = ejercicios.EstrategiasDeProgresion();
        if (!disponibles.Contains(estrategia, StringComparer.OrdinalIgnoreCase))
            return BadRequest(new { mensaje = "Estrategia no válida.", estrategiasDisponibles = disponibles });

        var sugerencia = await ejercicios.SugerirProgresionAsync(id, estrategia);
        return sugerencia is null ? NotFound() : Ok(SugerenciaProgresionResponse.From(sugerencia));
    }

    /// <summary>Elimina un ejercicio.</summary>
    /// <param name="id">Identificador del ejercicio.</param>
    /// <response code="204">Ejercicio eliminado.</response>
    /// <response code="404">No existe un ejercicio con ese id.</response>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Eliminar(int id)
    {
        if (await ejercicios.ObtenerAsync(id) is null) return NotFound();

        await ejercicios.EliminarAsync(id);
        return NoContent();
    }
}
