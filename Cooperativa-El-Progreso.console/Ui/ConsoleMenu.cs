using System.Globalization;
using Cooperativa_El_Progreso.console.Models;
using Cooperativa_El_Progreso.console.Repositories;
using Cooperativa_El_Progreso.console.Services;

namespace Cooperativa_El_Progreso.console.Ui;

public class ConsoleMenu
{
    private readonly AssociateService _associateService;
    private readonly TransactionService _transactionService;
    private readonly ITransactionRepository _transactionRepository;
    private readonly CultureInfo _culture = new("es-CO");

    public ConsoleMenu(
        AssociateService associateService,
        TransactionService transactionService,
        ITransactionRepository transactionRepository)
    {
        _associateService = associateService;
        _transactionService = transactionService;
        _transactionRepository = transactionRepository;
    }

    public async Task RunAsync()
    {
        bool running = true;

        while (running)
        {
            Console.Clear();
            DisplayMenu();
            Console.Write("\nSeleccione una opción: ");
            var option = Console.ReadLine()?.Trim();
            Console.WriteLine();

            // Centralized exception handling: Catches any validation or business exception to keep app running
            try
            {
                switch (option)
                {
                    case "1":
                        RegisterAssociateFlow();
                        break;
                    case "2":
                        ListAllAssociatesFlow();
                        break;
                    case "3":
                        SearchAssociateByDocumentFlow();
                        break;
                    case "4":
                        SearchAssociatesByNameFlow();
                        break;
                    case "5":
                        UpdateAssociateFlow();
                        break;
                    case "6":
                        DeleteAssociateFlow();
                        break;
                    case "7":
                        RegisterDepositFlow();
                        break;
                    case "8":
                        RegisterWithdrawalFlow();
                        break;
                    case "9":
                        ConsultBalanceFlow();
                        break;
                    case "10":
                        await ConsultBalanceUsdFlowAsync();
                        break;
                    case "11":
                        ViewAssociateTransactionsFlow();
                        break;
                    case "12":
                        ShowManagementReports();
                        break;
                    case "0":
                        running = false;
                        Console.WriteLine("¡Gracias por utilizar el sistema!");
                        break;
                    default:
                        Console.WriteLine("Opción no válida. Intente de nuevo.");
                        break;
                }
            }
            catch (Exception ex)
            {
                // Display captured error message gracefully to the console user
                Console.WriteLine($"[ERROR] {ex.Message}");
            }

            if (running)
            {
                Console.WriteLine("\nPresione cualquier tecla para continuar...");
                Console.ReadKey();
            }
        }
    }

    private void DisplayMenu()
    {
        Console.WriteLine("=================================================");
        Console.WriteLine("     COOPERATIVA FINANCIERA EL PROGRESO          ");
        Console.WriteLine("=================================================");
        Console.WriteLine("--- ASOCIADOS ---");
        Console.WriteLine("1. Registrar asociado");
        Console.WriteLine("2. Listar asociados");
        Console.WriteLine("3. Buscar por documento");
        Console.WriteLine("4. Buscar por nombre");
        Console.WriteLine("5. Actualizar asociado");
        Console.WriteLine("6. Eliminar asociado");
        Console.WriteLine("\n--- TRANSACCIONES ---");
        Console.WriteLine("7. Consignar (Depósito)");
        Console.WriteLine("8. Retirar");
        Console.WriteLine("9. Consultar saldo (COP)");
        Console.WriteLine("10. Consultar saldo en USD (TRM)");
        Console.WriteLine("11. Ver movimientos");
        Console.WriteLine("\n--- REPORTES ---");
        Console.WriteLine("12. Informes gerenciales");
        Console.WriteLine("-------------------------------------------------");
        Console.WriteLine("0. Salir");
        Console.WriteLine("=================================================");
    }

    private Associate GetAssociateByPrompt()
    {
        Console.Write("Ingrese el documento del asociado: ");
        var doc = Console.ReadLine()?.Trim() ?? string.Empty;

        var associate = _associateService.GetByDocument(doc);
        if (associate == null)
        {
            throw new Exception($"No existe un asociado con el documento '{doc}'.");
        }

        return associate;
    }

    private void RegisterAssociateFlow()
    {
        Console.WriteLine("=== REGISTRAR ASOCIADO ===");
        Console.Write("Documento: ");
        var doc = Console.ReadLine()?.Trim() ?? string.Empty;

        Console.Write("Nombre completo: ");
        var name = Console.ReadLine()?.Trim() ?? string.Empty;

        Console.Write("Teléfono: ");
        var phone = Console.ReadLine()?.Trim() ?? string.Empty;

        Console.Write("Dirección: ");
        var address = Console.ReadLine()?.Trim() ?? string.Empty;

        var associate = new Associate
        {
            DocumentNumber = doc,
            Name = name,
            Phone = phone,
            Address = address
        };

        _associateService.RegisterAssociate(associate);
        Console.WriteLine($"\nAsociado '{name}' registrado con éxito.");
    }

