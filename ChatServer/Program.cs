using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Collections.Generic;
using ChatServer;

Console.WriteLine("Starting TCP Chat Server...");

var listener = new TcpListener(IPAddress.Any, 5000);
listener.Start();
Console.WriteLine("Server is listening on port 5000...");

List<ClientHandler> handlers = new();

// Start background task to clean up inactive clients
_ = CleanUpInactiveClientsAsync(handlers);

while (true)
{
    var client = await listener.AcceptTcpClientAsync();
    var handler = new ClientHandler(client, OnClientDisconnected);
    handlers.Add(handler);
    _ = handler.Start(); // Start handling the client
}

// Callback when a client disconnects
void OnClientDisconnected(ClientHandler handler)
{
    lock (handlers)
        handlers.Remove(handler);
    Console.WriteLine($"{handler.Username} disconnected.");
}

// Background cleanup task
async Task CleanUpInactiveClientsAsync(List<ClientHandler> handlers)
{
    while (true)
    {
        await Task.Delay(300000); // Every 5 minutes
        var now = DateTime.Now;

        foreach (var handler in handlers.ToList())
        {
            if ((now - handler.LastActivityTime).TotalMinutes > 10) // 10-minute timeout
            {
                Console.WriteLine($"Disconnecting inactive user: {handler.Username}");
                handler.Disconnect(); // Call a safe disconnect method
                handlers.Remove(handler);
            }
        }
    }
}