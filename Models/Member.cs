namespace MemberManagementSystem.Models;

/// <summary>
/// Represents a cooperative member.
/// This class stores the basic personal information of the associated person.
/// The balance is not stored here, because the balance is calculated from the movements.
/// </summary>
public class Member
{
    public int Id { get; set; }
    public string DocumentNumber { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public DateTime RegistrationDate { get; set; }

    public string FullName => $"{FirstName} {LastName}".Trim();
}
