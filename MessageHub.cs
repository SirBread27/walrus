using Microsoft.AspNetCore.SignalR;

namespace WalrusMessenger
{
    public class MessageHub : Hub
    {
        public async Task PrivateMessageUpd(long chatId, Message msg)
        {
            await Clients.Others.SendAsync("RecieveMessage", chatId, msg);
        }
    }
}
