using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using DICREP.EcommerceSubastas.API.Filters;
using DICREP.EcommerceSubastas.Application.DTOs.Responses;
using DICREP.EcommerceSubastas.Application.DTOs.Subasta;
using DICREP.EcommerceSubastas.Application.UseCases.Subasta;
using Microsoft.AspNetCore.Authorization;
using Serilog;

namespace DICREP.EcommerceSubastas.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class SubastaController : ControllerBase
    {
        private readonly ExtraccionSubastaUseCase _extraccionSubastaUseCase;
        private readonly Serilog.ILogger _logger;

        public SubastaController(ExtraccionSubastaUseCase extraccionSubastaUseCase)
        {
            _extraccionSubastaUseCase = extraccionSubastaUseCase;
            _logger = Log.ForContext<SubastaController>();
        }

        /// <summary>
        /// Extrae prendas para subasta y cambia su estado a "En subasta"
        /// </summary>
        /// <param name="request">Filtros de extracción</param>
        /// <returns>Lista de prendas extraídas con toda su información</returns>
        [HttpPost("ExtraccionSubasta")]
        [ServiceFilter(typeof(ValidateModelAttribute))]
        [AllowAnonymous]
        public async Task<ActionResult<ResponseDTO<ExtraccionSubastaResponseDTO>>> ExtraccionSubasta(
            [FromBody] ExtraccionSubastaRequestDTO request)
        {
            _logger.Information("Recibiendo solicitud de extracción de subasta para empleado {EmpId}",
                request?.EmpId);

            var result = await _extraccionSubastaUseCase.ExecuteAsync(request);

            if (!result.Success)
            {
                _logger.Warning("Error en extracción de subasta: {ErrorMessage}",
                    result.Error?.Message);

                var statusCode = result.Error?.HttpStatusCode ?? StatusCodes.Status400BadRequest;
                return StatusCode(statusCode, result);
            }

            _logger.Information("Extracción de subasta completada. Total extraídas: {Total}",
                result.Data?.TotalExtraidas);

            return Ok(result);
        }

        /// <summary>
        /// Obtiene un resumen de prendas disponibles para extracción (sin cambiar estado)
        /// </summary>
        /// <param name="fechaDesde">Fecha desde (opcional)</param>
        /// <param name="fechaHasta">Fecha hasta (opcional)</param>
        /// <param name="organismoId">ID del organismo (opcional)</param>
        /// <param name="estBienId">ID del estado del bien (opcional)</param>
        /// <returns>Conteo de prendas que se extraerían</returns>
        [HttpGet("Preview")]
        [AllowAnonymous]
        public async Task<ActionResult<object>> PreviewExtraccion(
            [FromQuery] DateTime? fechaDesde = null,
            [FromQuery] DateTime? fechaHasta = null,
            [FromQuery] int? organismoId = null,
            [FromQuery] int? estBienId = null)
        {
            // Este endpoint podría implementarse para mostrar un preview
            // sin ejecutar la extracción real
            _logger.Information("Preview de extracción solicitado");

            // Implementación simple - podrías crear otro SP o consulta
            return Ok(new
            {
                mensaje = "Funcionalidad de preview no implementada",
                sugerencia = "Use el endpoint de extracción con un empleado de prueba"
            });
        }
    }
}
