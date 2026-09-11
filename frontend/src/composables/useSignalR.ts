import { ref } from 'vue';
import { HubConnection, HubConnectionBuilder } from '@microsoft/signalr';
import { NotificationDto } from './NotificationDto';

export function useSignalR(user: string) {
  const connection = ref<HubConnection | null>(null);
  const connectionError = ref<string | null>(null);
  const notifications = ref<NotificationDto[]>([]);

  // Accede a la variable de entorno usando `import.meta.env`
  const signalRUrl = import.meta.env.VITE_APP_SIGNALR_URL;

  // Validar si la URL está configurada correctamente
  if (!signalRUrl) {
    throw new Error('La URL del servidor SignalR no está configurada en las variables de entorno.');
  }

  // Conectar al Hub de SignalR
  const connectToSignalR = () => {
    connection.value = new HubConnectionBuilder()
      .withUrl(`${signalRUrl}/notifications?username=${user}`)
      .withAutomaticReconnect()
      .build();

    connection.value.on('ClientReceiveNotification', (notification: NotificationDto) => {
      console.log('Notificación recibida:', notification);
      const notificationExists = notifications.value.some(n => n.id === notification.id);
      if (!notificationExists) {
        notifications.value.push(notification);
      }
    });

    connection.value.start()
      .then(() => {
        console.log('Conectado a SignalR');
        connectionError.value = null;
      })
      .catch(err => {
        console.error('Error de conexión SignalR: ', err);
        connectionError.value = `Error de conexión: ${err.message}`;
      });
  };

  // Desconectar del Hub de SignalR
  const disconnectFromSignalR = () => {
    if (connection.value) {
      connection.value.stop();
      console.log('Desconectado de SignalR');
    }
  };

  return {
    connection,
    connectionError,
    notifications,
    connectToSignalR,
    disconnectFromSignalR
  };
}
