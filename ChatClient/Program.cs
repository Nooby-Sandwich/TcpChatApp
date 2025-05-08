using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

Console.InputEncoding = Encoding.UTF8;
Console.OutputEncoding = Encoding.UTF8;

TcpClient client = new TcpClient();

try
{
    await client.ConnectAsync("127.0.0.1", 5000);
    Console.WriteLine("Connected to chat server!");

    var stream = client.GetStream();

    // Set username
    Console.Write("Enter your username: ");
    string? username = Console.ReadLine();
    if (string.IsNullOrWhiteSpace(username))
        username = "Anonymous";

    byte[] loginData = Encoding.UTF8.GetBytes(username);
    await stream.WriteAsync(loginData, 0, loginData.Length);

    Console.WriteLine("Type your message or use /help for commands.");

    _ = ReadMessagesAsync(stream);

    while (true)
    {
        string? input = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(input)) continue;

        if (input.StartsWith('/'))
        {
            await HandleCommandAsync(stream, input, username);
        }
        else
        {
            byte[] data = Encoding.UTF8.GetBytes(input);
            await stream.WriteAsync(data, 0, data.Length);
        }
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Connection lost: {ex.Message}");
}
finally
{
    client.Close();
}

async Task HandleCommandAsync(NetworkStream stream, string command, string username)
{
    string[] parts = command.Split(' ', 2);
    string cmd = parts[0].ToLower();

    switch (cmd)
    {
        case "/help":
            Console.WriteLine("Available commands:");
            Console.WriteLine("/help - Show this help");
            Console.WriteLine("/pm <username> <message> - Send private message");
            Console.WriteLine("/listusers - List all connected users");
            break;

        case "/pm":
            if (parts.Length < 2)
            {
                Console.WriteLine("Usage: /pm <username> <message>");
                return;
            }

            string[] pmParts = parts[1].Split(' ', 2);
            if (pmParts.Length < 2)
            {
                Console.WriteLine("Usage: /pm <username> <message>");
                return;
            }

            string targetUser = pmParts[0];
            string message = pmParts[1];
            byte[] data = Encoding.UTF8.GetBytes($"/pm {targetUser} {message}");
            await stream.WriteAsync(data, 0, data.Length);
            break;

        case "/listusers":
            byte[] listData = Encoding.UTF8.GetBytes("/listusers");
            await stream.WriteAsync(listData, 0, listData.Length);
            break;

        default:
            Console.WriteLine($"Unknown command: {command}");
            Console.WriteLine("Type /help to see available commands.");
            break;
    }
}

async Task ReadMessagesAsync(NetworkStream stream)
{
    byte[] buffer = new byte[1024];

    while (true)
    {
        try
        {
            int read = await stream.ReadAsync(buffer, 0, buffer.Length);
            if (read == 0) break;

            string message = Encoding.UTF8.GetString(buffer, 0, read).Trim();

            if (message.StartsWith("/pm "))
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine(message.Substring(4).TrimStart());
                Console.ResetColor();
            }
            else if (message.StartsWith("/users "))
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("Connected users: " + message.Substring(7));
                Console.ResetColor();
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine(message);
                Console.ResetColor();
            }
        }
        catch
        {
            Console.WriteLine("Lost connection to server.");
            Environment.Exit(0);
        }
    }

    Console.WriteLine("Disconnected from server.");
    Environment.Exit(0);
}