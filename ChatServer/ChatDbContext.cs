using Microsoft.Data.Sqlite;
using System;

public class ChatDbContext
{
    private readonly string _connectionString;

    public ChatDbContext(string connectionString)
    {
        _connectionString = connectionString;
        InitializeDatabase();
    }

    private void InitializeDatabase()
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = @"
            CREATE TABLE IF NOT EXISTS Users (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Username TEXT NOT NULL,
                ConnectedAt TEXT NOT NULL
            );
            CREATE TABLE IF NOT EXISTS Messages (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Username TEXT NOT NULL,
                Content TEXT NOT NULL,
                Timestamp TEXT NOT NULL
            );
            CREATE TABLE IF NOT EXISTS PrivateMessages (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Sender TEXT NOT NULL,
                Receiver TEXT NOT NULL,
                Content TEXT NOT NULL,
                Timestamp TEXT NOT NULL
            );
        ";
        command.ExecuteNonQuery();
    }

    public void LogUser(string username)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = "INSERT INTO Users (Username, ConnectedAt) VALUES (@username, @time)";
        command.Parameters.AddWithValue("@username", username);
        command.Parameters.AddWithValue("@time", DateTime.Now.ToString("o"));
        command.ExecuteNonQuery();
    }

    public void LogMessage(string username, string message)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = "INSERT INTO Messages (Username, Content, Timestamp) VALUES (@user, @msg, @time)";
        command.Parameters.AddWithValue("@user", username);
        command.Parameters.AddWithValue("@msg", message);
        command.Parameters.AddWithValue("@time", DateTime.Now.ToString("o"));
        command.ExecuteNonQuery();
    }

    public void LogPrivateMessage(string sender, string receiver, string message)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = "INSERT INTO PrivateMessages (Sender, Receiver, Content, Timestamp) VALUES (@sender, @receiver, @msg, @time)";
        command.Parameters.AddWithValue("@sender", sender);
        command.Parameters.AddWithValue("@receiver", receiver);
        command.Parameters.AddWithValue("@msg", message);
        command.Parameters.AddWithValue("@time", DateTime.Now.ToString("o"));
        command.ExecuteNonQuery();
    }
}