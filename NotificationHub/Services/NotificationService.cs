using Microsoft.AspNetCore.SignalR;
using NotificationHub.Data;
using NotificationHub.Hubs;
using NotificationHub.Models;

namespace NotificationHub.Services;

public interface INotificationService
{
    Task SendNotificationAsync(string username, NotificationDto notification);
    Task<List<NotificationDto>> GetPendingNotificationsAsync(string username);
}

public class NotificationService : INotificationService
{
    private readonly IHubContext<NotificationHubClass> _hubContext;
    private readonly INotificationRepository _repository;

    public NotificationService(IHubContext<NotificationHubClass> hubContext, INotificationRepository repository)
    {
        _hubContext = hubContext;
        _repository = repository;
    }

    public async Task SendNotificationAsync(string username, NotificationDto notification)
    {
        await _hubContext.Clients.Group($"User_{username}")
            .SendAsync("ClientReceiveNotification", notification);
    }

    public async Task<List<NotificationDto>> GetPendingNotificationsAsync(string username)
    {
        var notifications = await _repository.GetPendingNotificationsAsync(username);
        
        return notifications.Select(n => new NotificationDto
        {
            Id = n.Id,
            ClaveId = n.ClaveId,
            SolicitudId = n.SolicitudId,
            NombreProceso = n.NombreProceso,
            Titulo = n.Titulo,
            Mensaje = n.Mensaje,
            NotificationTipo = n.NotificationTipo,
            IsAcknowledged = n.IsAcknowledged,
            RelativeNotifiedDateAndTime = n.RelativeNotifiedDateAndTime,
            CreatedOnUtc = n.CreatedOnUtc,
            Estado = n.Estado,
            Usuario = n.Usuario,
            Prioridad = n.Prioridad
        }).ToList();
    }
}
