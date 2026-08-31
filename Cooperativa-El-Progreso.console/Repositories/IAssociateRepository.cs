using Cooperativa_El_Progreso.console.Models;

namespace Cooperativa_El_Progreso.console.Repositories;

public interface IAssociateRepository
{
    void Add(Associate associate);
    List<Associate> GetAll();
    Associate? GetById(Guid id);
    Associate? GetByDocument(string documentNumber);
    void Update(Associate associate);
    void Delete(Guid id);
}