using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DICREP.EcommerceSubastas.Application.DTOs.Responses;
using DICREP.EcommerceSubastas.Application.DTOs.Subasta;

namespace DICREP.EcommerceSubastas.Application.Interfaces
{
    public interface ISubastaRepository
    {
        Task<ResponseDTO<ExtraccionSubastaResponseDTO>> ExtraccionSubastaAsync(ExtraccionSubastaRequestDTO request);
    }
}
