namespace FormulaOne.ChatService.Models
{
    public class Message
    {
        public int Id { get; set; }
        public string User { get; set; }
        public string MessageContent { get; set; }

        public int ChatRoomId { get; set; }
        public ChatRoom ChatRoom { get; set; }
        public Message(string user, string messageContent, int chatRoomId) { User = user; MessageContent = messageContent; ChatRoomId = chatRoomId; }
    }
}
