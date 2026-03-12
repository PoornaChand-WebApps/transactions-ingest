using CodingExerciseTransactions.Models;
using CodingExerciseTransactions.Utils;
using CodingExerciseTransactions.Data;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;

namespace CodingExerciseTransactions.Services;

public class TransactionService
{
    private readonly AppDbContext _dbContext;
    private readonly Messages _messages;

    public TransactionService(AppDbContext dbContext, Messages messages)
    {
        _dbContext = dbContext;
        _messages = messages;

        // _dbContext.Database.EnsureCreated();
    }

    public bool ProcessSnapshot(string jsonFilePath)
    {
        using var transaction = _dbContext.Database.BeginTransaction();

        try
        {
            var snapshot = LoadSnapshot(jsonFilePath);
            if (snapshot == null || snapshot.Count == 0)
            {
                _messages.LogError($"Feed file not found or empty: {jsonFilePath}");
                return false;
            }
            var cutoff = DateTime.UtcNow.AddDays(-1);

            foreach (var item in snapshot)
            {
                int transactionId = int.Parse(item.TransactionId.Replace("T-", ""));

                var existingTransaction = _dbContext.Transactions
                    .FirstOrDefault(t => t.TransactionId == transactionId);

                if (existingTransaction != null)
                {
                    if (existingTransaction.IsFinalized)
                        continue;

                    DetectAndRecordChanges(existingTransaction, item);

                    existingTransaction.LocationCode = item.LocationCode;
                    existingTransaction.ProductName = item.ProductName;
                    existingTransaction.Amount = item.Amount;
                    existingTransaction.TransactionTime = item.Timestamp;
                    existingTransaction.UpdatedAt = DateTime.UtcNow;
                }
                else
                {
                    var newTransaction = new TransactionModel
                    {
                        TransactionId = transactionId,
                        CardNumberHash = HashCard(item.CardNumber),
                        CardLast4 = item.CardNumber[^4..],
                        LocationCode = item.LocationCode,
                        ProductName = item.ProductName,
                        Amount = item.Amount,
                        TransactionTime = item.Timestamp,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    _dbContext.Transactions.Add(newTransaction);

                    _messages.LogInformation($"Inserted Transaction {transactionId}");
                }
            }

            _dbContext.SaveChanges();

            HandleRevocations(snapshot, cutoff);
            HandleFinalization(cutoff);

            _dbContext.SaveChanges();

            transaction.Commit();

            _messages.LogInformation("Snapshot processing completed successfully.");
            return true;
        }
        catch (Exception ex)
        {
            transaction.Rollback();
            _messages.LogError($"Processing failed: {ex.Message}");
            throw;
        }
    }

    private List<TransactionsDTO> LoadSnapshot(string jsonFilePath)
    {
        if (!File.Exists(jsonFilePath))
        {
            _messages.LogError($"Feed file not found: {jsonFilePath}");
            return new List<TransactionsDTO>();
        }

        var json = File.ReadAllText(jsonFilePath);

        var list = JsonSerializer.Deserialize<List<TransactionsDTO>>(json,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

        return list ?? new List<TransactionsDTO>();
    }

    private void DetectAndRecordChanges(TransactionModel existing, TransactionsDTO updated)
    {
        if (existing.Amount != updated.Amount)
        {
            AddAudit(existing.TransactionId, "Amount",
                existing.Amount.ToString(),
                updated.Amount.ToString(),
                "Updated");
        }

        if (existing.ProductName != updated.ProductName)
        {
            AddAudit(existing.TransactionId, "ProductName",
                existing.ProductName,
                updated.ProductName,
                "Updated");
        }

        if (existing.LocationCode != updated.LocationCode)
        {
            AddAudit(existing.TransactionId, "LocationCode",
                existing.LocationCode,
                updated.LocationCode,
                "Updated");
        }
    }

    private void HandleRevocations(List<TransactionsDTO> snapshot, DateTime cutoff)
    {
        var recentTransactions = _dbContext.Transactions
            .Where(t => t.TransactionTime >= cutoff && !t.IsRevoked)
            .ToList();

        foreach (var t in recentTransactions)
        {
            bool existsInSnapshot = snapshot.Any(s =>
                int.Parse(s.TransactionId.Replace("T-", "")) == t.TransactionId);

            if (!existsInSnapshot)
            {
                t.IsRevoked = true;

                AddAudit(t.TransactionId, "IsRevoked",
                    "false",
                    "true",
                    "Revoked");

                _messages.LogWarning($"Transaction {t.TransactionId} revoked.");
            }
        }
    }

    private void HandleFinalization(DateTime cutoff)
    {
        var oldTransactions = _dbContext.Transactions
            .Where(t => t.TransactionTime < cutoff && !t.IsFinalized)
            .ToList();

        foreach (var t in oldTransactions)
        {
            t.IsFinalized = true;

            AddAudit(t.TransactionId, "IsFinalized",
                "false",
                "true",
                "Finalized");
        }
    }

    private void AddAudit(int transactionId, string field, string oldValue, string newValue, string type)
    {
        _dbContext.TransactionAuditTrail.Add(new TransactionAuditTrailModel
        {
            TransactionId = transactionId,
            FieldName = field,
            OldValue = oldValue,
            NewValue = newValue,
            ChangeType = type,
            ChangedAt = DateTime.UtcNow
        });
    }

    private string HashCard(string cardNumber)
    {
        using var sha = System.Security.Cryptography.SHA256.Create();
        var bytes = sha.ComputeHash(System.Text.Encoding.UTF8.GetBytes(cardNumber));
        return Convert.ToBase64String(bytes);
    }
}