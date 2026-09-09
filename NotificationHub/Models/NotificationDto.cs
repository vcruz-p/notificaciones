namespace NotificationHub.Models;

public class NotificationDto
{
    public int Id { get; set; }
    public int? ClaveId { get; set; }
    public int? SolicitudId { get; set; }
    public string? NombreProceso { get; set; }
    public string? Titulo { get; set; }
    public string? Mensaje { get; set; }
    public string? NotificationTipo { get; set; }
    public bool IsAcknowledged { get; set; }
    public string? RelativeNotifiedDateAndTime { get; set; }
    public DateTime CreatedOnUtc { get; set; }
    public int? Estado { get; set; }
    public string? Usuario { get; set; }
    public int? Prioridad { get; set; }
}
