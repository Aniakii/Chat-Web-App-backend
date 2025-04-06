namespace FormulaOne.ChatService.Models
{
    public class Message
    {
        public int Id { get; set; }
        public string User { get; set; }
        public string MessageContent { get; set; }
        public Message(int id, string user, string messageContent) { Id = id; User = user; MessageContent = messageContent; }
    }
}
