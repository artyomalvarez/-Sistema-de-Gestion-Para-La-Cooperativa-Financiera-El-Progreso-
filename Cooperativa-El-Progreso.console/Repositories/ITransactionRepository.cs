using Cooperativa_El_Progreso.console.Models;

namespace Cooperativa_El_Progreso.console.Repositories;

public interface ITransactionRepository
{
    void Add(Transaction transaction);
    List<Transaction> GetByAssociateId(Guid associateId);
    List<Transaction> GetAll();
}