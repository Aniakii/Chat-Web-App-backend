namespace FormulaOne.ChatService.Models
{
    public class ChatRoom
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? ImageBase64 { get; set; }

        public List<Message> Messages { get; set; } = [];

        public ChatRoom(int id, string name, string? imageBase64 = null) { Id = id; Name = name; ImageBase64 = imageBase64; }
    }
}