    private void ListAllAssociatesFlow()
    {
        Console.WriteLine("=== LISTADO DE ASOCIADOS ===");
        var list = _associateService.GetAllAssociates();

        if (list.Count == 0)
        {
            Console.WriteLine("No hay asociados registrados.");
            return;
        }

        foreach (var a in list)
        {
            Console.WriteLine($"- Doc: {a.DocumentNumber} | Nombre: {a.Name} | Tel: {a.Phone} | Saldo: {a.GetBalance().ToString("C2", _culture)}");
        }
    }

    private void SearchAssociateByDocumentFlow()
    {
        Console.WriteLine("=== BUSCAR ASOCIADO ===");
        var associate = GetAssociateByPrompt();

        Console.WriteLine($"\nDocumento: {associate.DocumentNumber}");
        Console.WriteLine($"Nombre:    {associate.Name}");
        Console.WriteLine($"Teléfono:  {associate.Phone}");
        Console.WriteLine($"Dirección: {associate.Address}");
        Console.WriteLine($"Saldo:     {associate.GetBalance().ToString("C2", _culture)}");
    }

    private void SearchAssociatesByNameFlow()
    {
        Console.WriteLine("=== BUSCAR POR NOMBRE ===");
        Console.Write("Ingrese texto a buscar: ");
        var query = Console.ReadLine()?.Trim() ?? string.Empty;

        var results = _associateService.SearchByName(query);
        if (results.Count == 0)
        {
            Console.WriteLine("No se encontraron coincidencias.");
            return;
        }

        foreach (var a in results)
        {
            Console.WriteLine($"- Doc: {a.DocumentNumber} | {a.Name} | Saldo: {a.GetBalance().ToString("C2", _culture)}");
        }
    }

    private void UpdateAssociateFlow()
    {
        Console.WriteLine("=== ACTUALIZAR ASOCIADO ===");
        var associate = GetAssociateByPrompt();

        Console.Write($"Nuevo nombre (actual: {associate.Name}): ");
        var name = Console.ReadLine()?.Trim();
        if (!string.IsNullOrEmpty(name)) associate.Name = name;

        Console.Write($"Nuevo teléfono (actual: {associate.Phone}): ");
        var phone = Console.ReadLine()?.Trim();
        if (!string.IsNullOrEmpty(phone)) associate.Phone = phone;

        Console.Write($"Nueva dirección (actual: {associate.Address}): ");
        var address = Console.ReadLine()?.Trim();
        if (!string.IsNullOrEmpty(address)) associate.Address = address;

        _associateService.UpdateAssociate(associate);
        Console.WriteLine("\nDatos actualizados correctamente.");
    }

    private void DeleteAssociateFlow()
    {
        Console.WriteLine("=== ELIMINAR ASOCIADO ===");
        var associate = GetAssociateByPrompt();

        Console.Write($"¿Seguro que desea eliminar a {associate.Name}? (S/N): ");
        var confirm = Console.ReadLine()?.Trim().ToUpperInvariant();

        if (confirm == "S" || confirm == "SI")
        {
            _associateService.DeleteAssociate(associate.Id);
            Console.WriteLine("Asociado eliminado correctamente.");
        }
        else
        {
            Console.WriteLine("Operación cancelada.");
        }
    }

    private void RegisterDepositFlow()
    {
        Console.WriteLine("=== REGISTRAR DEPÓSITO ===");
        var associate = GetAssociateByPrompt();

        Console.Write("Monto a consignar: ");
        if (!decimal.TryParse(Console.ReadLine(), out var amount))
        {
            throw new Exception("Monto inválido.");
        }

        _transactionService.RegisterDeposit(associate.Id, amount);
        Console.WriteLine($"Depósito realizado. Nuevo saldo: {associate.GetBalance().ToString("C2", _culture)}");
    }

    private void RegisterWithdrawalFlow()
    {
        Console.WriteLine("=== REGISTRAR RETIRO ===");
        var associate = GetAssociateByPrompt();

        Console.WriteLine($"Saldo actual: {associate.GetBalance().ToString("C2", _culture)}");
        Console.Write("Monto a retirar: ");
        if (!decimal.TryParse(Console.ReadLine(), out var amount))
        {
            throw new Exception("Monto inválido.");
        }

        _transactionService.RegisterWithdrawal(associate.Id, amount);
        Console.WriteLine($"Retiro realizado. Nuevo saldo: {associate.GetBalance().ToString("C2", _culture)}");
    }

