namespace Cooperativa_El_Progreso.console.Models;

public class Associate
{
    public Guid Id { get; set; }
    public string DocumentNumber { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    
    public List<Transaction> Transactions { get; set; } = new List<Transaction>();

    // El requerimiento dice: "El saldo es siempre el resultado de sus movimientos"[cite: 1].
    // ¡Aquí te toca poner tu lógica luego para sumar/restar la lista de Transactions!
    public decimal GetBalance()
    {
        decimal balance = 0;

        foreach (var transaction in Transactions)
        {
            if (transaction.Type == TransactionType.Deposit)
            {
                balance += transaction.Amount;
            }
            else if (transaction.Type == TransactionType.Withdrawal)
            {
                balance -= (transaction.Amount + transaction.Commission);
            }
        }

        return balance;
    }
}