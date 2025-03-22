using System.Collections.Concurrent;
using FormulaOne.ChatService.Models;

namespace FormulaOne.ChatService.DataService
{
    public class SharedDb
    {
        private readonly ConcurrentDictionary<string, UserConnection> _connections = new ConcurrentDictionary<string, UserConnection>();

        private readonly ConcurrentDictionary<string, User> _users = new ConcurrentDictionary<string, User>();
        private readonly ConcurrentDictionary<string, ChatRoom> _chatRooms = new ConcurrentDictionary<string, ChatRoom>();
        public ConcurrentDictionary<string, UserConnection> connections => _connections;
        public ConcurrentDictionary<string, User> users => _users;
        public ConcurrentDictionary<string, ChatRoom> chatRooms => _chatRooms;



    }
}
