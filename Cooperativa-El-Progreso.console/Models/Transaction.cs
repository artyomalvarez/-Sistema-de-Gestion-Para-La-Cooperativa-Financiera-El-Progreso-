namespace Cooperativa_El_Progreso.console.Models;

public class Transaction
{
    public Guid Id { get; set; }
    public TransactionType Type { get; set; }
    public decimal Amount { get; set; }
    public decimal Commission { get; set; }
    public DateTime Date { get; set; }
    
    public Guid AssociateId { get; set; }
    public Guid UserId { get; set; } // La persona que realizó el movimiento

    public decimal GetTotalDeduction()
    {
        // To be implemented
        return 0;
    }
}