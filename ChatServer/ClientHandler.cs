using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ChatServer
{
    public class ClientHandler
    {
        private readonly TcpClient _client;
        private readonly Action<ClientHandler> _disconnectedCallback;
        private string _username = "Anonymous";

        public string Username => _username;
        public TcpClient Client => _client;
        public DateTime LastActivityTime { get; private set; } = DateTime.Now;

        public ClientHandler(TcpClient client, Action<ClientHandler> disconnectedCallback)
        {
            _client = client;
            _disconnectedCallback = disconnectedCallback;
        }

        public async Task Start()
        {
            // Your existing Start method logic...
        }

        public void Disconnect()
        {
            try
            {
                _client.Close();
            }
            catch { /* Ignore errors on close */ }

            _disconnectedCallback?.Invoke(this);
        }

        // Other methods like HandlePrivateMessage, SendToClientAsync, etc.
    }
}
