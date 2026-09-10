using NotificationHub.Data;
using NotificationHub.Hubs;
using NotificationHub.Services;

var builder = WebApplication.CreateBuilder(args);

// Configurar conexión a MySQL - Registrar el repositorio como singleton
builder.Services.AddSingleton<INotificationRepository, NotificationRepository>();
builder.Services.AddScoped<INotificationService, NotificationService>();

// Agregar SignalR y registrar el Hub para poder inyectar dependencias
builder.Services.AddSignalR().AddHubOptions<NotificationHubClass>(options =>
{
    options.EnableDetailedErrors = true;
});

// Registrar el worker service para enviar notificaciones pendientes
builder.Services.AddHostedService<NotificationWorkerService>();

// Configurar CORS para permitir conexiones desde el cliente Vue
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowVueClient", policy =>
    {
        policy.SetIsOriginAllowed(_ => true) // Permitir cualquier origen
              .AllowAnyHeader()
              .AllowAnyMethod();
              // No usar AllowCredentials() con origen comodín
    });
});

var app = builder.Build();

// Usar CORS
app.UseCors("AllowVueClient");

// Mapear el Hub de SignalR
app.MapHub<NotificationHubClass>("/notifications");

// Endpoint para obtener notificaciones pendientes
app.MapGet("/api/notifications/{username}", async (string username, INotificationService service) =>
{
    var notifications = await service.GetPendingNotificationsAsync(username);
    return Results.Ok(notifications);
});

// Endpoint para marcar notificación como leída
app.MapPost("/api/notifications/{id}/acknowledge", async (int id, INotificationRepository repository) =>
{
    await repository.MarkAsAcknowledgedAsync(id);
    return Results.Ok();
});

app.MapGet("/", () => "SignalR Notification Hub está funcionando!");

app.Run();
