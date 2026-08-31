using Cooperativa_El_Progreso.console.Models;
using Cooperativa_El_Progreso.console.Repositories;

namespace Cooperativa_El_Progreso.console.Services;

public class TransactionService
{
    private readonly ITransactionRepository _transactionRepository;
    private readonly IAssociateRepository _associateRepository;
    private readonly ITrmService _trmService;

    public TransactionService(
        ITransactionRepository transactionRepository,
        IAssociateRepository associateRepository,
        ITrmService? trmService = null)
    {
        _transactionRepository = transactionRepository;
        _associateRepository = associateRepository;
        _trmService = trmService ?? new TrmService();
    }

    public void RegisterDeposit(Guid associateId, decimal amount)
    {
        if (amount <= 0)
        {
            throw new ArgumentException("El monto a consignar debe ser mayor a cero.");
        }

        var associate = _associateRepository.GetById(associateId);
        if (associate == null)
        {
            throw new KeyNotFoundException("Asociado no encontrado.");
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
            throw new ArgumentException("El monto a retirar debe ser mayor a cero.");
        }

        var associate = _associateRepository.GetById(associateId);
        if (associate == null)
        {
            throw new KeyNotFoundException("Asociado no encontrado.");
        }

        // Business Rule: $8,000 cash handling fee applies if withdrawal amount exceeds $1,000,000
        decimal commission = amount > 1000000 ? 8000 : 0;
        decimal totalDeduction = amount + commission;

        // Business Rule: Withdrawal cannot result in a negative balance (including commission fee)
        decimal currentBalance = associate.GetBalance();
        if (currentBalance < totalDeduction)
        {
            throw new InvalidOperationException("Fondos insuficientes. La operación dejaría el saldo en negativo.");
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

    public async Task<decimal?> GetBalanceInUsdAsync(Guid associateId)
    {
        var associate = _associateRepository.GetById(associateId);
        if (associate == null)
        {
            throw new KeyNotFoundException("Asociado no encontrado.");
        }

        var balanceInCop = associate.GetBalance();
        if (balanceInCop == 0)
        {
            return 0;
        }

        // Asynchronously fetch current live TRM rate
        var trmData = await _trmService.GetCurrentTrmAsync();
        if (trmData == null || trmData.NumericValue == 0)
        {
            return null;
        }

        // Return converted USD amount by dividing COP balance by TRM rate
        return balanceInCop / trmData.NumericValue;
    }

    public SummaryReportDto GetCooperativeSummary()
    {
        var associates = _associateRepository.GetAll();
        var totalAssociates = associates.Count;
        var totalBalance = associates.Sum(a => a.GetBalance());

        // Prevent division by zero if cooperative has no associates registered
        var averageBalance = totalAssociates > 0 ? totalBalance / totalAssociates : 0;

        return new SummaryReportDto
        {
            TotalAssociates = totalAssociates,
            TotalBalance = totalBalance,
            AverageBalance = averageBalance
        };
    }

    public List<Associate> GetTopAssociates()
    {
        // Retrieve top 5 associates with the highest balance
        return _associateRepository.GetAll()
            .OrderByDescending(a => a.GetBalance())
            .Take(5)
            .ToList();
    }

    public List<Associate> GetInactiveAssociates()
    {
        // Filter associates with zero balance and no registered transactions
        return _associateRepository.GetAll()
            .Where(a => a.GetBalance() == 0 && (a.Transactions == null || a.Transactions.Count == 0))
            .ToList();
    }

    public PeriodSummaryDto GetPeriodSummary(DateTime startDate, DateTime endDate)
    {
        // Filter transactions within the specified date range
        var transactions = _transactionRepository.GetAll()
            .Where(t => t.Date >= startDate && t.Date <= endDate)
            .ToList();

        var totalDeposited = transactions
            .Where(t => t.Type == TransactionType.Deposit)
            .Sum(t => t.Amount);

        var totalWithdrawn = transactions
            .Where(t => t.Type == TransactionType.Withdrawal)
            .Sum(t => t.Amount);

        return new PeriodSummaryDto
        {
            StartDate = startDate,
            EndDate = endDate,
            TotalDeposited = totalDeposited,
            TotalWithdrawn = totalWithdrawn
        };
    }

    public List<Transaction> GetLargestTransactions()
    {
        // Retrieve top 10 highest-amount transactions across cooperative
        return _transactionRepository.GetAll()
            .OrderByDescending(t => t.Amount)
            .Take(10)
            .ToList();
    }

    public List<AssociateMovementDto> GetAssociateMovementSummary()
    {
        // Group transactions by associate and compute total movements, deposits, and withdrawals
        return _transactionRepository.GetAll()
            .GroupBy(t => t.AssociateId)
            .Select(group =>
            {
                var associate = _associateRepository.GetById(group.Key);
                var totalDeposited = group.Where(t => t.Type == TransactionType.Deposit).Sum(t => t.Amount);
                var totalWithdrawn = group.Where(t => t.Type == TransactionType.Withdrawal).Sum(t => t.Amount);

                return new AssociateMovementDto
                {
                    AssociateName = associate?.Name ?? "Unknown Associate",
                    MovementCount = group.Count(),
                    TotalDeposited = totalDeposited,
                    TotalWithdrawn = totalWithdrawn,
                    CurrentBalance = associate?.GetBalance() ?? 0m
                };
            })
            .OrderByDescending(dto => dto.MovementCount)
            .ToList();
    }
}
