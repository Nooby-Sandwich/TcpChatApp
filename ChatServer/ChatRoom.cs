using System.Collections.Concurrent;
using System.Net.Sockets;

namespace ChatServer
{
    public static class ChatRoom
    {
        private static readonly ConcurrentDictionary<TcpClient, (string Username, ClientHandler Handler)> Clients = new();

        public static void AddClient(TcpClient client, string username, ClientHandler handler)
        {
            Clients.TryAdd(client, (username, handler));
        }

        public static bool RemoveClient(TcpClient client)
        {
            return Clients.TryRemove(client, out _);
        }

        public static IEnumerable<TcpClient> GetClients()
        {
            return Clients.Keys;
        }

        public static string GetUsername(TcpClient client)
        {
            return Clients.TryGetValue(client, out var entry) ? entry.Username : "Unknown";
        }

        public static ClientHandler GetHandler(TcpClient client)
        {
            return Clients.TryGetValue(client, out var entry) ? entry.Handler : null!;
        }
    }
}