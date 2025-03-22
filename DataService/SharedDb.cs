using System.Collections.Concurrent;
using FormulaOne.ChatService.Models;

namespace FormulaOne.ChatService.DataService
{
    public class SharedDb
    {
        private readonly ConcurrentDictionary<string, UserConnection> _connections = new ConcurrentDictionary<string, UserConnection>();

        private readonly ConcurrentDictionary<int, User> _users = new ConcurrentDictionary<int, User>();
        private readonly ConcurrentDictionary<int, ChatRoom> _chatRooms = new ConcurrentDictionary<int, ChatRoom>();
        public ConcurrentDictionary<string, UserConnection> connections => _connections;
        public ConcurrentDictionary<int, User> users => _users;
        public ConcurrentDictionary<int, ChatRoom> chatRooms => _chatRooms;



    }
}
