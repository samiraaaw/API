using DICREP.EcommerceSubastas.API.Attributes;
using DICREP.EcommerceSubastas.Application.DTOs.Auth;
using DICREP.EcommerceSubastas.Application.UseCases.Acceso;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DICREP.EcommerceSubastas.API.Controllers
{
    [DevelopmentOnly]
    [Route("api/[controller]")]
    [ApiController]
    public class AccesoController : ControllerBase
    {
        private readonly GetAllFuncionalidadesUseCase _getAllFuncionalidadesUseCase;

        public AccesoController(GetAllFuncionalidadesUseCase getAllFuncionalidadesUseCase)
        {
            _getAllFuncionalidadesUseCase = getAllFuncionalidadesUseCase; 
        }

    }
}
