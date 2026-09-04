using MemberManagementSystem.Models;

namespace MemberManagementSystem.Interfaces;

/// <summary>
/// Application contract for member operations.
/// </summary>
public interface IMemberService
{
    OperationResult<Member> RegisterMember(string documentNumber, string firstName, string lastName, string phone, string address);
    List<Member> GetAllMembers();
    Member? GetMemberByDocumentNumber(string documentNumber);
    List<Member> SearchMembersByName(string name);
    OperationResult<Member> UpdateMember(int id, string documentNumber, string firstName, string lastName, string phone, string address);
    OperationResult DeleteMember(int id);
    decimal GetBalance(int memberId);
    decimal GetBalanceInDollars(int memberId, decimal trmValue);
}
