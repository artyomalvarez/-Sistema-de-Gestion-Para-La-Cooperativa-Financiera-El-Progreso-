using Cooperativa_El_Progreso.console.Models;
using Cooperativa_El_Progreso.console.Repositories;
using Cooperativa_El_Progreso.console.Services;
using Cooperativa_El_Progreso.console.Ui;

// 1. Initialize Repositories (In-memory storage)
IAssociateRepository associateRepository = new AssociateRepository();
ITransactionRepository transactionRepository = new TransactionRepository();

// 2. Seed Initial Mock Data for Testing and Demonstration
SeedData(associateRepository, transactionRepository);

// 3. Initialize Services with Injected Dependencies
ITrmService trmService = new TrmService();
var associateService = new AssociateService(associateRepository, transactionRepository);
var transactionService = new TransactionService(transactionRepository, associateRepository, trmService);

// 4. Initialize and Run Console UI
var consoleMenu = new ConsoleMenu(associateService, transactionService, transactionRepository);
await consoleMenu.RunAsync();

static void SeedData(IAssociateRepository associateRepo, ITransactionRepository transactionRepo)
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

    // Seed Transactions
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
