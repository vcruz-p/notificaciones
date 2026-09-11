using Microsoft.AspNetCore.SignalR;
using NotificationHub.Data;
using NotificationHub.Models;

namespace NotificationHub.Hubs;

public class NotificationHubClass : Hub
{
    private readonly INotificationRepository _repository;

    public NotificationHubClass(INotificationRepository repository)
    {
        _repository = repository;
    }

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
