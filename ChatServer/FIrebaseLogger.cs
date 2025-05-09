using Google.Cloud.Firestore;
using Google.Apis.Auth.OAuth2;
using System;
using System.Threading.Tasks;
using Google.Cloud.Firestore.V1;

public static class FirebaseLogger
{
    private static FirestoreDb _db;

    public static void Initialize(string projectId, string jsonKeyPath)
    {
        try
        {
            // Load credentials from the service account JSON file
            GoogleCredential credential = GoogleCredential.FromFile(jsonKeyPath);

            // Create Firestore DB instance using the correct builder usage
            var firestoreClient = new FirestoreClientBuilder
            {
                Credential = credential
            }.Build();

            // Build the FirestoreDb with the client and project ID
            _db = FirestoreDb.Create(projectId, firestoreClient);

            Console.WriteLine("Firebase Firestore initialized.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error initializing Firebase: {ex.Message}");
        }
    }

    public static async Task LogMessageAsync(string username, string message)
    {
        if (_db == null)
        {
            Console.WriteLine("Firebase not initialized. Cannot log message.");
            return;
        }

        var docRef = _db.Collection("chat_logs").Document();
        await docRef.SetAsync(new
        {
            Username = username,
            Message = message,
            Timestamp = Timestamp.FromDateTime(DateTime.UtcNow)
        });
    }

    public static async Task LogPrivateMessageAsync(string sender, string receiver, string message)
    {
        if (_db == null)
        {
            Console.WriteLine("Firebase not initialized. Cannot log private message.");
            return;
        }

        var docRef = _db.Collection("private_messages").Document();
        await docRef.SetAsync(new
        {
            Sender = sender,
            Receiver = receiver,
            Message = message,
            Timestamp = Timestamp.FromDateTime(DateTime.UtcNow)
        });
    }
}