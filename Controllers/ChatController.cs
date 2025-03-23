using FormulaOne.ChatService.Controllers.Dto;
using FormulaOne.ChatService.DataService;
using FormulaOne.ChatService.Hubs;
using FormulaOne.ChatService.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace FormulaOne.ChatService.Controllers
{
    [ApiController]
    [Route("api/chat")]
    public class ChatController : ControllerBase
    {
        private readonly SharedDb _sharedDb;
        private readonly IHubContext<ChatHub> _hubContext;

        public ChatController(SharedDb sharedDb, IHubContext<ChatHub> hubContext)
        {
            _sharedDb = sharedDb;
            _hubContext = hubContext;
        }


        [HttpPost("create")]
        public IActionResult CreateChat([FromBody] UserConnection connection)
        {
            if (string.IsNullOrWhiteSpace(connection.ChatRoomName))
            {
                return BadRequest("EMPTY_ROOM_NAME");
            }

            int nextId = _sharedDb.chatRooms.IsEmpty ? 1 : _sharedDb.chatRooms.Keys.Max() + 1;

            var newChatRoom = new ChatRoom(nextId, connection.ChatRoomName);

            if (_sharedDb.chatRooms.TryAdd(nextId, newChatRoom))
            {
                connection.ChatRoomId = nextId;
                _sharedDb.connections[connection.Username] = connection;

                return Ok(newChatRoom);
            }

            return StatusCode(500, "ROOM_CREATION_FAILED");
        }

        [HttpPost("join")]
        public IActionResult JoinChat([FromBody] UserConnection connection)
        {
            if (connection.ChatRoomId <= 0)
            {
                return BadRequest("INVALID_ROOM_ID");
            }

            if (!_sharedDb.chatRooms.TryGetValue(connection.ChatRoomId, out var chatRoom))
            {
                return NotFound("ROOM_NOT_FOUND");
            }

            _sharedDb.connections[connection.Username] = connection;
            return Ok(chatRoom);
        }


        [HttpPost("send")]
        public IActionResult SendMessage([FromBody] MessageDto messageDto)
        {
            if (string.IsNullOrWhiteSpace(messageDto.Content))
            {
                return BadRequest("EMPTY_MESSAGE");
            }

            if (!_sharedDb.connections.TryGetValue(messageDto.Username, out UserConnection conn))
            {
                return Unauthorized("USER_NOT_IN_ROOM");
            }

            var chatRoom = _sharedDb.chatRooms[conn.ChatRoomId];
            var message = new Message(chatRoom.Messages.Count + 1, messageDto.Username, messageDto.Content);
            chatRoom.Messages.Add(message);

            _hubContext.Clients.Group(conn.ChatRoomId.ToString()).SendAsync("ReceiveMessage", conn.Username, messageDto.Content);

            return Ok(message);
        }


        [HttpGet("messages/{roomId}")]
        public IActionResult GetMessages(int roomId)
        {
            if (!_sharedDb.chatRooms.TryGetValue(roomId, out var chatRoom))
            {
                return NotFound("ROOM_NOT_FOUND");
            }

            return Ok(chatRoom.Messages);
        }
    }
}
