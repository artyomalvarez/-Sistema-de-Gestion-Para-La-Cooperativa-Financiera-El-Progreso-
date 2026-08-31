using Cooperativa_El_Progreso.console.Models;
using Cooperativa_El_Progreso.console.Repositories;

namespace Cooperativa_El_Progreso.console.Services;

public class TransactionService
{
    private readonly ITransactionRepository _transactionRepository;
    private readonly IAssociateRepository _associateRepository;

    public TransactionService(
        ITransactionRepository transactionRepository,
        IAssociateRepository associateRepository)
    {
        _transactionRepository = transactionRepository;
        _associateRepository = associateRepository;
    }

    public void RegisterDeposit(Guid associateId, decimal amount)
    {
        if (amount <= 0)
        {
            throw new ArgumentException("Deposit amount must be greater than zero.");
        }

        var associate = _associateRepository.GetById(associateId);
        if (associate == null)
        {
            throw new KeyNotFoundException("Associate not found.");
        }

        var transaction = new Transaction
        {
            Id = Guid.NewGuid(),
            AssociateId = associateId,
            Type = TransactionType.Deposit,
            Amount = amount,
            Commission = 0,
            Date = DateTime.Now
        };

        _transactionRepository.Add(transaction);
        associate.Transactions.Add(transaction);
    }

    public void RegisterWithdrawal(Guid associateId, decimal amount)
    {
        if (amount <= 0)
        {
            throw new ArgumentException("Withdrawal amount must be greater than zero.");
        }

        var associate = _associateRepository.GetById(associateId);
        if (associate == null)
        {
            throw new KeyNotFoundException("Associate not found.");
        }

        decimal commission = amount > 1000000 ? 8000 : 0;
        decimal totalDeduction = amount + commission;

        decimal currentBalance = associate.GetBalance();
        if (currentBalance < totalDeduction)
        {
            throw new InvalidOperationException("Insufficient funds. The withdrawal would result in a negative balance.");
        }

        var transaction = new Transaction
        {
            Id = Guid.NewGuid(),
            AssociateId = associateId,
            Type = TransactionType.Withdrawal,
            Amount = amount,
            Commission = commission,
            Date = DateTime.Now
        };

        _transactionRepository.Add(transaction);
        associate.Transactions.Add(transaction);
    }
}
