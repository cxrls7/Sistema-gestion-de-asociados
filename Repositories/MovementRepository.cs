using MemberManagementSystem.Data;
using MemberManagementSystem.Interfaces;
using MemberManagementSystem.Models;

namespace MemberManagementSystem.Repositories;

public class MovementRepository : IMovementRepository
{
    public void Add(Movement movement)
    {
        using var connection = MySqlDatabase.CreateConnection();
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = """
            INSERT INTO Movements
                (TransactionId, MemberId, Type, Amount, WithdrawalCommission, PerformedBy, CreatedAt)
            VALUES (@transactionId, @memberId, @type, @amount, @withdrawalCommission, @performedBy, @createdAt);
            SELECT LAST_INSERT_ID();
            """;
        AddMovementParameters(command, movement);
        movement.Id = Convert.ToInt32(command.ExecuteScalar());
    }

    public List<Movement> GetByMemberId(int memberId)
    {
        using var connection = MySqlDatabase.CreateConnection();
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = BaseSelect + " WHERE MemberId = @memberId ORDER BY CreatedAt;";
        command.Parameters.AddWithValue("@memberId", memberId);
        return ReadMovements(command);
    }

    public List<Movement> GetAll()
    {
        using var connection = MySqlDatabase.CreateConnection();
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = BaseSelect + " ORDER BY CreatedAt;";
        return ReadMovements(command);
    }

    public List<Movement> GetByDateRange(DateTime startDate, DateTime endDate)
    {
        using var connection = MySqlDatabase.CreateConnection();
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = BaseSelect + " WHERE CreatedAt >= @startDate AND CreatedAt <= @endDate ORDER BY CreatedAt;";
        command.Parameters.AddWithValue("@startDate", startDate);
        command.Parameters.AddWithValue("@endDate", endDate);
        return ReadMovements(command);
    }

    public List<Movement> GetTopTenLargest()
    {
        using var connection = MySqlDatabase.CreateConnection();
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = BaseSelect + " ORDER BY Amount DESC LIMIT 10;";
        return ReadMovements(command);
    }

    public List<MemberMovementSummary> GetMovementSummaryByMember()
    {
        using var connection = MySqlDatabase.CreateConnection();
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT MemberId, COUNT(*) AS MovementCount,
                   COALESCE(SUM(CASE WHEN Type = 1 THEN Amount ELSE 0 END), 0) AS TotalDeposits,
                   COALESCE(SUM(CASE WHEN Type = 2 THEN Amount + WithdrawalCommission ELSE 0 END), 0) AS TotalWithdrawals
            FROM Movements
            GROUP BY MemberId;
            """;
        using var reader = command.ExecuteReader();
        var summaries = new List<MemberMovementSummary>();
        while (reader.Read())
        {
            decimal deposits = reader.GetDecimal("TotalDeposits");
            decimal withdrawals = reader.GetDecimal("TotalWithdrawals");
            summaries.Add(new MemberMovementSummary
            {
                MemberId = reader.GetInt32("MemberId"),
                MovementCount = reader.GetInt32("MovementCount"),
                TotalDeposits = deposits,
                TotalWithdrawals = withdrawals,
                CurrentBalance = deposits - withdrawals
            });
        }

        return summaries;
    }

    private const string BaseSelect = """
        SELECT Id, TransactionId, MemberId, Type, Amount, WithdrawalCommission, PerformedBy, CreatedAt
        FROM Movements
        """;

    private static void AddMovementParameters(MySqlConnector.MySqlCommand command, Movement movement)
    {
        command.Parameters.AddWithValue("@transactionId", movement.TransactionId);
        command.Parameters.AddWithValue("@memberId", movement.MemberId);
        command.Parameters.AddWithValue("@type", (int)movement.Type);
        command.Parameters.AddWithValue("@amount", movement.Amount);
        command.Parameters.AddWithValue("@withdrawalCommission", movement.WithdrawalCommission);
        command.Parameters.AddWithValue("@performedBy", movement.PerformedBy);
        command.Parameters.AddWithValue("@createdAt", movement.CreatedAt);
    }

    private static List<Movement> ReadMovements(MySqlConnector.MySqlCommand command)
    {
        using var reader = command.ExecuteReader();
        var movements = new List<Movement>();
        while (reader.Read())
        {
            movements.Add(new Movement
            {
                Id = reader.GetInt32("Id"),
                TransactionId = reader.GetString("TransactionId"),
                MemberId = reader.GetInt32("MemberId"),
                Type = (MovementType)reader.GetInt32("Type"),
                Amount = reader.GetDecimal("Amount"),
                WithdrawalCommission = reader.GetDecimal("WithdrawalCommission"),
                PerformedBy = reader.GetString("PerformedBy"),
                CreatedAt = reader.GetDateTime("CreatedAt")
            });
        }

        return movements;
    }
}
