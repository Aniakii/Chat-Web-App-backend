namespace FormulaOne.ChatService.Models
{
    public class Message
    {
        public int Id { get; set; }
        public User User { get; set; }
        public string messageContent { get; set; }

        public Message(int id, User user) { Id = id; User = user; }
    }
}
