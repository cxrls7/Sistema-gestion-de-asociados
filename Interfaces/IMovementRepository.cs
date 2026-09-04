using MemberManagementSystem.Models;

namespace MemberManagementSystem.Interfaces;

/// <summary>
/// Contract for movement persistence operations.
/// </summary>
public interface IMovementRepository
{
    void Add(Movement movement);
    List<Movement> GetByMemberId(int memberId);
    List<Movement> GetAll();
    List<Movement> GetByDateRange(DateTime startDate, DateTime endDate);
    List<Movement> GetTopTenLargest();
    List<MemberMovementSummary> GetMovementSummaryByMember();
}
