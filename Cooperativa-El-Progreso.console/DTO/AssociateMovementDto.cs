namespace Cooperativa_El_Progreso.console.Models;

public class AssociateMovementDto
{
    public string AssociateName { get; set; } = string.Empty;
    public int MovementCount { get; set; }
    public decimal TotalDeposited { get; set; }
    public decimal TotalWithdrawn { get; set; }
    public decimal CurrentBalance { get; set; }
}
