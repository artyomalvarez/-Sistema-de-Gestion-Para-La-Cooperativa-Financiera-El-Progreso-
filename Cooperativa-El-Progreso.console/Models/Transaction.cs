namespace Cooperativa_El_Progreso.console.Models;

public class Transaction
{
    public Guid Id { get; set; }
    public TransactionType Type { get; set; }
    public decimal Amount { get; set; }
    public decimal Commission { get; set; }
    public DateTime Date { get; set; }
    
    public Guid AssociateId { get; set; }
    public Guid UserId { get; set; }

    public decimal GetTotalDeduction()
    {
        return Type == TransactionType.Withdrawal ? Amount + Commission : 0m;
    }
}
