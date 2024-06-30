using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

public class FlightHub : Hub
{
    public Task SendMessage(string user, string message)
    {
        return Clients.All.SendAsync("ReceiveMessage", user, message);
    }
}