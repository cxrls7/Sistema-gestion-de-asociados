using MemberManagementSystem.Data;
using MemberManagementSystem.Interfaces;
using MemberManagementSystem.Models;

namespace MemberManagementSystem.Repositories;

public class MemberRepository : IMemberRepository
{
    public void Add(Member member)
    {
        using var connection = MySqlDatabase.CreateConnection();
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = """
            INSERT INTO Members (DocumentNumber, FirstName, LastName, Phone, Address, RegistrationDate)
            VALUES (@documentNumber, @firstName, @lastName, @phone, @address, @registrationDate);
            SELECT LAST_INSERT_ID();
            """;
        AddMemberParameters(command, member);
        member.Id = Convert.ToInt32(command.ExecuteScalar());
    }

    public List<Member> GetAll()
    {
        using var connection = MySqlDatabase.CreateConnection();
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = SelectMembers + " ORDER BY FirstName, LastName;";
        return ReadMembers(command);
    }

    public Member? GetById(int id)
    {
        using var connection = MySqlDatabase.CreateConnection();
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = SelectMembers + " WHERE Id = @id;";
        command.Parameters.AddWithValue("@id", id);
        return ReadMembers(command).FirstOrDefault();
    }

    public Member? GetByDocumentNumber(string documentNumber)
    {
        using var connection = MySqlDatabase.CreateConnection();
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = SelectMembers + " WHERE LOWER(DocumentNumber) = LOWER(@documentNumber);";
        command.Parameters.AddWithValue("@documentNumber", documentNumber.Trim());
        return ReadMembers(command).FirstOrDefault();
    }

    public List<Member> SearchByName(string namePattern)
    {
        using var connection = MySqlDatabase.CreateConnection();
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = SelectMembers + """
             WHERE @namePattern = ''
                OR LOWER(FirstName) LIKE LOWER(@likePattern)
                OR LOWER(LastName) LIKE LOWER(@likePattern)
             ORDER BY FirstName, LastName;
            """;
        command.Parameters.AddWithValue("@namePattern", namePattern.Trim());
        command.Parameters.AddWithValue("@likePattern", $"%{namePattern.Trim()}%");
        return ReadMembers(command);
    }

    public void Update(Member member)
    {
        using var connection = MySqlDatabase.CreateConnection();
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = """
            UPDATE Members
            SET DocumentNumber = @documentNumber, FirstName = @firstName, LastName = @lastName,
                Phone = @phone, Address = @address
            WHERE Id = @id;
            """;
        AddMemberParameters(command, member);
        command.Parameters.AddWithValue("@id", member.Id);
        command.ExecuteNonQuery();
    }

    public void Delete(int id)
    {
        using var connection = MySqlDatabase.CreateConnection();
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM Members WHERE Id = @id;";
        command.Parameters.AddWithValue("@id", id);
        command.ExecuteNonQuery();
    }

    public bool ExistsByDocumentNumber(string documentNumber, int? excludeMemberId = null)
    {
        return Exists("DocumentNumber", documentNumber.Trim(), excludeMemberId);
    }

    public bool ExistsByPhone(string phone, int? excludeMemberId = null)
    {
        return Exists("Phone", phone.Trim(), excludeMemberId);
    }

    private static bool Exists(string column, string value, int? excludeMemberId)
    {
        using var connection = MySqlDatabase.CreateConnection();
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = $"SELECT EXISTS(SELECT 1 FROM Members WHERE LOWER({column}) = LOWER(@value)" +
            (excludeMemberId.HasValue ? " AND Id <> @excludeMemberId" : string.Empty) + ");";
        command.Parameters.AddWithValue("@value", value);
        if (excludeMemberId.HasValue)
        {
            command.Parameters.AddWithValue("@excludeMemberId", excludeMemberId.Value);
        }

        return Convert.ToBoolean(command.ExecuteScalar());
    }

    private const string SelectMembers = """
        SELECT Id, DocumentNumber, FirstName, LastName, Phone, Address, RegistrationDate
        FROM Members
        """;

    private static void AddMemberParameters(MySqlConnector.MySqlCommand command, Member member)
    {
        command.Parameters.AddWithValue("@documentNumber", member.DocumentNumber);
        command.Parameters.AddWithValue("@firstName", member.FirstName);
        command.Parameters.AddWithValue("@lastName", member.LastName);
        command.Parameters.AddWithValue("@phone", string.IsNullOrWhiteSpace(member.Phone) ? DBNull.Value : member.Phone);
        command.Parameters.AddWithValue("@address", member.Address);
        command.Parameters.AddWithValue("@registrationDate", member.RegistrationDate);
    }

    private static List<Member> ReadMembers(MySqlConnector.MySqlCommand command)
    {
        using var reader = command.ExecuteReader();
        var members = new List<Member>();
        while (reader.Read())
        {
            members.Add(new Member
            {
                Id = reader.GetInt32("Id"),
                DocumentNumber = reader.GetString("DocumentNumber"),
                FirstName = reader.GetString("FirstName"),
                LastName = reader.GetString("LastName"),
                Phone = reader.IsDBNull(reader.GetOrdinal("Phone")) ? string.Empty : reader.GetString("Phone"),
                Address = reader.GetString("Address"),
                RegistrationDate = reader.GetDateTime("RegistrationDate")
            });
        }

        return members;
    }
}
