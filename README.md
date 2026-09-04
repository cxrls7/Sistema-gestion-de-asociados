# 🏦 Cooperative Member Management System

A console-based cooperative management application developed in C# with a layered architecture. This project simulates a small financial cooperative where members can be registered, updated, searched, and managed through deposits, withdrawals, reports, and balance consultation.

## ✨ Project Description

This system is designed to support the basic operational flow of a cooperative or member-based savings organization. The application allows an operator to manage members and keep track of their movements in a simple and structured way.

The project focuses on:

- member registration and management
- cash movements (deposits and withdrawals)
- validation rules for financial operations
- balance queries in Colombian pesos (COP)
- secondary conversion to US dollars using the current TRM
- managerial reporting and movement analysis
- a terminal-based interface that simulates a cashier workflow

## 🧱 Architecture

The solution follows a layered architecture, separating responsibilities by folder and component type:

- Models: core business entities such as Member, Movement, TrmRate, and enums
- Interfaces: contracts for services and repositories
- Repositories: MySQL data access and persistence logic
- Services: validation, business rules, and calculation logic
- Dependencies: integration with the official TRM API
- UI: console menus and interaction flow
- Program.cs: application entry point

This separation keeps the business logic decoupled from data access and presentation, which makes the project easier to understand and extend.

## 🏗️ Technical Decisions Taken

Several design choices were made to keep the project clear and aligned with the business context:

- MySQL is used to persist members and movements between application executions.
- The business logic was centralized in the service layer to enforce rules consistently.
- Validation is applied at the moment the user enters the data, so the system can correct the specific field immediately.
- Colombian pesos (COP) were defined as the main currency of the system because the cooperative context is local and the business rules are based on the Colombian market.
- USD conversion is treated as a secondary reference based on the TRM, not as the primary transaction currency.
- The TRM is consumed through an external official data source to simulate a real financial integration.
- The UI is console-based, prioritizing clarity and educational value over graphical interfaces.

## 🧩 Main Features

- member registration
- document-based and name-based search
- member update
- member deletion with business rules
- cash deposit registration
- cash withdrawal registration
- withdrawal commission handling
- balance consultation in COP
- conversion of balances to USD using TRM
- movement history by member
- management reports and summaries
- duplicate validation for document number and phone

## 🛠️ Technologies Used

- C#
- .NET 10
- Console application
- LINQ
- HTTP client
- JSON deserialization
- MySQL
- Layered software architecture

## 📁 Project Structure

- Program.cs
- Models/
- Interfaces/
- Repositories/
- Services/
- Dependencies/
- UI/
- README.md

## ▶️ Execution Instructions

From the project folder, run the following command:

```bash
dotnet run
```

If you are using Visual Studio Code:

1. Open the project folder.
2. Open the terminal.
3. Run:

```bash
dotnet run
```

If you want to build the project without running it:

```bash
dotnet build
```

## 🗄️ MySQL Configuration

1. Create the database in MySQL:

```sql
CREATE DATABASE cooperative_management CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
```

2. Configure the connection string through the `MYSQL_CONNECTION_STRING` environment variable:

```bash
export MYSQL_CONNECTION_STRING="Server=localhost;Port=3306;Database=cooperative_management;User ID=app_user;Password=su_clave;"
```

On Windows PowerShell:

```powershell
$env:MYSQL_CONNECTION_STRING = "Server=localhost;Port=3306;Database=cooperative_management;User ID=app_user;Password=su_clave;"
```

If the variable is not defined, the application will try to connect to `localhost:3306` using the `root` user with no password and the `cooperative_management` database. The tables are created automatically when the application starts.

## ✅ Business Rules Implemented

- valid document number required
- names and surnames cannot contain numeric values
- deposit amounts must be greater than zero
- withdrawal amounts must be greater than zero
- the member cannot be deleted if they have movements or a balance
- withdrawal is blocked if the available balance is insufficient
- large withdrawals may include a commission
- duplicate document numbers are not allowed
- duplicate phone numbers are not allowed
- balances are handled in COP and converted to USD as a secondary report value


## 👤 Author

Carlos Daniel Molina Ordoñez
