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
        // Regla: No pueden existir dos asociados con el mismo documento
        var existingAssociate = _associateRepository.GetByDocument(associate.DocumentNumber);
        
        if (existingAssociate != null)
        {
            throw new InvalidOperationException("Error: Ya existe un asociado con este número de documento.");
        }

        // Asignamos ID y fecha de creación automáticamente
        associate.Id = Guid.NewGuid();
        associate.CreatedAt = DateTime.Now;
        
        // Al registrarse, la lista de transacciones inicia vacía (saldo en cero)
        _associateRepository.Add(associate);
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
        var associate = _associateRepository.GetById(id);
        if (associate == null)
        {
            throw new KeyNotFoundException("Error: Asociado no encontrado.");
        }

        // Regla: No se puede eliminar si tiene movimientos
        var history = _transactionRepository.GetByAssociateId(id);
        if (history.Any())
        {
            throw new InvalidOperationException("Error: No se puede eliminar un asociado que tenga movimientos registrados.");
        }

        _associateRepository.Delete(id);
    }
}