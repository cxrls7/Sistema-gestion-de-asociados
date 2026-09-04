using MemberManagementSystem.Models;

namespace MemberManagementSystem.Interfaces;

/// <summary>
/// Contract for the official currency exchange rate provider.
/// </summary>
public interface ITrmService
{
    Task<TrmRate?> GetCurrentTrmAsync();
}
