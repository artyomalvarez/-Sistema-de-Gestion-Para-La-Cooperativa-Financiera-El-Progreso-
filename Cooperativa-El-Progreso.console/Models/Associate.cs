namespace Cooperativa_El_Progreso.console.Models;

public class Associate
{
    public Guid Id { get; set; }
    public string DocumentNumber { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    
    public List<Transaction> Transactions { get; set; } = new();

    // El requerimiento dice: "El saldo es siempre el resultado de sus movimientos"[cite: 1].
    // ¡Aquí te toca poner tu lógica luego para sumar/restar la lista de Transactions!
    public decimal GetBalance()
    {
        throw new NotImplementedException("Aquí va tu lógica del saldo");
    }