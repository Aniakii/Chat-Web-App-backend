using FormulaOne.ChatService.Controllers.Dto;
using FormulaOne.ChatService.DataService;
using FormulaOne.ChatService.Hubs;
using FormulaOne.ChatService.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace FormulaOne.ChatService.Controllers
{
    [ApiController]
    [Route("api/chat")]
    public class ChatController : ControllerBase
    {
        private readonly AwsDbContext _dbContext;
        private readonly IHubContext<ChatHub> _hubContext;
        private readonly IFileService _fileService;

        public ChatController(AwsDbContext dbContext, IHubContext<ChatHub> hubContext, IFileService fileService)
        {
            _dbContext = dbContext;
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

            var nextId = _dbContext.ChatRooms.Any() ? _dbContext.ChatRooms.Max(c => c.Id) + 1 : 1;

            string? imageUrl = null;

            if (imageFile != null)
            {
                var key = await _fileService.UploadRoomImageAsync(imageFile, nextId);
                if (key != null)
                {
                    var ext = Path.GetExtension(imageFile.FileName);
                    imageUrl = _fileService.GetImageUrl(nextId, ext);
                }
            }

            var newChatRoom = new ChatRoom(nextId, chatRoomName, imageUrl);

            try
            {
                _dbContext.ChatRooms.Add(newChatRoom);
                await _dbContext.SaveChangesAsync();
            }
            catch
            {
                return StatusCode(500, "ROOM_CREATION_FAILED");
            }

            var userConnection = new UserConnection
            {
                Username = username,
                ChatRoomId = nextId,
                ChatRoomName = chatRoomName
            };

            _dbContext.UsersConnection.Add(userConnection);
            await _dbContext.SaveChangesAsync();

            return Ok(newChatRoom);
        }

        [HttpPost("join")]
        public async Task<IActionResult> JoinChat([FromBody] UserConnection connection)
        {
            if (connection.ChatRoomId <= 0)
            {
                return BadRequest("INVALID_ROOM_ID");
            }

            var room = await _dbContext.ChatRooms.FindAsync(connection.ChatRoomId);
            if (room == null)
                return NotFound("ROOM_NOT_FOUND");
            else
                connection.ChatRoomName = room.Name;

            _dbContext.UsersConnection.Add(connection);
            await _dbContext.SaveChangesAsync();
            return Ok(room);
        }


        [HttpPost("send")]
        public async Task<IActionResult> SendMessage([FromBody] MessageDto messageDto)
        {
            if (string.IsNullOrWhiteSpace(messageDto.Content))
            {
                return BadRequest("EMPTY_MESSAGE");
            }

            var connection = await _dbContext.UsersConnection
                .FirstOrDefaultAsync(c => c.Username == messageDto.Username);


            if (connection == null)
            {
                return Unauthorized("USER_NOT_IN_ROOM");
            }

            var chatRoom = await _dbContext.ChatRooms.FindAsync(connection.ChatRoomId);
            if (chatRoom == null)
            {
                return BadRequest("ROOM_NOT_FOUND");
            }

            try
            {
                var message = new Message(chatRoom.Messages.Count + 1, messageDto.Username, messageDto.Content);
                chatRoom.Messages.Add(message);

                await _hubContext.Clients.Group(connection.ChatRoomId.ToString()).SendAsync("ReceiveMessage", connection.Username, messageDto.Content);
                return Ok(message);
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR: " + ex.Message);
                return StatusCode(500, "Internal server error: " + ex.Message);
            }


        }


        [HttpGet("messages/{roomId}")]
        public async Task<IActionResult> GetMessages(int roomId)
        {
            var room = await _dbContext.ChatRooms.FindAsync(roomId);
            if (room == null)
                return NotFound("ROOM_NOT_FOUND");
            return Ok(room.Messages);
        }

        [HttpGet("rooms")]
        public async Task<IActionResult> GetRooms()
        {
            var rooms = await _dbContext.ChatRooms.ToListAsync();
            return Ok(rooms);
        }
    }
}
