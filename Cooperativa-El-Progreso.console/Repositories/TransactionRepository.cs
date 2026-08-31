using Cooperativa_El_Progreso.console.Models;

namespace Cooperativa_El_Progreso.console.Repositories;

public class TransactionRepository : ITransactionRepository
{
    private readonly List<Transaction> _transactions = new();

    public void Add(Transaction transaction)
    {
        _transactions.Add(transaction);
    }

    public List<Transaction> GetByAssociateId(Guid associateId)
    {
        return _transactions.Where(t => t.AssociateId == associateId).ToList();
    }

    public List<Transaction> GetAll()
    {
        return _transactions;
    }
}