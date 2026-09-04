using MemberManagementSystem.Models;

namespace MemberManagementSystem.Interfaces;

/// <summary>
/// Application contract for movement operations.
/// </summary>
public interface IMovementService
{
    OperationResult<Movement> RegisterDeposit(int memberId, decimal amount, string performedBy);
    OperationResult<Movement> RegisterWithdrawal(int memberId, decimal amount, string performedBy);
    List<Movement> GetMovementsByMember(int memberId);
    decimal GetBalance(int memberId);
    decimal GetBalanceInDollars(int memberId, decimal trmValue);
    List<Movement> GetAllMovements();
    List<Movement> GetMovementsByDateRange(DateTime startDate, DateTime endDate);
    List<Movement> GetTopTenLargestMovements();
    List<MemberMovementSummary> GetMemberMovementSummary();
}
