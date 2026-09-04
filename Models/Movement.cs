namespace MemberManagementSystem.Models;

/// <summary>
/// Represents a deposit or withdrawal made by a member.
/// Each movement has an amount, a date, and the type of operation performed.
/// The total account balance is calculated from these records.
/// </summary>
public class Movement
{
    public int Id { get; set; }
    public string TransactionId { get; set; } = string.Empty;
    public int MemberId { get; set; }
    public MovementType Type { get; set; }
    public decimal Amount { get; set; }
    public decimal WithdrawalCommission { get; set; }
    public string PerformedBy { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
