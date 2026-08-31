namespace Cooperativa_El_Progreso.console.Models;

public class CashierSummaryDto
{
    public Guid CashierId { get; set; }
    public int TransactionCount { get; set; }
    public decimal TotalProcessedAmount { get; set; }
}
