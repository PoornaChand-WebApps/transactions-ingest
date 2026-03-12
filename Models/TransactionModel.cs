using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
namespace CodingExerciseTransactions.Models;

[Index(nameof(TransactionId), IsUnique = true)]
[Index(nameof(TransactionTime))]
public class TransactionModel
{
    public int Id { get; set; }

    [Required]
    public int TransactionId { get; set; }

    [MaxLength(64)]
    public string CardNumberHash { get; set; } = string.Empty;

    [MaxLength(4)]
    public string CardLast4 { get; set; } = string.Empty;

    [MaxLength(20)]
    public string LocationCode { get; set; } = string.Empty;

    [MaxLength(20)]
    public string ProductName { get; set; } = string.Empty;

    [Column(TypeName = "decimal(18,2)")]
    public decimal Amount { get; set; }

    public DateTime TransactionTime { get; set; }

    public bool IsRevoked { get; set; }

    public bool IsFinalized { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}