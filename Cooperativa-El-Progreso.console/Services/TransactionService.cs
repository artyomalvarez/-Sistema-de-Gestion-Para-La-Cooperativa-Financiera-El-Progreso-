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

    /// <summary>
    /// Calculates the current balance of an associate converted to USD using the live TRM rate.
    /// </summary>
    /// <param name="associateId">The unique identifier of the associate.</param>
    /// <returns>
    /// The balance converted to USD, 0 if balance in COP is 0, or null if the TRM API fails or rate is unavailable.
    /// </returns>
    /// <exception cref="KeyNotFoundException">Thrown when the associate does not exist.</exception>
    public async Task<decimal?> GetBalanceInUsdAsync(Guid associateId)
    {
        // 1. Retrieve associate and validate existence
        var associate = _associateRepository.GetById(associateId);
        if (associate == null)
        {
            throw new KeyNotFoundException("Asociado no encontrado.");
        }

        // 2. Calculate balance in local currency (COP)
        var balanceInCop = associate.GetBalance();
        if (balanceInCop == 0)
        {
            return 0;
        }

        // 3. Asynchronously fetch current TRM data
        var trmData = await _trmService.GetCurrentTrmAsync();

        // 4. Business Rule: If TRM API fails or returns invalid value, return null so UI can alert the cashier
        if (trmData == null || trmData.NumericValue == 0)
        {
            return null;
        }

        // 5. Log TRM validity range as required
        Console.WriteLine($"[Info] TRM Vigente desde: {trmData.ValidityFrom.ToShortDateString()} hasta: {trmData.ValidityTo.ToShortDateString()}");

        // 6. Return converted USD balance
        return balanceInCop / trmData.NumericValue;
    }

    public SummaryReportDto GetCooperativeSummary()
    {
        var associates = _associateRepository.GetAll();

        // LINQ haciendo el trabajo pesado
        var totalAssociates = associates.Count;

        // Sumamos el saldo de cada asociado en una sola línea
        var totalBalance = associates.Sum(a => a.GetBalance());

        // Prevenimos la división por cero si la cooperativa está vacía
        var averageBalance = totalAssociates > 0 ? totalBalance / totalAssociates : 0;

        return new SummaryReportDto
        {
            TotalAssociates = totalAssociates,
            TotalBalance = totalBalance,
            AverageBalance = averageBalance
        };
    }

    /// <summary>
    /// Retrieves the top 10 associates with the highest account balance.
    /// </summary>
    /// <returns>List of the top 10 associates ordered by descending balance.</returns>
    public List<Associate> GetTopAssociates()
    {
        return _associateRepository.GetAll()
            .OrderByDescending(a => a.GetBalance())
            .Take(10)
            .ToList();
    }

    /// <summary>
    /// Retrieves all inactive associates: those whose current balance is 0 and have no registered transactions.
    /// </summary>
    /// <returns>List of inactive associates.</returns>
    public List<Associate> GetInactiveAssociates()
    {
        return _associateRepository.GetAll()
            .Where(a => a.GetBalance() == 0 && (a.Transactions == null || a.Transactions.Count == 0))
            .ToList();
    }

    /// <summary>
    /// Computes summary report of total deposits and total withdrawals within a given date range.
    /// </summary>
    /// <param name="startDate">The start date of the period.</param>
    /// <param name="endDate">The end date of the period.</param>
    /// <returns>PeriodSummaryDto containing aggregated deposited and withdrawn amounts.</returns>
    public PeriodSummaryDto GetPeriodSummary(DateTime startDate, DateTime endDate)
    {
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

    /// <summary>
    /// Retrieves the 5 largest transactions across the entire cooperative by amount, regardless of type.
    /// </summary>
    /// <returns>List of top 5 highest-amount transactions.</returns>
    public List<Transaction> GetLargestTransactions()
    {
        return _transactionRepository.GetAll()
            .OrderByDescending(t => t.Amount)
            .Take(5)
            .ToList();
    }

    /// <summary>
    /// Groups transactions by cashier (UserId) and calculates the total number of transactions and sum of amounts processed.
    /// </summary>
    /// <returns>List of CashierSummaryDto per cashier.</returns>
    public List<CashierSummaryDto> GetCashierSummary()
    {
        return _transactionRepository.GetAll()
            .GroupBy(t => t.UserId)
            .Select(group => new CashierSummaryDto
            {
                CashierId = group.Key,
                TransactionCount = group.Count(),
                TotalProcessedAmount = group.Sum(t => t.Amount)
            })
            .OrderByDescending(c => c.TotalProcessedAmount)
            .ToList();
    }
}
