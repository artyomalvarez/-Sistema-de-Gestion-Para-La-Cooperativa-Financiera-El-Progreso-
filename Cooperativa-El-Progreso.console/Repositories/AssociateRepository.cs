using Cooperativa_El_Progreso.console.Models;

namespace Cooperativa_El_Progreso.console.Repositories;

public class AssociateRepository : IAssociateRepository
{
    // Simulamos la base de datos en memoria
    private readonly List<Associate> _associates = new();

    public void Add(Associate associate)
    {
        _associates.Add(associate);
    }

    public List<Associate> GetAll()
    {
        return _associates;
    }

    public Associate? GetById(Guid id)
    {
        return _associates.FirstOrDefault(a => a.Id == id);
    }

    public Associate? GetByDocument(string documentNumber)
    {
        return _associates.FirstOrDefault(a => a.DocumentNumber == documentNumber);
    }

    public void Update(Associate associate)
    {
        var existing = GetById(associate.Id);
        if (existing != null)
        {
            existing.Name = associate.Name;
            existing.Phone = associate.Phone;
            existing.Address = associate.Address;
        }
    }

    public void Delete(Guid id)
    {
        var associate = GetById(id);
        if (associate != null)
        {
            _associates.Remove(associate);
        }
    }
}