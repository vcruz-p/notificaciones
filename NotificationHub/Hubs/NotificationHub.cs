using Microsoft.AspNetCore.SignalR;
using NotificationHub.Models;

namespace NotificationHub.Hubs;

public class NotificationHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        var username = Context.GetHttpContext()?.Request.Query["username"].ToString();
        
        if (!string.IsNullOrEmpty(username))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"User_{username}");
            Console.WriteLine($"Usuario {username} conectado con ConnectionId: {Context.ConnectionId}");
        }
        
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        Console.WriteLine($"Usuario desconectado: {Context.ConnectionId}");
        await base.OnDisconnectedAsync(exception);
    }

    public async Task SendNotificationToUser(string username, NotificationDto notification)
    {
        await Clients.Group($"User_{username}").SendAsync("ClientReceiveNotification", notification);
    }
}
