using MySqlConnector;
using NotificationHub.Models;

namespace NotificationHub.Data;

public interface INotificationRepository
{
    Task<List<Notification>> GetPendingNotificationsAsync(string usuario);
    Task MarkAsAcknowledgedAsync(int notificationId);
    Task<List<Notification>> GetUnsentNotificationsByEstadoAsync();
    Task<List<Notification>> GetSystemNotificationsAsync();
    Task MarkNotificationAsSentAsync(int notificationId);
}

public class NotificationRepository : INotificationRepository
{
    private readonly string _connectionString;

    public NotificationRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection") 
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
    }

    public async Task<List<Notification>> GetPendingNotificationsAsync(string usuario)
    {
        var notifications = new List<Notification>();
        
        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();
        
        var sql = @"SELECT Id, ClaveId, SolicitudId, NombreProceso, Titulo, Mensaje, 
                           NotificationTipo, IsAcknowledged, RelativeNotifiedDateAndTime, 
                           CreatedOnUtc, Estado, usuario, prioridad
                    FROM notifications 
                    WHERE usuario = @Usuario AND IsAcknowledged = 0
                    ORDER BY CreatedOnUtc DESC";
        
        await using var cmd = new MySqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("@Usuario", usuario);
        
        await using var reader = await cmd.ExecuteReaderAsync();
        
        while (await reader.ReadAsync())
        {
            notifications.Add(new Notification
            {
                Id = reader.GetInt32("Id"),
                ClaveId = reader.IsDBNull(reader.GetOrdinal("ClaveId")) ? null : reader.GetInt32("ClaveId"),
                SolicitudId = reader.IsDBNull(reader.GetOrdinal("SolicitudId")) ? null : reader.GetInt32("SolicitudId"),
                NombreProceso = reader.IsDBNull(reader.GetOrdinal("NombreProceso")) ? null : reader.GetString("NombreProceso"),
                Titulo = reader.IsDBNull(reader.GetOrdinal("Titulo")) ? null : reader.GetString("Titulo"),
                Mensaje = reader.IsDBNull(reader.GetOrdinal("Mensaje")) ? null : reader.GetString("Mensaje"),
                NotificationTipo = reader.IsDBNull(reader.GetOrdinal("NotificationTipo")) ? null : reader.GetString("NotificationTipo"),
                IsAcknowledged = reader.GetBoolean("IsAcknowledged"),
                RelativeNotifiedDateAndTime = reader.IsDBNull(reader.GetOrdinal("RelativeNotifiedDateAndTime")) ? null : reader.GetString("RelativeNotifiedDateAndTime"),
                CreatedOnUtc = reader.GetDateTime("CreatedOnUtc"),
                Estado = reader.IsDBNull(reader.GetOrdinal("Estado")) ? null : reader.GetInt32("Estado"),
                Usuario = reader.IsDBNull(reader.GetOrdinal("usuario")) ? null : reader.GetString("usuario"),
                Prioridad = reader.IsDBNull(reader.GetOrdinal("prioridad")) ? null : reader.GetInt32("prioridad")
            });
        }
        
        return notifications;
    }

    public async Task<List<Notification>> GetUnsentNotificationsByEstadoAsync()
    {
        var notifications = new List<Notification>();
        
        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();
        
        // Obtener notificaciones con estado 0 que no han sido atendidas
        var sql = @"SELECT Id, ClaveId, SolicitudId, NombreProceso, Titulo, Mensaje, 
                           NotificationTipo, IsAcknowledged, RelativeNotifiedDateAndTime, 
                           CreatedOnUtc, Estado, usuario, prioridad
                    FROM notifications 
                    WHERE Estado = 0
                    ORDER BY CreatedOnUtc DESC";
        
        await using var cmd = new MySqlCommand(sql, connection);
        
        await using var reader = await cmd.ExecuteReaderAsync();
        
        while (await reader.ReadAsync())
        {
            notifications.Add(new Notification
            {
                Id = reader.GetInt32("Id"),
                ClaveId = reader.IsDBNull(reader.GetOrdinal("ClaveId")) ? null : reader.GetInt32("ClaveId"),
                SolicitudId = reader.IsDBNull(reader.GetOrdinal("SolicitudId")) ? null : reader.GetInt32("SolicitudId"),
                NombreProceso = reader.IsDBNull(reader.GetOrdinal("NombreProceso")) ? null : reader.GetString("NombreProceso"),
                Titulo = reader.IsDBNull(reader.GetOrdinal("Titulo")) ? null : reader.GetString("Titulo"),
                Mensaje = reader.IsDBNull(reader.GetOrdinal("Mensaje")) ? null : reader.GetString("Mensaje"),
                NotificationTipo = reader.IsDBNull(reader.GetOrdinal("NotificationTipo")) ? null : reader.GetString("NotificationTipo"),
                IsAcknowledged = reader.GetBoolean("IsAcknowledged"),
                RelativeNotifiedDateAndTime = reader.IsDBNull(reader.GetOrdinal("RelativeNotifiedDateAndTime")) ? null : reader.GetString("RelativeNotifiedDateAndTime"),
                CreatedOnUtc = reader.GetDateTime("CreatedOnUtc"),
                Estado = reader.IsDBNull(reader.GetOrdinal("Estado")) ? null : reader.GetInt32("Estado"),
                Usuario = reader.IsDBNull(reader.GetOrdinal("usuario")) ? null : reader.GetString("usuario"),
                Prioridad = reader.IsDBNull(reader.GetOrdinal("prioridad")) ? null : reader.GetInt32("prioridad")
            });
        }
        
        return notifications;
    }

    public async Task<List<Notification>> GetSystemNotificationsAsync()
    {
        var notifications = new List<Notification>();
        
        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();
        
        // Obtener notificaciones del usuario 'system' con estado 0
        var sql = @"SELECT Id, ClaveId, SolicitudId, NombreProceso, Titulo, Mensaje, 
                           NotificationTipo, IsAcknowledged, RelativeNotifiedDateAndTime, 
                           CreatedOnUtc, Estado, usuario, prioridad
                    FROM notifications 
                    WHERE usuario = 'system' AND Estado = 0
                    ORDER BY CreatedOnUtc DESC";
        
        await using var cmd = new MySqlCommand(sql, connection);
        
        await using var reader = await cmd.ExecuteReaderAsync();
        
        while (await reader.ReadAsync())
        {
            notifications.Add(new Notification
            {
                Id = reader.GetInt32("Id"),
                ClaveId = reader.IsDBNull(reader.GetOrdinal("ClaveId")) ? null : reader.GetInt32("ClaveId"),
                SolicitudId = reader.IsDBNull(reader.GetOrdinal("SolicitudId")) ? null : reader.GetInt32("SolicitudId"),
                NombreProceso = reader.IsDBNull(reader.GetOrdinal("NombreProceso")) ? null : reader.GetString("NombreProceso"),
                Titulo = reader.IsDBNull(reader.GetOrdinal("Titulo")) ? null : reader.GetString("Titulo"),
                Mensaje = reader.IsDBNull(reader.GetOrdinal("Mensaje")) ? null : reader.GetString("Mensaje"),
                NotificationTipo = reader.IsDBNull(reader.GetOrdinal("NotificationTipo")) ? null : reader.GetString("NotificationTipo"),
                IsAcknowledged = reader.GetBoolean("IsAcknowledged"),
                RelativeNotifiedDateAndTime = reader.IsDBNull(reader.GetOrdinal("RelativeNotifiedDateAndTime")) ? null : reader.GetString("RelativeNotifiedDateAndTime"),
                CreatedOnUtc = reader.GetDateTime("CreatedOnUtc"),
                Estado = reader.IsDBNull(reader.GetOrdinal("Estado")) ? null : reader.GetInt32("Estado"),
                Usuario = reader.IsDBNull(reader.GetOrdinal("usuario")) ? null : reader.GetString("usuario"),
                Prioridad = reader.IsDBNull(reader.GetOrdinal("prioridad")) ? null : reader.GetInt32("prioridad")
            });
        }
        
        return notifications;
    }

    public async Task MarkAsAcknowledgedAsync(int notificationId)
    {
        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();
        
        var sql = "UPDATE notifications SET IsAcknowledged = 1 WHERE Id = @Id";
        
        await using var cmd = new MySqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("@Id", notificationId);
        
        await cmd.ExecuteNonQueryAsync();
    }

    public async Task MarkNotificationAsSentAsync(int notificationId)
    {
        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();
        
        // Actualizar el estado a 1 (enviado/atendido) para que no se vuelva a enviar
        var sql = "UPDATE notifications SET Estado = 1 WHERE Id = @Id";
        
        await using var cmd = new MySqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("@Id", notificationId);
        
        await cmd.ExecuteNonQueryAsync();
    }
}
