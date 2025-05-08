using System.Collections.Concurrent;
using System.Net.Sockets;

public static class ChatRoom
{
    private static readonly ConcurrentDictionary<TcpClient, string> _clients = new();

    public static void AddClient(TcpClient client, string username)
    {
        _clients.TryAdd(client, username);
    }

    public static bool RemoveClient(TcpClient client)
    {
        return _clients.TryRemove(client, out _);
    }

    public static IEnumerable<TcpClient> GetClients()
    {
        return _clients.Keys;
    }

    public static string GetUsername(TcpClient client)
    {
        return _clients.TryGetValue(client, out var username) ? username : "Unknown";
    }
}