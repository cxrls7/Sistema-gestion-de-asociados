using MemberManagementSystem.Models;

namespace MemberManagementSystem.Interfaces;

/// <summary>
/// Contract for member persistence and reading operations.
/// </summary>
public interface IMemberRepository
{
    void Add(Member member);
    List<Member> GetAll();
    Member? GetById(int id);
    Member? GetByDocumentNumber(string documentNumber);
    List<Member> SearchByName(string namePattern);
    void Update(Member member);
    void Delete(int id);
    bool ExistsByDocumentNumber(string documentNumber, int? excludeMemberId = null);
    bool ExistsByPhone(string phone, int? excludeMemberId = null);
}
