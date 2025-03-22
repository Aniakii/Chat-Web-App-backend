using FormulaOne.ChatService.DataService;
using FormulaOne.ChatService.Models;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.SignalR;

namespace FormulaOne.ChatService.Hubs
{
    [EnableCors("AllowSpecificOrigin")]
    public class ChatHub : Hub
    {
        private readonly SharedDb _sharedDb;

        public ChatHub(SharedDb sharedDb) => _sharedDb = sharedDb;
        public async Task JoinChat(UserConnection connection)
        {
            await Clients.All.SendAsync("ReceiveMessage", "admin", $"{connection.Username} has joined");
        }

        public async Task CreateChat(UserConnection connection)
        {
            int nextId;
            if (_sharedDb.chatRooms.IsEmpty)
            {
                nextId = 0;
            }
            else
            {
                nextId = _sharedDb.chatRooms.Last().Value.Id + 1;
            }

            var newChatRoom = new ChatRoom(nextId, connection.ChatRoomName);

            // Używamy TryAdd, aby uniknąć nadpisywania istniejącego klucza
            if (_sharedDb.chatRooms.TryAdd(connection.ChatRoomName, newChatRoom))
            {
                await Clients.All.SendAsync("ReceiveMessage", "admin", $"{connection.Username} created chat {connection.ChatRoomName}");
            }
            else
            {
                await Clients.Caller.SendAsync("ReceiveMessage", "admin", $"Chat {connection.ChatRoomName} already exists");
            }
        }

        public async Task JoinSpecificChatRoom(UserConnection connection)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, connection.ChatRoomName);

            _sharedDb.connections[Context.ConnectionId] = connection;

            await Clients.Group(connection.ChatRoom).
                SendAsync("ReceiveMessage", "admin", $"{connection.Username} has joined {connection.ChatRoom}");
        }

        public async Task SendMessage(string msg)
        {
            if (_sharedDb.connections.TryGetValue(Context.ConnectionId, out UserConnection conn))
            {
                await Clients.Group(conn.ChatRoom).SendAsync("ReceiveSpecificMessage", conn.Username, msg);
            }
        }
    }
}
