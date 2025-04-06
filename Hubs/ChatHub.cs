using FormulaOne.ChatService.Models;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.SignalR;

namespace FormulaOne.ChatService.Hubs
{
    [EnableCors("AllowSpecificOrigin")]
    public class ChatHub : Hub
    {
        public async Task SubscribeChat(UserConnection connection)
        {

            await Groups.AddToGroupAsync(Context.ConnectionId, connection.ChatRoomId.ToString());
            await Clients.Group(connection.ChatRoomId.ToString()).SendAsync("ReceiveMessage", "admin", $"{connection.Username} has joined");

        }

        public async Task UnsubscribeChat(UserConnection connection)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, connection.ChatRoomId.ToString());
            await Clients.Group(connection.ChatRoomId.ToString()).SendAsync("ReceiveMessage", "admin", $"{connection.Username} has left chat");
        }
    }
}
