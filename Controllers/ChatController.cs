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
        private readonly IFileService _fileService;

        public ChatController(SharedDb sharedDb, IHubContext<ChatHub> hubContext, IFileService fileService)
        {
            _sharedDb = sharedDb;
            _hubContext = hubContext;
            _fileService = fileService;
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateChat([FromForm] string username, [FromForm] string chatRoomName, IFormFile? imageFile)
        {
            if (string.IsNullOrWhiteSpace(chatRoomName))
            {
                return BadRequest("EMPTY_ROOM_NAME");
            }

            if (string.IsNullOrWhiteSpace(username))
            {
                return BadRequest("EMPTY_USERNAME");
            }

            int nextId = _sharedDb.chatRooms.IsEmpty ? 1 : _sharedDb.chatRooms.Keys.Max() + 1;

            //string? imageBase64 = null;
            string? imageUrl = null;

            if (imageFile != null)
            {
                var key = await _fileService.UploadRoomImageAsync(imageFile, nextId);
                if (key != null)
                {
                    var ext = Path.GetExtension(imageFile.FileName);
                    imageUrl = _fileService.GetImageUrl(nextId, ext);
                }
                //using var memoryStream = new MemoryStream();
                //await imageFile.CopyToAsync(memoryStream);
                //byte[] imageBytes = memoryStream.ToArray();
                //imageBase64 = Convert.ToBase64String(imageBytes);
            }

            //var newChatRoom = new ChatRoom(nextId, chatRoomName, imageBase64);
            var newChatRoom = new ChatRoom(nextId, chatRoomName, imageUrl);

            if (_sharedDb.chatRooms.TryAdd(nextId, newChatRoom))
            {
                var userConnection = new UserConnection
                {
                    Username = username,
                    ChatRoomId = nextId,
                    ChatRoomName = chatRoomName
                };

                _sharedDb.connections[username] = userConnection;

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

        [HttpGet("rooms")]
        public IActionResult GetRooms()
        {
            var rooms = _sharedDb.chatRooms.Values.Select(r => new { id = r.Id, name = r.Name }).ToList();
            return Ok(rooms);
        }
    }
}
