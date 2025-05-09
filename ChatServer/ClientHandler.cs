using System;
using System.Net.Sockets;
using System.Net.Security;
using System.Text;
using System.Threading.Tasks;

namespace ChatServer
{
    public class ClientHandler
    {
        private readonly TcpClient _client;
        private readonly SslStream _sslStream;
        private readonly Action<ClientHandler> _disconnectedCallback;
        private string _username = "Anonymous";

        public string Username => _username;
        public DateTime LastActivityTime { get; private set; } = DateTime.Now;

        public ClientHandler(TcpClient client, SslStream sslStream, Action<ClientHandler> disconnectedCallback)
        {
            _client = client;
            _sslStream = sslStream;
            _disconnectedCallback = disconnectedCallback;
        }

        public async Task Start()
        {
            var buffer = new byte[1024];

            try
            {
                // Ask for username
                await SendToClientAsync("USERNAME:");
                int read = await _sslStream.ReadAsync(buffer, 0, buffer.Length);
                if (read > 0)
                {
                    _username = Encoding.UTF8.GetString(buffer, 0, read).Trim();
                    await SendJoinMessageAsync();
                    ChatRoom.AddClient(_client, _username, this); // Pass 'this' as handler
                }

                while (true)
                {
                    read = await _sslStream.ReadAsync(buffer, 0, buffer.Length);
                    if (read == 0) break;

                    LastActivityTime = DateTime.Now;

                    string message = Encoding.UTF8.GetString(buffer, 0, read).Trim();

                    if (message.StartsWith("/pm "))
                    {
                        await HandlePrivateMessageAsync(message);
                    }
                    else if (message == "/listusers")
                    {
                        await SendUserListAsync();
                    }
                    else if (!string.IsNullOrWhiteSpace(message))
                    {
                        string formattedMessage = $"{_username}: {message}";
                        Logger.Log(formattedMessage);
                        await BroadcastMessageAsync(formattedMessage);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"Error with {_username}: {ex.Message}");
            }
            finally
            {
                await SendLeaveMessageAsync();
                ChatRoom.RemoveClient(_client);
                _disconnectedCallback(this);
                Disconnect();
            }
        }

        private async Task SendJoinMessageAsync()
        {
            string serverMessage = $"[SERVER] {_username} has joined the chat.";
            string personalMessage = "[SERVER] You have joined the chat.";

            Logger.Log(serverMessage);
            await BroadcastMessageAsync(serverMessage);
            await SendToClientAsync(personalMessage);
        }

        private async Task SendLeaveMessageAsync()
        {
            string leaveMessage = $"[SERVER] {_username} has left the chat.";
            Logger.Log(leaveMessage);
            await BroadcastMessageAsync(leaveMessage);
        }

        private async Task SendUserListAsync()
        {
            var usernames = ChatRoom.GetClients().Select(ChatRoom.GetUsername).ToList();
            if (usernames.Count == 0)
            {
                await SendToClientAsync("/users No users online.");
                return;
            }

            string userList = "Server 1\n";
            for (int i = 0; i < usernames.Count; i++)
            {
                userList += $"  User {i + 1}: {usernames[i]}\n";
            }

            await SendToClientAsync($"/users {userList}");
        }

        private async Task HandlePrivateMessageAsync(string message)
        {
            string[] parts = message.Substring(4).Split(' ', 2);
            if (parts.Length < 2) return;

            string targetUser = parts[0];
            string privateMessage = parts[1];

            foreach (var client in ChatRoom.GetClients())
            {
                if (ChatRoom.GetUsername(client) == targetUser && client.Connected)
                {
                    string logMessage = $"{_username} -> {targetUser}: {privateMessage}";
                    Logger.LogPrivateMessage(_username, targetUser, privateMessage);

                    var handler = ChatRoom.GetHandler(client);
                    if (handler != null)
                    {
                        await SendToClientAsync(handler._sslStream, $"/pm [{_username}] {privateMessage}");
                    }

                    await SendToClientAsync(_sslStream, $"/pm [To {targetUser}] {privateMessage}");
                    break;
                }
            }
        }

        private async Task SendToClientAsync(SslStream recipient, string message)
        {
            byte[] data = Encoding.UTF8.GetBytes(message + "\n");
            await recipient.WriteAsync(data, 0, data.Length);
        }

        private async Task SendToClientAsync(string message)
        {
            await SendToClientAsync(_sslStream, message);
        }

        private async Task BroadcastMessageAsync(string message)
        {
            byte[] data = Encoding.UTF8.GetBytes(message + "\n");

            foreach (var client in ChatRoom.GetClients())
            {
                var handler = ChatRoom.GetHandler(client);
                if (handler != null && client.Connected)
                {
                    try
                    {
                        await handler.SendToClientAsync(message);
                    }
                    catch
                    {
                        // Ignore failed writes
                    }
                }
            }
        }

        public void Disconnect()
        {
            try
            {
                _sslStream.Dispose();
                _client.Close();
            }
            catch { /* Ignore errors on close */ }
        }
    }
}