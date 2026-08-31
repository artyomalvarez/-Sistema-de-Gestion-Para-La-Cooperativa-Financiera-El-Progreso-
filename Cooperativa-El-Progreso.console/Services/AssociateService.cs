using Cooperativa_El_Progreso.console.Models;
using Cooperativa_El_Progreso.console.Repositories;

namespace Cooperativa_El_Progreso.console.Services;

public class AssociateService
{
    private readonly IAssociateRepository _associateRepository;
    private readonly ITransactionRepository _transactionRepository;

    // Inyección de dependencias
    public AssociateService(IAssociateRepository associateRepository, ITransactionRepository transactionRepository)
    {
        _associateRepository = associateRepository;
        _transactionRepository = transactionRepository;
    }

    public void RegisterAssociate(Associate associate)
    {
        // TODO: Validar que NO exista un asociado con el mismo DocumentNumber[cite: 1]
        // TODO: Si existe, lanzar una excepción (throw new Exception("..."))
        // TODO: Si no existe, agregarlo usando _associateRepository.Add(associate)
    }

    public List<Associate> GetAllAssociates()
    {
        return _associateRepository.GetAll();
    }

    public Associate? GetByDocument(string documentNumber)
    {
        return _associateRepository.GetByDocument(documentNumber);
    }

    public List<Associate> SearchByName(string namePartial)
    {
        // TODO: Retornar los asociados cuyo nombre contenga 'namePartial'
        // TODO: Ignorar mayúsculas y minúsculas (usar .ToLower() o StringComparison.OrdinalIgnoreCase)[cite: 1]
        return new List<Associate>(); 
    }

    public void DeleteAssociate(Guid id)
    {
        // TODO: Buscar si el asociado tiene movimientos registrados en _transactionRepository[cite: 1]
        // TODO: Calcular si su saldo actual es mayor a 0[cite: 1]
        // TODO: Si tiene movimientos o saldo, lanzar excepción. Si está en ceros y sin historial, borrarlo.
    }
}