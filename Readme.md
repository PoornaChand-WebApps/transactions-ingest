# Coding Exercise: Transaction Ingestion Console App

## Overview

This is a **.NET 10 Console Application** that ingests transaction data from a **mock JSON feed** and persists it into a **SQLite database** using **Entity Framework Core (code-first)**.  

The application:  
- Inserts new transactions.  
- Detects changes to existing transactions and maintains an **audit trail**.  
- Handles transaction **revocation** and **finalization** based on timestamps.  
- Logs operations using a lightweight `Messages` logger.

---

## Features

1. **Code-First EF Core**
   - Entities: `TransactionModel` and `TransactionAuditTrailModel`.  
   - DbContext: `AppDbContext` handles database access and migrations.  
   - SQLite database (`transactions.db`) is auto-created if missing.

2. **Configuration**
   - `appsettings.json` contains:
     ```json
     {
       "ConnectionStrings": {
         "Default": "Data Source=transactions.db"
       },
       "FeedSettings": {
         "MockFeedPath": "mock-data.json"
       }
     }
     ```
   - Configurable feed path and database connection string.

3. **Logging**
   - Console logging for **information**, **warnings**, and **errors**.  

4. **Transaction Processing**
   - Detects updated fields: `Amount`, `ProductName`, `LocationCode`.  
   - Maintains a **TransactionAuditTrail**.  
   - Marks old transactions as **finalized**.  
   - Revokes transactions no longer in the feed.  

5. **Mock JSON Feed**
   - Example `mock-data.json`:
     ```json
     [
       {
         "TransactionId": "T-1001",
         "CardNumber": "4111111111111111",
         "LocationCode": "LOC01",
         "ProductName": "Widget A",
         "Amount": 19.99,
         "Timestamp": "2026-03-12T12:00:00Z"
       },
       {
         "TransactionId": "T-1002",
         "CardNumber": "5555444433332222",
         "LocationCode": "LOC02",
         "ProductName": "Widget B",
         "Amount": 29.99,
         "Timestamp": "2026-03-12T13:00:00Z"
       }
     ]
     ```

---

## Project Structure

```
CodingExerciseTransactions/
│
├─ Data/
│   └─ AppDbContext.cs           # EF Core DbContext
│
├─ Models/
│   ├─ TransactionModel.cs
│   ├─ TransactionAuditTrailModel.cs
│   └─ TransactionsDTO.cs        # JSON feed DTO
│
├─ Services/
│   └─ TransactionService.cs     # Main ingestion & processing logic
│
├─ Utils/
│   └─ Messages.cs               # Console logging wrapper
│
├─ Program.cs                     # App entry point
├─ appsettings.json               # Config feed path & connection string
├─ mock-data.json                 # Sample feed for testing
├─ migrations/                    # EF Core migrations
└─ README.md

---

## Setup & Build

1. Clone the repository:  
   ```
   git clone <your-repo-url>
   cd CodingExerciseTransactions
   ```
2. Restore NuGet packages:
    ```
    dotnet restore
    ```
3.  Apply migrations to create SQLite database:
    ```
    dotnet ef migrations add InitialCreate
    dotnet ef database update
    ```

---

## Running Application

    Run the console application to process transactions from the JSON feed:
    ```
    dotnet run 
    ```
---
## Verify Data
    Run the console application to process transactions from the JSON feed:
    ```
    sqlite3 transactions.db
    .tables
    SELECT * FROM Transactions;
    SELECT * FROM TransactionAuditTrail;
    ```
---

## Assumptions & Notes

- Transactions are uniquely identified by `TransactionId` (numeric part).  
- Card numbers are **hashed with SHA-256**; last 4 digits are stored for reference.  
- Transactions older than 1 day are **finalized**, recent transactions not in the feed are **revoked**.  
- Audit trail tracks changes for `Amount`, `ProductName`, `LocationCode`, `IsFinalized`, and `IsRevoked`.  
- No external API required; local JSON feed is sufficient for testing.  

---

## Testing

- **Manual testing**: Modify `mock-data.json` and rerun `dotnet run`.  
- **Verify audit trail**: Changes in transactions create new records in `TransactionAuditTrail`.  
- Automated tests optional; can be implemented with **xUnit** or **NUnit**.

---

## Estimated & Actual Time

- **Estimated**: 4–6 hours  
- **Actual**: 5:30 hours  

**Notes**:  
- Initial setup and migrations required extra time due to SQLite behavior.  
- Revocation and audit trail logic implemented for correctness and traceability.  

---