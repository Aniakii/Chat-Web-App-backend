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


        public async Task<ChatRoom?> CreateChat(UserConnection connection)
        {

            if (string.IsNullOrWhiteSpace(connection.ChatRoomName))
            {
                throw new HubException("EMPTY_ROOM_NAME");
            }

            Console.WriteLine($"User {connection.Username} próbuje utworzyć pokój");

            int nextId = _sharedDb.chatRooms.IsEmpty ? 1 : _sharedDb.chatRooms.Keys.Max() + 1;

            var newChatRoom = new ChatRoom(nextId, connection.ChatRoomName);

            if (_sharedDb.chatRooms.TryAdd(nextId, newChatRoom))
            {
                connection.ChatRoomId = nextId;

                await Groups.AddToGroupAsync(Context.ConnectionId, connection.ChatRoomId.ToString());

                _sharedDb.connections[Context.ConnectionId] = connection;

                Console.WriteLine($"User {connection.Username} dołączył do nowo utworzonego pokoju {connection.ChatRoomId}");

                await Clients.Group(connection.ChatRoomId.ToString()).SendAsync("ReceiveMessage", "admin", $"{connection.Username} created chat {connection.ChatRoomName}");
                return newChatRoom;
            }
            else
            {
                throw new HubException("ROOM_CREATION_FAILED");
            }
        }

        public async Task<ChatRoom?> JoinChat(UserConnection connection)
        {
            Console.WriteLine($"User {connection.Username} próbuje dołączyć do pokoju {connection.ChatRoomId}");

            if (connection.ChatRoomId <= 0)
            {
                throw new HubException("INVALID_ROOM_ID");
            }

            if (!_sharedDb.chatRooms.TryGetValue(connection.ChatRoomId, out var chatRoom))
            {
                throw new HubException("ROOM_NOT_FOUND");

            }

            await Groups.AddToGroupAsync(Context.ConnectionId, connection.ChatRoomId.ToString());
            _sharedDb.connections[Context.ConnectionId] = connection;

            await Clients.Group(connection.ChatRoomId.ToString()).SendAsync("ReceiveMessage", "admin", $"{connection.Username} has joined {chatRoom.Name}");

            return chatRoom;
        }

        public async Task SendMessage(string msg)
        {
            if (string.IsNullOrWhiteSpace(msg))
            {
                throw new HubException("EMPTY_MESSAGE");
            }
            if (_sharedDb.connections.TryGetValue(Context.ConnectionId, out UserConnection conn))
            {
                //var chatRoom = _sharedDb.chatRooms[conn.ChatRoomId];
                //int nextId = chatRoom.Messages.Any() ? chatRoom.Messages.Last().Id + 1 : 1;
                //var user = _sharedDb.users.Where(n => n.Value.Name == conn.Username);
                //var message = new Message(nextId, (User)user);

                //message.messageContent = msg;


                await Clients.Group(conn.ChatRoomId.ToString()).SendAsync("ReceiveMessage", conn.Username, msg);

                //_sharedDb.chatRooms[conn.ChatRoomId].Messages.Add(message);

            }
        }
    }
}
