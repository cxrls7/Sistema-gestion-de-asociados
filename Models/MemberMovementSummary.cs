namespace MemberManagementSystem.Models;

/// <summary>
/// Summary data used for management reports.
/// </summary>
public class MemberMovementSummary
{
    public int MemberId { get; set; }
    public string MemberName { get; set; } = string.Empty;
    public int MovementCount { get; set; }
    public decimal TotalDeposits { get; set; }
    public decimal TotalWithdrawals { get; set; }
    public decimal CurrentBalance { get; set; }
}
