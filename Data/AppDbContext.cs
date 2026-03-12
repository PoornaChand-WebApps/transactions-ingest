using Microsoft.EntityFrameworkCore;
using CodingExerciseTransactions.Models;

namespace CodingExerciseTransactions.Data;

public class AppDbContext : DbContext
{
    public DbSet<TransactionAuditTrailModel> TransactionAuditTrail => Set<TransactionAuditTrailModel>();
    public DbSet<TransactionModel> Transactions => Set<TransactionModel>();
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }
}