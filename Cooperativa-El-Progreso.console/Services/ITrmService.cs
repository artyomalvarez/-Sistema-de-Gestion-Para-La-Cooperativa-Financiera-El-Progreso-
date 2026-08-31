using Cooperativa_El_Progreso.console.Models;

namespace Cooperativa_El_Progreso.console.Services;

public interface ITrmService
{
    Task<TrmResponseDto?> GetCurrentTrmAsync();
}
