namespace Cooperativa_El_Progreso.console.Models;

public class PeriodSummaryDto
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal TotalDeposited { get; set; }
    public decimal TotalWithdrawn { get; set; }
    public decimal NetMovement => TotalDeposited - TotalWithdrawn;
}
