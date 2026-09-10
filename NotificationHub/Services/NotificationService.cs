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

public class NotificationWorkerService : BackgroundService
{
    private readonly ILogger<NotificationWorkerService> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly TimeSpan _pollingInterval = TimeSpan.FromSeconds(30);

    public NotificationWorkerService(ILogger<NotificationWorkerService> logger, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Notification Worker Service iniciado.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var repository = scope.ServiceProvider.GetRequiredService<INotificationRepository>();
                    var hubContext = scope.ServiceProvider.GetRequiredService<IHubContext<NotificationHubClass>>();

                    // Enviar notificaciones con estado 0 (no atendidas) solo a usuarios específicos
                    await SendUnsentNotificationsByEstadoAsync(repository, hubContext, stoppingToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en Notification Worker Service");
            }

            await Task.Delay(_pollingInterval, stoppingToken);
        }

        _logger.LogInformation("Notification Worker Service detenido.");
    }

    private async Task SendUnsentNotificationsByEstadoAsync(
        INotificationRepository repository, 
        IHubContext<NotificationHubClass> hubContext,
        CancellationToken stoppingToken)
    {
        var notifications = await repository.GetUnsentNotificationsByEstadoAsync();

        foreach (var notification in notifications)
        {
            if (stoppingToken.IsCancellationRequested)
                break;

            // Solo enviar si tiene un usuario específico asignado (excluir 'system')
            if (!string.IsNullOrEmpty(notification.Usuario) && notification.Usuario.ToLower() != "system")
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

                // Enviar solo al usuario específico, no a todos
                await hubContext.Clients.Group($"User_{notification.Usuario}")
                    .SendAsync("ClientReceiveNotification", notificationDto, stoppingToken);

                _logger.LogInformation($"Notificación {notification.Id} enviada al usuario {notification.Usuario}");

                // Marcar la notificación como enviada para que no se vuelva a enviar
                await repository.MarkNotificationAsSentAsync(notification.Id);
            }
        }
    }
}
