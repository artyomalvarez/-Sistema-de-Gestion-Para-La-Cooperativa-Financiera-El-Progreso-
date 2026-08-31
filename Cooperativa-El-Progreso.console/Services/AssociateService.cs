using Cooperativa_El_Progreso.console.Models;
using Cooperativa_El_Progreso.console.Repositories;

namespace Cooperativa_El_Progreso.console.Services;

public class AssociateService
{
    private readonly IAssociateRepository _associateRepository;
    private readonly ITransactionRepository _transactionRepository;

    public AssociateService(IAssociateRepository associateRepository, ITransactionRepository transactionRepository)
    {
        _associateRepository = associateRepository;
        _transactionRepository = transactionRepository;
    }

    public void RegisterAssociate(Associate associate)
    {
        // Validation: Cannot register two associates with the same document number
        var existingAssociate = _associateRepository.GetByDocument(associate.DocumentNumber);
        if (existingAssociate != null)
        {
            throw new InvalidOperationException("Error: Ya existe un asociado con este número de documento.");
        }

        associate.Id = Guid.NewGuid();
        associate.CreatedAt = DateTime.Now;
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
        if (string.IsNullOrWhiteSpace(namePartial))
        {
            return new List<Associate>();
        }

        // Case-insensitive partial name search
        return _associateRepository.GetAll()
            .Where(a => a.Name.Contains(namePartial, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    public void UpdateAssociate(Associate associate)
    {
        var existing = _associateRepository.GetById(associate.Id);
        if (existing == null)
        {
            throw new KeyNotFoundException("Error: Asociado no encontrado.");
        }

        // Only update profile and contact information
        existing.Name = associate.Name;
        existing.Phone = associate.Phone;
        existing.Address = associate.Address;

        _associateRepository.Update(existing);
    }

    public void DeleteAssociate(Guid id)
    {
        var associate = _associateRepository.GetById(id);
        if (associate == null)
        {
            throw new KeyNotFoundException("Error: Asociado no encontrado.");
        }

        // Business Rule: Cannot delete an associate if they have registered transaction history
        var history = _transactionRepository.GetByAssociateId(id);
        if (history.Any())
        {
            throw new InvalidOperationException("Error: No se puede eliminar un asociado que tenga movimientos registrados.");
        }

        _associateRepository.Delete(id);
    }
}
