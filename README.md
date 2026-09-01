# Cooperativa Financiera El Progreso - Console System

## Project Description
Financial management console application developed in .NET 10 to handle teller operations, associate records, transactions, and managerial reporting.

---

## Directory Structure

```text
-Sistema-de-Gestion-Para-La-Cooperativa-Financiera-El-Progreso-/
├── Cooperativa-El-Progreso.console/
│   ├── DTO/
│   │   ├── AssociateMovementDto.cs
│   │   ├── PeriodSummaryDto.cs
│   │   ├── SumaryReportDto.cs
│   │   └── TrmResponseDto.cs
│   ├── enums/
│   │   └── Enums.cs
│   ├── Models/
│   │   ├── Associate.cs
│   │   ├── Transaction.cs
│   │   └── User.cs
│   ├── Repositories/
│   │   ├── IAssociateRepository.cs
│   │   ├── AssociateRepository.cs
│   │   ├── ITransactionRepository.cs
│   │   └── TransactionRepository.cs
│   ├── Services/
│   │   ├── AssociateService.cs
│   │   ├── TransactionService.cs
│   │   ├── ITrmService.cs
│   │   └── TrmService.cs
│   ├── Ui/
│   │   └── ConsoleMenu.cs
│   ├── Program.cs
│   └── Cooperativa-El-Progreso.console.csproj
├── Cooperativa-El-Progreso.unix/
│   ├── UnitTest1.cs
│   └── Cooperativa-El-Progreso.unix.csproj
├── docs/
│   └── (UML diagrams and architectural documentation)
├── Cooperativa-El-Progreso.slnx
├── CHANGELOG.md
└── README.md
```

---

## Architecture
The application uses a layered architecture:

- **Models**: Domain entities (`Associate`, `Transaction`, `User`).
- **DTO**: Data transfer objects for reports and TRM API integration.
- **enums**: Enumerations for user roles and transaction types.
- **Repositories**: In-memory data access interfaces and implementations.
- **Services**: Business logic, transactions, LINQ reports, and TRM integration.
- **Ui**: Console user interface and menu routing.
- **docs**: Location for UML diagrams and project documentation.
- **Program.cs**: Dependency injection setup and entry point.

---

## Business Rules
1. **Initial Balance**: New associates are registered with a 0 COP balance.
2. **Unique Document**: Duplicate document numbers are not allowed.
3. **Withdrawal Fee**: Withdrawals above 1,000,000 COP incur an 8,000 COP fee.
4. **No Negative Balance**: Withdrawals that exceed available balance (amount + fee) are rejected.
5. **Deletion Restriction**: Associates with transaction history cannot be deleted.
6. **TRM API Resilience**: External TRM API failures are handled gracefully without application crashes.

---

## Management Reports (LINQ)
1. **General Summary**: Total associates, total balance, and average balance.
2. **Top 5 Associates**: Highest balances ordered descending.
3. **Inactive Associates**: Associates with 0 balance and no transactions.
4. **Top 10 Transactions**: Highest transaction amounts.
5. **Associate Activity**: Transaction count, deposits, withdrawals, and balance per associate.
6. **Period Summary**: Total deposits, withdrawals, and net movement by date range.

---

## How to Run

1. Run the console application:
```bash
dotnet run --project Cooperativa-El-Progreso.console/Cooperativa-El-Progreso.console.csproj
```

2. Run unit tests:
```bash
dotnet test
```
