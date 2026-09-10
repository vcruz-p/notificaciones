import { ref } from 'vue';
import { HubConnection, HubConnectionBuilder } from '@microsoft/signalr';
import { NotificationDto } from '../types/NotificationDto';

export function useSignalR(user: string) {
  const connection = ref<HubConnection | null>(null);
  const connectionError = ref<string | null>(null);
  const notifications = ref<NotificationDto[]>([]);

  // URL del servidor SignalR - ajustar según tu entorno
  const signalRUrl = import.meta.env.VITE_APP_SIGNALR_URL || 'http://localhost:5000';

  // Conectar al Hub de SignalR
  const connectToSignalR = () => {
    if (connection.value?.state === 1) { // Already connected
      console.log('Ya está conectado a SignalR');
      return;
    }

    connection.value = new HubConnectionBuilder()
      .withUrl(`${signalRUrl}/notifications?username=${encodeURIComponent(user)}`, {
        // Configurar transporte si es necesario
        // transport: HttpTransportType.WebSockets
      })
      .withAutomaticReconnect([0, 2000, 5000, 10000, 30000]) // Reintentos de conexión
      .build();

    // Suscribirse al evento de notificación
    connection.value.on('ClientReceiveNotification', (notification: NotificationDto) => {
      console.log('Notificación recibida:', notification);
      
      // Verificar si la notificación ya existe para evitar duplicados
      const notificationExists = notifications.value.some(n => n.id === notification.id);
      if (!notificationExists) {
        notifications.value.push(notification);
      }
    });

    // Manejar errores de conexión
    connection.value.onclose((error?: Error) => {
      console.warn('Conexión SignalR cerrada:', error);
      connectionError.value = error ? `Conexión cerrada: ${error.message}` : 'Conexión cerrada';
    });

    // Manejar reconexiones fallidas
    connection.value.onreconnecting((error?: Error) => {
      console.warn('Reconectando a SignalR:', error);
      connectionError.value = 'Reconectando...';
    });

    connection.value.onreconnected((connectionId?: string) => {
      console.log('Reconectado a SignalR con connectionId:', connectionId);
      connectionError.value = null;
    });

    // Iniciar conexión
    connection.value.start()
      .then(() => {
        console.log('✅ Conectado a SignalR');
        connectionError.value = null;
      })
      .catch(err => {
        console.error('❌ Error de conexión SignalR:', err);
        connectionError.value = `Error de conexión: ${err.message}`;
      });
  };

  // Desconectar del Hub de SignalR
  const disconnectFromSignalR = async () => {
    if (connection.value) {
      try {
        await connection.value.stop();
        console.log('Desconectado de SignalR');
        connection.value = null;
        connectionError.value = null;
      } catch (err) {
        console.error('Error al desconectar:', err);
      }
    }
  };

  // Marcar notificación como leída
  const acknowledgeNotification = async (notificationId: number) => {
    const index = notifications.value.findIndex(n => n.id === notificationId);
    if (index !== -1) {
      notifications.value[index].isAcknowledged = true;
      
      // Llamar al endpoint para marcar como leída en el servidor
      try {
        await fetch(`${signalRUrl}/api/notifications/${notificationId}/acknowledge`, {
          method: 'POST',
        });
      } catch (err) {
        console.error('Error al marcar notificación como leída:', err);
      }
    }
  };

  // Limpiar notificaciones
  const clearNotifications = () => {
    notifications.value = [];
  };

  return {
    connection,
    connectionError,
    notifications,
    connectToSignalR,
    disconnectFromSignalR,
    acknowledgeNotification,
    clearNotifications
  };
}
