using MySqlConnector;

namespace MemberManagementSystem.Data;

public static class MySqlDatabase
{
    private const string DefaultConnectionString =
        "Server=localhost;Port=3306;Database=cooperative_management;User ID=root;Password=;";

    public static string ConnectionString =>
        Environment.GetEnvironmentVariable("MYSQL_CONNECTION_STRING") ?? DefaultConnectionString;

    public static MySqlConnection CreateConnection()
    {
        return new MySqlConnection(ConnectionString);
    }

    public static void Initialize()
    {
        using var connection = CreateConnection();
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = """
            CREATE TABLE IF NOT EXISTS Members (
                Id INT NOT NULL AUTO_INCREMENT,
                DocumentNumber VARCHAR(50) NOT NULL,
                FirstName VARCHAR(100) NOT NULL,
                LastName VARCHAR(100) NOT NULL,
                Phone VARCHAR(50) NULL,
                Address VARCHAR(255) NOT NULL,
                RegistrationDate DATETIME NOT NULL,
                PRIMARY KEY (Id),
                UNIQUE KEY UX_Members_DocumentNumber (DocumentNumber),
                UNIQUE KEY UX_Members_Phone (Phone)
            );

            CREATE TABLE IF NOT EXISTS Movements (
                Id INT NOT NULL AUTO_INCREMENT,
                TransactionId VARCHAR(50) NOT NULL,
                MemberId INT NOT NULL,
                Type INT NOT NULL,
                Amount DECIMAL(18,2) NOT NULL,
                WithdrawalCommission DECIMAL(18,2) NOT NULL DEFAULT 0,
                PerformedBy VARCHAR(150) NOT NULL,
                CreatedAt DATETIME NOT NULL,
                PRIMARY KEY (Id),
                UNIQUE KEY UX_Movements_TransactionId (TransactionId),
                KEY IX_Movements_MemberId (MemberId),
                CONSTRAINT FK_Movements_Members FOREIGN KEY (MemberId)
                    REFERENCES Members (Id)
            );
            """;
        command.ExecuteNonQuery();
    }
}
