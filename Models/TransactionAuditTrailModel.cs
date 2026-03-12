public class TransactionAuditTrailModel
{
    public int Id { get; set; }

    public int TransactionId { get; set; }

    public string ChangeType { get; set; } = string.Empty;

    public string FieldName { get; set; } = string.Empty;

    public string OldValue { get; set; } = string.Empty;

    public string NewValue { get; set; } = string.Empty;

    public DateTime ChangedAt { get; set; } = DateTime.UtcNow;
}