namespace FormulaOne.ChatService.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<ChatRoom> RoomList { get; set; } = new List<ChatRoom>();
    }
}
