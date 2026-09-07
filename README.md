# Cooperativa Financiera El Progreso - Sistema de Gestion

## Descripcion del Proyecto
Sistema integral de gestion financiera desarrollado en .NET 10 (C#) para la Cooperativa Financiera El Progreso. Administra operaciones de ventanilla (caja), asociados, historiales de transacciones, conversion multimoneda con TRM en tiempo real mediante API externa, y generacion de reportes gerenciales analiticos con LINQ.

El sistema cuenta con una aplicacion de consola interactiva, una Web API REST en ASP.NET Core, scripts de base de datos relacional (SQL) y documentacion arquitectonica en diagramas Draw.io (UML).

---

## Estructura del Directorio

```text
-Sistema-de-Gestion-Para-La-Cooperativa-Financiera-El-Progreso-/
├── Cooperativa-El-Progreso.console/          # Aplicacion interactiva de consola
│   ├── DTO/                                  # Objetos de transferencia de datos para reportes y TRM
│   │   ├── AssociateMovementDto.cs
│   │   ├── PeriodSummaryDto.cs
│   │   ├── SumaryReportDto.cs
│   │   └── TrmResponseDto.cs
│   ├── enums/                                # Enumeraciones (Role, TransactionType)
│   │   └── Enums.cs
│   ├── Models/                               # Modelos de dominio (Associate, Transaction, User)
│   │   ├── Associate.cs
│   │   ├── Transaction.cs
│   │   └── User.cs
│   ├── Repositories/                         # Capa de persistencia (Interfaces e implementaciones en memoria)
│   │   ├── IAssociateRepository.cs
│   │   ├── AssociateRepository.cs
│   │   ├── ITransactionRepository.cs
│   │   └── TransactionRepository.cs
│   ├── Services/                             # Logica de negocio, reportes LINQ y consumo HTTP TRM
│   │   ├── AssociateService.cs
│   │   ├── TransactionService.cs
│   │   ├── ITrmService.cs
│   │   └── TrmService.cs
│   ├── Ui/                                   # Interfaz de usuario por consola
│   │   └── ConsoleMenu.cs
│   ├── Program.cs                            # Punto de entrada y configuracion de dependencias
│   └── Cooperativa-El-Progreso.console.csproj
├── Cooperativa-El-Progreso.api/              # Web API REST en ASP.NET Core (.NET 10)
│   ├── Program.cs                            # Endpoints Minimal API y documentacion OpenAPI
│   ├── appsettings.json
│   └── Cooperativa-El-Progreso.api.csproj
├── Cooperativa-El-Progreso.unix/             # Proyecto de pruebas unitarias (NUnit)
│   ├── UnitTest1.cs
│   └── Cooperativa-El-Progreso.unix.csproj
├── database/                                 # Scripts SQL de base de datos relacional
│   ├── schema.sql                            # DDL: Tablas, restricciones, indices, vistas y funciones
│   └── seed.sql                              # DML: Datos iniciales de prueba
├── docs/                                     # Diagramas UML y arquitectura en formato Draw.io
│   ├── Cooperativa-El-Progreso.drawio        # Archivo maestro Draw.io (7 paginas en pestañas)
│   ├── Diagrama-Casos-De-Uso.drawio          # Casos de uso (Cajero, Gerente, API TRM)
│   ├── Diagrama-Secuencia.drawio             # Flujo de retiro con comision y validacion de saldo
│   ├── Diagrama-Actividad.drawio             # Diagrama de actividad y bifurcaciones de negocio
│   ├── Diagrama-Modelos.drawio               # Diagrama de clases de modelos y DTOs
│   ├── Diagrama-Repositorios.drawio          # Diagrama de clases de repositorios
│   ├── Diagrama-Servicios.drawio             # Diagrama de clases de servicios de negocio
│   ├── Diagrama-UI.drawio                    # Diagrama de clases de consola UI
│   └── *.drawio.png                          # Imagenes PNG con metadatos Draw.io integrados
├── Cooperativa-El-Progreso.slnx              # Solucion unificada para JetBrains Rider y dotnet CLI
├── CHANGELOG.md                              # Historial de cambios
└── README.md                                 # Documentacion tecnica del sistema
```

---

## Arquitectura del Sistema
La solucion aplica una arquitectura en capas desacoplada:

- **Modelos de Dominio (`Models`)**: Entidades centrales (`Associate`, `Transaction`, `User`). La entidad `Associate` calcula su balance dinamicamente mediante el metodo `GetBalance()`.
- **DTOs (`DTO`)**: Estructuras de datos para transporte de reportes consolidados y respuesta del endpoint de la TRM.
- **Enumeraciones (`enums`)**: Roles de usuario (`Cashier`, `Manager`) y tipos de movimiento (`Deposit`, `Withdrawal`).
- **Repositorios (`Repositories`)**: Abstraccion de acceso a datos mediante interfaces desacopladas de su implementacion.
- **Servicios (`Services`)**: Orquestacion de reglas de negocio, validaciones, consultas analiticas con LINQ y consumo asincrono tolerante a fallos del API de la TRM (`datos.gov.co`).
- **Presentacion (`Ui` y `api`)**:
  - `Cooperativa-El-Progreso.console`: Interfaz interactiva de consola con formateo regional colombiano (`es-CO`).
  - `Cooperativa-El-Progreso.api`: API REST con endpoints JSON y documentacion OpenAPI.

---

## Reglas de Negocio
1. **Saldo Inicial**: Todo nuevo asociado inicia con un saldo de $0 COP.
2. **Documento Unico**: No se permiten documentos de identidad duplicados.
3. **Tarifa por Manejo en Retiros**: Si el monto de un retiro supera $1.000.000 COP, se aplica automaticamente una comision de $8.000 COP.
4. **Proteccion de Sobregiro**: Se rechaza cualquier retiro si el saldo disponible es menor a la deduccion total (monto solicitado + comision).
5. **Restriccion de Eliminacion**: No se permite eliminar un asociado que posea historial de transacciones financieras registradas.
6. **Resiliencia de Red (TRM)**: Si la API publica de la TRM no esta disponible o falla la conexion, la aplicacion captura el error sin colapsar y retorna aviso correspondiente.

---

## Base de Datos (Scripts SQL)
En el directorio `database/` se encuentran los scripts listos para ejecutar en PostgreSQL, MySQL o SQL Server:

- **`database/schema.sql`**:
  - Tablas: `roles`, `users`, `associates`, `transaction_types`, `transactions`.
  - Integridad referencial con `ON DELETE RESTRICT` para impedir la eliminacion de asociados con movimientos.
  - Indices de rendimiento para busquedas por documento, nombre, fecha y monto.
  - Vistas analiticas: `vw_associate_balances`, `vw_cooperative_summary`, `vw_inactive_associates`.
  - Funcion / Procedimiento almacenado: `fn_register_withdrawal()` para validacion y ejecucion atomica de retiros con cobro de comisiones.
- **`database/seed.sql`**:
  - Insercion de datos semilla para roles, usuarios de ventanilla y administracion, asociados de prueba y transacciones de ejemplo.

### Ejecucion de los Scripts SQL (PostgreSQL):
```bash
psql -U tu_usuario -d tu_base_datos -f database/schema.sql
psql -U tu_usuario -d tu_base_datos -f database/seed.sql
```

---

## Diagramas de Arquitectura (Draw.io)
Los diagramas se encuentran en el directorio `docs/` y se pueden abrir directamente en [draw.io / diagrams.net](https://app.diagrams.net/), en JetBrains Rider o en VS Code:

- **`docs/Cooperativa-El-Progreso.drawio`**: Archivo maestro con 7 paginas en pestañas:
  1. `Casos de Uso`: Relaciones entre Cajero, Gerente, API TRM y casos de uso organizados por modulos.
  2. `Diagrama de Secuencia`: Interaccion completa del flujo de retiro con validacion de saldo y comisiones.
  3. `Diagrama de Actividad`: Flujo procedimental y bifurcaciones de negocio al procesar un retiro.
  4. `Modelos y DTOs`: Clases de entidades de dominio y objetos de transferencia.
  5. `Repositorios`: Interfaces e implementaciones del patron repositorio.
  6. `Servicios`: Logica de servicios, metodos transaccionales y cliente TRM.
  7. `Interfaz UI`: Estructura de la consola y flujo del menu principal.
- Archivos `.drawio` individuales por cada diagrama para edicion o exportacion independiente.

---

## Informes Gerenciales (LINQ)
1. **Resumen General**: Total de asociados, saldo total en custodia y saldo promedio.
2. **Top 5 Asociados**: Asociados con mayores balances ordenados descendentemente.
3. **Asociados Inactivos**: Cuentas con saldo $0 COP y sin transacciones historicas.
4. **Top 10 Transacciones**: Transacciones individuales de mayor cuantia.
5. **Actividad por Asociado**: Conteo de movimientos, total consignado, total retirado y saldo por cliente.
6. **Balance por Periodo**: Total de depositos, retiros y flujo neto para un rango de fechas o mes actual.

---

## Como Ejecutar

### 1. Aplicacion de Consola (Modo Desarrollo)
```bash
dotnet run --project Cooperativa-El-Progreso.console/Cooperativa-El-Progreso.console.csproj
```

### 2. Generar Ejecutable Autonomo (Standalone Single-File)
Permite generar un binario independiente que se ejecuta sin requerir el SDK de .NET instalado:

- **Para Linux**:
```bash
dotnet publish Cooperativa-El-Progreso.console/Cooperativa-El-Progreso.console.csproj -c Release -r linux-x64 --self-contained -p:PublishSingleFile=true -o ./publish/linux
```
Ejecucion:
```bash
./publish/linux/Cooperativa-El-Progreso.console
```

- **Para Windows (.exe)**:
```bash
dotnet publish Cooperativa-El-Progreso.console/Cooperativa-El-Progreso.console.csproj -c Release -r win-x64 --self-contained -p:PublishSingleFile=true -o ./publish/windows
```
Ejecucion:
Doble clic sobre `Cooperativa-El-Progreso.console.exe` dentro de `./publish/windows/`.

### 3. Web API REST (.NET 10)
Iniciar el servidor de la API:
```bash
dotnet run --project Cooperativa-El-Progreso.api/Cooperativa-El-Progreso.api.csproj
```

La especificacion OpenAPI estara disponible en:
`http://localhost:5000/openapi/v1.json`

#### Catalogo de Endpoints Principales:

| Metodo | Endpoint | Descripcion |
| :--- | :--- | :--- |
| `GET` | `/api/associates` | Listar todos los asociados con su saldo |
| `GET` | `/api/associates/{documentNumber}` | Buscar asociado por documento |
| `GET` | `/api/associates/search?name={texto}` | Buscar asociados por coincidencia de nombre |
| `POST` | `/api/associates` | Registrar nuevo asociado |
| `PUT` | `/api/associates/{id}` | Actualizar datos de contacto |
| `DELETE` | `/api/associates/{id}` | Eliminar asociado (restringido si tiene historial) |
| `POST` | `/api/transactions/deposit` | Registrar consignacion de fondos |
| `POST` | `/api/transactions/withdrawal` | Registrar retiro (aplica comision y valida saldo) |
| `GET` | `/api/transactions/balance/{associateId}` | Consultar saldo en COP |
| `GET` | `/api/transactions/balance-usd/{associateId}` | Consultar saldo en USD segun TRM en tiempo real |
| `GET` | `/api/transactions/history/{associateId}` | Consultar extracto historico de transacciones |
| `GET` | `/api/reports/summary` | Informe 1: Resumen general de la cooperativa |
| `GET` | `/api/reports/top-associates` | Informe 2: Top 5 asociados con mayor saldo |
| `GET` | `/api/reports/inactive-associates` | Informe 3: Asociados inactivos |
| `GET` | `/api/reports/largest-transactions` | Informe 4: Top 10 transacciones mas grandes |
| `GET` | `/api/reports/associate-movements` | Informe 5: Resumen de actividad por asociado |
| `GET` | `/api/reports/period-summary` | Informe 6: Balance y flujo del periodo |

### 4. Pruebas Unitarias
```bash
dotnet test
```
