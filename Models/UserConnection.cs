namespace FormulaOne.ChatService.Models
{

    public class UserConnection
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Username { get; set; } = string.Empty;
        public string ChatRoomName { get; set; } = string.Empty;
        public int ChatRoomId { get; set; }
    }
}
