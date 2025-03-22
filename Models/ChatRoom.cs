namespace FormulaOne.ChatService.Models
{
    public class ChatRoom
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public List<Message> Messages { get; set; } = [];

        public ChatRoom(int id, string name) { Id = id; Name = name; }
    }
}
