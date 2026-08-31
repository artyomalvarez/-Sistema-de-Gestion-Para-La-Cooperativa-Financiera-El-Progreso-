namespace Cooperativa_El_Progreso.console.Models;

public class User
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public Role Role { get; set; }

    public void Login()
    {
        // To be implemented
    }
}