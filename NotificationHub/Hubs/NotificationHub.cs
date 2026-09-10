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

            // Enviar solo las notificaciones no atendidas de este usuario específico
            await SendPendingNotificationsToCallerAsync(username);
        }
        
        await base.OnConnectedAsync();
    }

    private async Task SendPendingNotificationsToCallerAsync(string username)
    {
        try
        {
            var pendingNotifications = await _repository.GetPendingNotificationsAsync(username);

            foreach (var notification in pendingNotifications)
            {
                var notificationDto = new NotificationDto
                {
                    Id = notification.Id,
                    ClaveId = notification.ClaveId,
                    SolicitudId = notification.SolicitudId,
                    NombreProceso = notification.NombreProceso,
                    Titulo = notification.Titulo,
                    Mensaje = notification.Mensaje,
                    NotificationTipo = notification.NotificationTipo,
                    IsAcknowledged = notification.IsAcknowledged,
                    RelativeNotifiedDateAndTime = notification.RelativeNotifiedDateAndTime,
                    CreatedOnUtc = notification.CreatedOnUtc,
                    Estado = notification.Estado,
                    Usuario = notification.Usuario,
                    Prioridad = notification.Prioridad
                };

                // Enviar solo al cliente que se conectó (Caller)
                await Clients.Caller.SendAsync("ClientReceiveNotification", notificationDto);
            }

            Console.WriteLine($"Enviadas {pendingNotifications.Count} notificaciones pendientes al usuario {username}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al enviar notificaciones pendientes al usuario {username}: {ex.Message}");
        }
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
