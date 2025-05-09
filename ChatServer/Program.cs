using Google.Cloud.Firestore;
using Google.Apis.Auth.OAuth2;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Collections.Generic;
using ChatServer;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

// Initialize Firebase
string projectId = "tcp-chat-app-8273b";
string firebaseKeyPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tcp-chat-app-8273b-firebase-adminsdk-fbsvc-cbceaaa0a8.json");
FirebaseLogger.Initialize(projectId, firebaseKeyPath);

Console.WriteLine("Starting TCP Chat Server...");

// Load SSL certificate
var certificate = new X509Certificate2("chatserver.pfx", "password");

var listener = new TcpListener(IPAddress.Any, 5000);
listener.Start();
Console.WriteLine("Server is listening on port 5000...");

List<ClientHandler> handlers = new();

// Start background task to clean up inactive clients
_ = CleanUpInactiveClientsAsync(handlers);

while (true)
{
    var client = await listener.AcceptTcpClientAsync();
    var sslStream = new SslStream(client.GetStream(), false);

    try
    {
        await sslStream.AuthenticateAsServerAsync(certificate);
        var handler = new ClientHandler(client, sslStream, OnClientDisconnected);
        handlers.Add(handler);
        _ = handler.Start();
    }
    catch (Exception ex)
    {
        Console.WriteLine("SSL Error: " + ex.Message);
        client.Close();
    }
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
                handler.Disconnect();
                handlers.Remove(handler);
            }
        }
    }
}