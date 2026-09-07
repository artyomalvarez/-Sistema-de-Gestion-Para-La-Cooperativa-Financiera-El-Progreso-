using Cooperativa_El_Progreso.console.Models;
using Cooperativa_El_Progreso.console.Repositories;
using Cooperativa_El_Progreso.console.Services;

var builder = WebApplication.CreateBuilder(args);

// Register Repositories as Singletons (In-Memory state preservation)
builder.Services.AddSingleton<IAssociateRepository, AssociateRepository>();
builder.Services.AddSingleton<ITransactionRepository, TransactionRepository>();
builder.Services.AddSingleton<ITrmService, TrmService>();
builder.Services.AddSingleton<AssociateService>();
builder.Services.AddSingleton<TransactionService>();

// Configure OpenAPI
builder.Services.AddOpenApi();

var app = builder.Build();

// Seed initial mock data
using (var scope = app.Services.CreateScope())
{
    var aRepo = scope.ServiceProvider.GetRequiredService<IAssociateRepository>();
    var tRepo = scope.ServiceProvider.GetRequiredService<ITransactionRepository>();
    SeedInitialData(aRepo, tRepo);
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// ==========================================
// ENDPOINTS: ASOCIADOS
// ==========================================
var associatesGroup = app.MapGroup("/api/associates").WithTags("Asociados");

associatesGroup.MapGet("/", (AssociateService service) =>
{
    var list = service.GetAllAssociates();
    var result = list.Select(a => new
    {
        a.Id,
        a.DocumentNumber,
        a.Name,
        a.Phone,
        a.Address,
        a.CreatedAt,
        Balance = a.GetBalance()
    });
    return Results.Ok(result);
});

associatesGroup.MapGet("/{documentNumber}", (string documentNumber, AssociateService service) =>
{
    var a = service.GetByDocument(documentNumber);
    if (a == null) return Results.NotFound(new { message = $"No existe un asociado con el documento {documentNumber}." });
    return Results.Ok(new
    {
        a.Id,
        a.DocumentNumber,
        a.Name,
        a.Phone,
        a.Address,
        a.CreatedAt,
        Balance = a.GetBalance()
    });
});

associatesGroup.MapGet("/search", (string name, AssociateService service) =>
{
    var results = service.SearchByName(name);
    return Results.Ok(results.Select(a => new
    {
        a.Id,
        a.DocumentNumber,
        a.Name,
        a.Phone,
        Balance = a.GetBalance()
    }));
});

associatesGroup.MapPost("/", (CreateAssociateRequest req, AssociateService service) =>
{
    try
    {
        var associate = new Associate
        {
            DocumentNumber = req.DocumentNumber,
            Name = req.Name,
            Phone = req.Phone,
            Address = req.Address
        };
        service.RegisterAssociate(associate);
        return Results.Created($"/api/associates/{associate.DocumentNumber}", associate);
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

associatesGroup.MapPut("/{id:guid}", (Guid id, UpdateAssociateRequest req, AssociateService service, IAssociateRepository repo) =>
{
    var existing = repo.GetById(id);
    if (existing == null) return Results.NotFound(new { message = "Asociado no encontrado." });

    if (!string.IsNullOrWhiteSpace(req.Name)) existing.Name = req.Name;
    if (!string.IsNullOrWhiteSpace(req.Phone)) existing.Phone = req.Phone;
    if (!string.IsNullOrWhiteSpace(req.Address)) existing.Address = req.Address;

    service.UpdateAssociate(existing);
    return Results.Ok(existing);
});

associatesGroup.MapDelete("/{id:guid}", (Guid id, AssociateService service) =>
{
    try
    {
        service.DeleteAssociate(id);
        return Results.Ok(new { message = "Asociado eliminado con éxito." });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

// ==========================================
// ENDPOINTS: TRANSACCIONES Y OPERACIONES
// ==========================================
var txGroup = app.MapGroup("/api/transactions").WithTags("Transacciones y Caja");

txGroup.MapPost("/deposit", (DepositRequest req, TransactionService service, IAssociateRepository aRepo) =>
{
    try
    {
        service.RegisterDeposit(req.AssociateId, req.Amount);
        var assoc = aRepo.GetById(req.AssociateId);
        return Results.Ok(new
        {
            message = "Depósito registrado con éxito.",
            associateId = req.AssociateId,
            depositedAmount = req.Amount,
            newBalance = assoc?.GetBalance() ?? 0
        });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

txGroup.MapPost("/withdrawal", (WithdrawalRequest req, TransactionService service, IAssociateRepository aRepo) =>
{
    try
    {
        service.RegisterWithdrawal(req.AssociateId, req.Amount);
        var assoc = aRepo.GetById(req.AssociateId);
        decimal commission = req.Amount > 1000000 ? 8000 : 0;
        return Results.Ok(new
        {
            message = "Retiro realizado con éxito.",
            associateId = req.AssociateId,
            withdrawnAmount = req.Amount,
            appliedCommission = commission,
            totalDeducted = req.Amount + commission,
            newBalance = assoc?.GetBalance() ?? 0
        });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

txGroup.MapGet("/balance/{associateId:guid}", (Guid associateId, IAssociateRepository aRepo) =>
{
    var assoc = aRepo.GetById(associateId);
    if (assoc == null) return Results.NotFound(new { message = "Asociado no encontrado." });
    return Results.Ok(new
    {
        associateId = assoc.Id,
        associateName = assoc.Name,
        balanceCop = assoc.GetBalance()
    });
});

txGroup.MapGet("/balance-usd/{associateId:guid}", async (Guid associateId, TransactionService service, IAssociateRepository aRepo) =>
{
    var assoc = aRepo.GetById(associateId);
    if (assoc == null) return Results.NotFound(new { message = "Asociado no encontrado." });

    var usdBalance = await service.GetBalanceInUsdAsync(associateId);
    return Results.Ok(new
    {
        associateId = assoc.Id,
        associateName = assoc.Name,
        balanceCop = assoc.GetBalance(),
        balanceUsd = usdBalance.HasValue ? Math.Round(usdBalance.Value, 2) : (decimal?)null,
        trmAvailable = usdBalance.HasValue
    });
});

txGroup.MapGet("/history/{associateId:guid}", (Guid associateId, ITransactionRepository tRepo, IAssociateRepository aRepo) =>
{
    var assoc = aRepo.GetById(associateId);
    if (assoc == null) return Results.NotFound(new { message = "Asociado no encontrado." });

    var history = tRepo.GetByAssociateId(associateId);
    return Results.Ok(new
    {
        associate = assoc.Name,
        balance = assoc.GetBalance(),
        transactions = history.OrderByDescending(t => t.Date)
    });
});

// ==========================================
// ENDPOINTS: REPORTES GERENCIALES (LINQ)
// ==========================================
var reportGroup = app.MapGroup("/api/reports").WithTags("Informes Gerenciales");

reportGroup.MapGet("/summary", (TransactionService service) =>
{
    return Results.Ok(service.GetCooperativeSummary());
});

reportGroup.MapGet("/top-associates", (TransactionService service) =>
{
    var top = service.GetTopAssociates().Select(a => new
    {
        a.Id,
        a.DocumentNumber,
        a.Name,
        Balance = a.GetBalance()
    });
    return Results.Ok(top);
});

reportGroup.MapGet("/inactive-associates", (TransactionService service) =>
{
    return Results.Ok(service.GetInactiveAssociates().Select(a => new
    {
        a.Id,
        a.DocumentNumber,
        a.Name
    }));
});

reportGroup.MapGet("/largest-transactions", (TransactionService service) =>
{
    return Results.Ok(service.GetLargestTransactions());
});

reportGroup.MapGet("/associate-movements", (TransactionService service) =>
{
    return Results.Ok(service.GetAssociateMovementSummary());
});

reportGroup.MapGet("/period-summary", (DateTime? startDate, DateTime? endDate, TransactionService service) =>
{
    var start = startDate ?? new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
    var end = endDate ?? DateTime.Now;
    return Results.Ok(service.GetPeriodSummary(start, end));
});

// Root welcome
app.MapGet("/", () => Results.Ok(new
{
    system = "Sistema de Gestion Para La Cooperativa Financiera El Progreso",
    version = "1.0.0",
    docs = "/openapi/v1.json",
    endpoints = new[]
    {
        "/api/associates",
        "/api/transactions",
        "/api/reports"
    }
}));

app.Run();

// Seed Helper
static void SeedInitialData(IAssociateRepository associateRepo, ITransactionRepository transactionRepo)
{
    var assoc1 = new Associate
    {
        Id = Guid.NewGuid(),
        DocumentNumber = "1001",
        Name = "Carlos Perez",
        Phone = "3001234567",
        Address = "Calle 10 # 20-30",
        CreatedAt = DateTime.Now.AddDays(-30)
    };

    var assoc2 = new Associate
    {
        Id = Guid.NewGuid(),
        DocumentNumber = "1002",
        Name = "Maria Gomez",
        Phone = "3109876543",
        Address = "Carrera 15 # 45-12",
        CreatedAt = DateTime.Now.AddDays(-20)
    };

    var assoc3 = new Associate
    {
        Id = Guid.NewGuid(),
        DocumentNumber = "1003",
        Name = "Juan Rodriguez",
        Phone = "3205551122",
        Address = "Avenida Siempre Viva 742",
        CreatedAt = DateTime.Now.AddDays(-10)
    };

    var assocInactive = new Associate
    {
        Id = Guid.NewGuid(),
        DocumentNumber = "1004",
        Name = "Ana Inactiva",
        Phone = "3150000000",
        Address = "Diagonal 5 # 1-2",
        CreatedAt = DateTime.Now.AddDays(-5)
    };

    associateRepo.Add(assoc1);
    associateRepo.Add(assoc2);
    associateRepo.Add(assoc3);
    associateRepo.Add(assocInactive);

    var tx1 = new Transaction
    {
        Id = Guid.NewGuid(),
        AssociateId = assoc1.Id,
        Type = TransactionType.Deposit,
        Amount = 2500000m,
        Commission = 0m,
        Date = DateTime.Now.AddDays(-25)
    };
    assoc1.Transactions.Add(tx1);
    transactionRepo.Add(tx1);

    var tx2 = new Transaction
    {
        Id = Guid.NewGuid(),
        AssociateId = assoc1.Id,
        Type = TransactionType.Withdrawal,
        Amount = 500000m,
        Commission = 0m,
        Date = DateTime.Now.AddDays(-15)
    };
    assoc1.Transactions.Add(tx2);
    transactionRepo.Add(tx2);

    var tx3 = new Transaction
    {
        Id = Guid.NewGuid(),
        AssociateId = assoc2.Id,
        Type = TransactionType.Deposit,
        Amount = 5000000m,
        Commission = 0m,
        Date = DateTime.Now.AddDays(-18)
    };
    assoc2.Transactions.Add(tx3);
    transactionRepo.Add(tx3);

    var tx4 = new Transaction
    {
        Id = Guid.NewGuid(),
        AssociateId = assoc2.Id,
        Type = TransactionType.Withdrawal,
        Amount = 1500000m,
        Commission = 8000m,
        Date = DateTime.Now.AddDays(-8)
    };
    assoc2.Transactions.Add(tx4);
    transactionRepo.Add(tx4);

    var tx5 = new Transaction
    {
        Id = Guid.NewGuid(),
        AssociateId = assoc3.Id,
        Type = TransactionType.Deposit,
        Amount = 800000m,
        Commission = 0m,
        Date = DateTime.Now.AddDays(-2)
    };
    assoc3.Transactions.Add(tx5);
    transactionRepo.Add(tx5);
}

// DTO records for requests
record CreateAssociateRequest(string DocumentNumber, string Name, string Phone, string Address);
record UpdateAssociateRequest(string? Name, string? Phone, string? Address);
record DepositRequest(Guid AssociateId, decimal Amount);
record WithdrawalRequest(Guid AssociateId, decimal Amount);