    private void ConsultBalanceFlow()
    {
        Console.WriteLine("=== CONSULTA DE SALDO ===");
        var associate = GetAssociateByPrompt();

        Console.WriteLine($"Asociado: {associate.Name}");
        Console.WriteLine($"Saldo actual: {associate.GetBalance().ToString("C2", _culture)}");
    }

    private async Task ConsultBalanceUsdFlowAsync()
    {
        Console.WriteLine("=== CONSULTA DE SALDO EN USD ===");
        var associate = GetAssociateByPrompt();

        Console.WriteLine("Consultando TRM en tiempo real...");
        var usdBalance = await _transactionService.GetBalanceInUsdAsync(associate.Id);

        if (usdBalance == null)
        {
            Console.WriteLine("No se pudo obtener la TRM en este momento.");
            Console.WriteLine($"Saldo en COP: {associate.GetBalance().ToString("C2", _culture)}");
            return;
        }

        Console.WriteLine($"Asociado: {associate.Name}");
        Console.WriteLine($"Saldo en COP: {associate.GetBalance().ToString("C2", _culture)}");
        Console.WriteLine($"Saldo en USD: ${usdBalance.Value:F2} USD");
    }

    private void ViewAssociateTransactionsFlow()
    {
        Console.WriteLine("=== MOVIMIENTOS DEL ASOCIADO ===");
        var associate = GetAssociateByPrompt();

        var history = _transactionRepository.GetByAssociateId(associate.Id);
        if (history.Count == 0)
        {
            Console.WriteLine("El asociado no tiene movimientos registrados.");
            return;
        }

        foreach (var t in history)
        {
            var tipo = t.Type == TransactionType.Deposit ? "Depósito" : "Retiro";
            Console.WriteLine($"- {t.Date:yyyy-MM-dd HH:mm} | {tipo,-8} | Monto: {t.Amount.ToString("C2", _culture)} | Comisión: {t.Commission.ToString("C2", _culture)}");
        }

        Console.WriteLine($"Saldo total: {associate.GetBalance().ToString("C2", _culture)}");
    }

    private void ShowManagementReports()
    {
        Console.WriteLine("=== INFORMES GERENCIALES ===");

        // 1. General cooperative summary
        var summary = _transactionService.GetCooperativeSummary();
        Console.WriteLine($"1. Resumen General -> Total Asociados: {summary.TotalAssociates} | Saldo Total: {summary.TotalBalance.ToString("C2", _culture)} | Promedio: {summary.AverageBalance.ToString("C2", _culture)}");

        // 2. Top 5 associates by balance
        Console.WriteLine("\n2. Top 5 Asociados con mayor saldo:");
        var top = _transactionService.GetTopAssociates();
        foreach (var a in top)
        {
            Console.WriteLine($"   - {a.Name}: {a.GetBalance().ToString("C2", _culture)}");
        }

        // 3. Inactive associates
        Console.WriteLine("\n3. Asociados inactivos (Saldo 0 sin movimientos):");
        var inactives = _transactionService.GetInactiveAssociates();
        foreach (var a in inactives)
        {
            Console.WriteLine($"   - {a.Name} (Doc: {a.DocumentNumber})");
        }

        // 4. Top 10 largest transactions
        Console.WriteLine("\n4. Top 10 transacciones más grandes:");
        var largest = _transactionService.GetLargestTransactions();
        foreach (var t in largest)
        {
            var tipo = t.Type == TransactionType.Deposit ? "Depósito" : "Retiro";
            Console.WriteLine($"   - {tipo}: {t.Amount.ToString("C2", _culture)} el {t.Date:yyyy-MM-dd}");
        }

        // 5. Activity summary per associate
        Console.WriteLine("\n5. Resumen de actividad por asociado:");
        var movements = _transactionService.GetAssociateMovementSummary();
        foreach (var m in movements)
        {
            Console.WriteLine($"   - {m.AssociateName}: {m.MovementCount} movs | Depósitos: {m.TotalDeposited.ToString("C2", _culture)} | Retiros: {m.TotalWithdrawn.ToString("C2", _culture)}");
        }

        // 6. Current month period summary
        Console.WriteLine("\n6. Flujo del mes actual:");
        var startOfMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
        var period = _transactionService.GetPeriodSummary(startOfMonth, DateTime.Now);
        Console.WriteLine($"   - Total Consignado: {period.TotalDeposited.ToString("C2", _culture)} | Total Retirado: {period.TotalWithdrawn.ToString("C2", _culture)} | Neto: {period.NetMovement.ToString("C2", _culture)}");
    }
}
