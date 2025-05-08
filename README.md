# TCP Chat Server & Client in C#

A simple TCP chat application built in C#/.NET with:
- Private Messaging (`/pm Alice Hello`)
- User Listing (`/listusers`)
- Logging
- Session Timeout

## Features
- Console-based chat server and client
- Supports multiple users
- Logs messages to `chatserver.log`
- Auto-cleans inactive users after 10 minutes

## Getting Started
```bash
# Run server
dotnet run --project ChatServer

# Run client
dotnet run --project ChatClient