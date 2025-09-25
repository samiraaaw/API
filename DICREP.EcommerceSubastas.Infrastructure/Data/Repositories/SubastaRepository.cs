using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// Data/Repositories/SubastaRepository.cs
using DICREP.EcommerceSubastas.Application.DTOs.Responses;
using DICREP.EcommerceSubastas.Application.DTOs.Subasta;
using DICREP.EcommerceSubastas.Application.Interfaces;
using DICREP.EcommerceSubastas.Infrastructure.Persistence;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System.Data;

namespace DICREP.EcommerceSubastas.Infrastructure.Data.Repositories
{
    public class SubastaRepository : ISubastaRepository
    {
        private readonly EcoCircularContext _context;
        private readonly Serilog.ILogger _logger;

        public SubastaRepository(EcoCircularContext context)
        {
            _context = context;
            _logger = Log.ForContext<SubastaRepository>();
        }

        public async Task<ResponseDTO<ExtraccionSubastaResponseDTO>> ExtraccionSubastaAsync(ExtraccionSubastaRequestDTO request)
        {
            _logger.Information("Iniciando extracción de subasta para empleado {EmpId}", request.EmpId);

            var response = new ResponseDTO<ExtraccionSubastaResponseDTO>();

            try
            {
                // Preparar parámetros
                var totalExtraidasParam = new SqlParameter("@TotalExtraidas", SqlDbType.Int)
                {
                    Direction = ParameterDirection.Output
                };

                var fechaDesdeParam = new SqlParameter("@FechaDesde", request.FechaDesde ?? (object)DBNull.Value);
                var fechaHastaParam = new SqlParameter("@FechaHasta", request.FechaHasta ?? (object)DBNull.Value);
                var organismoIdParam = new SqlParameter("@Organismo_ID", request.OrganismoId ?? (object)DBNull.Value);
                var estBienIdParam = new SqlParameter("@EstBien_ID", request.EstBienId ?? (object)DBNull.Value);
                var empIdParam = new SqlParameter("@Emp_ID", request.EmpId);
                var pcParam = new SqlParameter("@PC", request.PC ?? (object)DBNull.Value);
                var comisionParam = new SqlParameter("@Comision", request.Comision ?? (object)DBNull.Value);
                var incrementoParam = new SqlParameter("@Incremento", request.Incremento ?? (object)DBNull.Value);

                // El procedimiento puede retornar múltiples result sets:
                // 1. Errores de validación (si los hay)
                // 2. Datos de prendas extraídas
                using var command = _context.Database.GetDbConnection().CreateCommand();
                command.CommandText = "dbo.sp_Extraccion_Subasta";
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.Add(fechaDesdeParam);
                command.Parameters.Add(fechaHastaParam);
                command.Parameters.Add(organismoIdParam);
                command.Parameters.Add(estBienIdParam);
                command.Parameters.Add(empIdParam);
                command.Parameters.Add(pcParam);
                command.Parameters.Add(comisionParam);
                command.Parameters.Add(incrementoParam);
                command.Parameters.Add(totalExtraidasParam);

                await _context.Database.OpenConnectionAsync();

                var result = new ExtraccionSubastaResponseDTO();

                using var reader = await command.ExecuteReaderAsync();

                // Primer resultset: Errores de validación (si existe)
                if (reader.HasRows)
                {
                    var errores = new List<PrendaValidacionErrorDTO>();
                    while (await reader.ReadAsync())
                    {
                        errores.Add(new PrendaValidacionErrorDTO
                        {
                            CLPrendaId = reader.GetInt64("CLPrenda_ID"),
                            CLPrendaCod = reader.GetString("CLPrenda_Cod"),
                            Motivo = reader.GetString("Motivo")
                        });
                    }
                    result.ErroresValidacion = errores;
                }

                // Si hay errores, el procedimiento lanzará excepción y no llegará aquí
                // Si no hay errores, leer el segundo resultset con las prendas
                if (await reader.NextResultAsync() && reader.HasRows)
                {
                    var prendas = new List<ExtraccionSubastaItemDTO>();
                    while (await reader.ReadAsync())
                    {
                        prendas.Add(new ExtraccionSubastaItemDTO
                        {
                            CLPrendaCodigo = reader.IsDBNull("CLPrenda_Codigo") ? null : reader.GetString("CLPrenda_Codigo"),
                            FechaIngresoFase2 = reader.IsDBNull("Fecha ingreso Fase 2") ? null : reader.GetDateTime("Fecha ingreso Fase 2"),
                            FechaTerminoFase2 = reader.IsDBNull("Fecha termino Fase 2") ? null : reader.GetDateTime("Fecha termino Fase 2"),
                            Categoria = reader.IsDBNull("Categoria") ? null : reader.GetString("Categoria"),
                            NombreProducto = reader.IsDBNull("NombreProducto") ? null : reader.GetString("NombreProducto"),
                            Descripcion = reader.IsDBNull("Descripción") ? null : reader.GetString("Descripción"),
                            EstadoBien = reader.IsDBNull("Estado bien") ? null : reader.GetString("Estado bien"),
                            Cantidad = reader.IsDBNull("Cantidad") ? null : reader.GetInt32("Cantidad"),
                            PrecioTotal = reader.IsDBNull("Precio Total (Unidad x Cantidad)") ? null : reader.GetDecimal("Precio Total (Unidad x Cantidad)"),
                            Comision = reader.IsDBNull("Comisión") ? null : reader.GetDecimal("Comisión"),
                            Incremento = reader.IsDBNull("Incremento") ? null : reader.GetDecimal("Incremento"),

                            // Datos del organismo
                            NombreOrganizacion = reader.IsDBNull("Nombre Organización") ? null : reader.GetString("Nombre Organización"),
                            RutOrganizacion = reader.IsDBNull("Rut Organización") ? null : reader.GetString("Rut Organización"),
                            Correo = reader.IsDBNull("Correo") ? null : reader.GetString("Correo"),
                            Telefono = reader.IsDBNull("Telefono") ? null : reader.GetString("Telefono"),
                            Comuna = reader.IsDBNull("Comuna") ? null : reader.GetString("Comuna"),
                            DireccionContacto = reader.IsDBNull("Direccion contacto") ? null : reader.GetString("Direccion contacto"),
                            Region = reader.IsDBNull("Región") ? null : reader.GetString("Región"),

                            // Fotos
                            Foto1 = reader.IsDBNull("Foto1") ? null : reader.GetString("Foto1"),
                            Foto2 = reader.IsDBNull("Foto2") ? null : reader.GetString("Foto2"),
                            Foto3 = reader.IsDBNull("Foto3") ? null : reader.GetString("Foto3"),
                            Foto4 = reader.IsDBNull("Foto4") ? null : reader.GetString("Foto4"),
                            Foto5 = reader.IsDBNull("Foto5") ? null : reader.GetString("Foto5"),
                            Foto6 = reader.IsDBNull("Foto6") ? null : reader.GetString("Foto6"),

                            // Informes
                            Informe1 = reader.IsDBNull("Informe1") ? null : reader.GetString("Informe1"),
                            Informe2 = reader.IsDBNull("Informe2") ? null : reader.GetString("Informe2"),
                            Informe3 = reader.IsDBNull("Informe3") ? null : reader.GetString("Informe3"),
                            Informe4 = reader.IsDBNull("Informe4") ? null : reader.GetString("Informe4"),
                            Informe5 = reader.IsDBNull("Informe5") ? null : reader.GetString("Informe5"),
                            Informe6 = reader.IsDBNull("Informe6") ? null : reader.GetString("Informe6")
                        });
                    }
                    result.PrendasExtraidas = prendas;
                }

                // Obtener el total de extraídas del parámetro OUTPUT
                result.TotalExtraidas = (int)totalExtraidasParam.Value;
                result.Mensaje = result.TotalExtraidas == 0
                    ? "No se encontraron prendas que cumplan los criterios de extracción"
                    : $"Se extrajeron {result.TotalExtraidas} prendas exitosamente";

                _logger.Information("Extracción completada. Total extraídas: {Total}", result.TotalExtraidas);

                response.Success = true;
                response.Data = result;
                response.Message = "Extracción realizada exitosamente";

                return response;
            }
            catch (SqlException ex)
            {
                _logger.Error(ex, "Error SQL en extracción de subasta: {ErrorNumber} - {ErrorMessage}",
                    ex.Number, ex.Message);

                var (message, httpStatus, businessCode) = MapSqlError(ex.Number, ex.Message);

                response.Success = false;
                response.Error = new ErrorResponseDto
                {
                    ErrorCode = businessCode ?? ex.Number,
                    Message = message,
                    HttpStatusCode = httpStatus ?? 400
                };
                response.Message = "Error en la extracción de subasta";

                return response;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error inesperado en extracción de subasta");

                response.Success = false;
                response.Error = new ErrorResponseDto
                {
                    ErrorCode = 500,
                    Message = "Error interno del servidor",
                    HttpStatusCode = 500
                };
                response.Message = "Error interno del servidor";

                return response;
            }
            finally
            {
                await _context.Database.CloseConnectionAsync();
            }
        }

        private static (string Message, int? HttpStatusCode, int? BusinessCode) MapSqlError(int errorCode, string originalMessage)
        {
            return errorCode switch
            {
                53003 => ("ID de empleado requerido para auditoría", 400, 53003),
                53006 => ("Rango de fechas inválido: FechaDesde > FechaHasta", 400, 53006),
                53011 => (originalMessage, 400, 53011), // Prendas con campos inválidos
                _ => (originalMessage, 400, errorCode)
            };
        }
    }
}
