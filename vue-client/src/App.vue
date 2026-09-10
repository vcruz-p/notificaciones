<template>
  <div class="app">
    <h1>🔔 Cliente de Notificaciones SignalR</h1>
    
    <div class="connection-section">
      <div class="form-group">
        <label for="username">Usuario:</label>
        <input 
          type="text" 
          id="username" 
          v-model="username" 
          placeholder="Ingresa tu nombre de usuario"
        />
      </div>
      
      <button @click="connect" :disabled="isConnected || !username">
        {{ isConnected ? '✅ Conectado' : '🔌 Conectar' }}
      </button>
      
      <button @click="disconnect" :disabled="!isConnected" class="disconnect-btn">
        Desconectar
      </button>
    </div>

    <div v-if="connectionError" class="error">
      ❌ {{ connectionError }}
    </div>

    <div v-if="isConnected" class="status">
      <p>Estado: <strong>Conectado</strong></p>
      <p>Usuario: <strong>{{ username }}</strong></p>
    </div>

    <div class="notifications-section">
      <h2>Notificaciones ({{ notifications.length }})</h2>
      
      <button @click="clearNotifications" :disabled="notifications.length === 0">
        Limpiar todas
      </button>

      <div v-if="notifications.length === 0" class="no-notifications">
        No hay notificaciones pendientes
      </div>

      <div v-else class="notifications-list">
        <div 
          v-for="notification in notifications" 
          :key="notification.id"
          class="notification-card"
          :class="{ acknowledged: notification.isAcknowledged }"
        >
          <div class="notification-header">
            <span class="notification-title">{{ notification.titulo || 'Sin título' }}</span>
            <span class="notification-badge" :class="getPriorityClass(notification.prioridad)">
              {{ getPriorityText(notification.prioridad) }}
            </span>
          </div>
          
          <p class="notification-message">{{ notification.mensaje }}</p>
          
          <div class="notification-footer">
            <span class="notification-date">
              {{ formatDate(notification.createdOnUtc) }}
            </span>
            
            <button 
              v-if="!notification.isAcknowledged"
              @click="acknowledgeNotification(notification.id)"
              class="ack-btn"
            >
              Marcar como leída
            </button>
            
            <span v-else class="acknowledged-badge">✓ Leída</span>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onUnmounted } from 'vue';
import { useSignalR } from './composables/useSignalR';

const username = ref<string>('');
const { 
  connection, 
  connectionError, 
  notifications, 
  connectToSignalR, 
  disconnectFromSignalR,
  acknowledgeNotification,
  clearNotifications
} = useSignalR(username.value);

const isConnected = computed(() => connection.value?.state === 1);

const connect = () => {
  if (username.value.trim()) {
    // Actualizar el username en el composable antes de conectar
    connectToSignalR();
  }
};

const disconnect = async () => {
  await disconnectFromSignalR();
};

const getPriorityClass = (priority: number | null): string => {
  if (priority === null) return 'priority-normal';
  if (priority >= 8) return 'priority-high';
  if (priority >= 5) return 'priority-medium';
  return 'priority-low';
};

const getPriorityText = (priority: number | null): string => {
  if (priority === null) return 'Normal';
  if (priority >= 8) return 'Alta';
  if (priority >= 5) return 'Media';
  return 'Baja';
};

const formatDate = (date: Date | string): string => {
  const d = new Date(date);
  return d.toLocaleString('es-ES');
};

onUnmounted(() => {
  disconnectFromSignalR();
});
</script>

<style>
* {
  box-sizing: border-box;
  margin: 0;
  padding: 0;
}

body {
  font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Oxygen, Ubuntu, sans-serif;
  background-color: #f5f5f5;
  color: #333;
  line-height: 1.6;
}

.app {
  max-width: 800px;
  margin: 0 auto;
  padding: 2rem;
}

h1 {
  text-align: center;
  margin-bottom: 2rem;
  color: #2c3e50;
}

h2 {
  margin-bottom: 1rem;
  color: #2c3e50;
}

.connection-section {
  background: white;
  padding: 1.5rem;
  border-radius: 8px;
  box-shadow: 0 2px 4px rgba(0,0,0,0.1);
  margin-bottom: 2rem;
  display: flex;
  gap: 1rem;
  align-items: center;
  flex-wrap: wrap;
}

.form-group {
  display: flex;
  align-items: center;
  gap: 0.5rem;
}

.form-group label {
  font-weight: 600;
}

.form-group input {
  padding: 0.5rem 1rem;
  border: 1px solid #ddd;
  border-radius: 4px;
  font-size: 1rem;
  min-width: 200px;
}

button {
  padding: 0.5rem 1.5rem;
  background-color: #3498db;
  color: white;
  border: none;
  border-radius: 4px;
  cursor: pointer;
  font-size: 1rem;
  transition: background-color 0.2s;
}

button:hover:not(:disabled) {
  background-color: #2980b9;
}

button:disabled {
  background-color: #bdc3c7;
  cursor: not-allowed;
}

.disconnect-btn {
  background-color: #e74c3c;
}

.disconnect-btn:hover:not(:disabled) {
  background-color: #c0392b;
}

.error {
  background-color: #fee;
  color: #c0392b;
  padding: 1rem;
  border-radius: 4px;
  margin-bottom: 1rem;
  border-left: 4px solid #e74c3c;
}

.status {
  background-color: #e8f8f5;
  color: #27ae60;
  padding: 1rem;
  border-radius: 4px;
  margin-bottom: 1rem;
  border-left: 4px solid #2ecc71;
}

.notifications-section {
  background: white;
  padding: 1.5rem;
  border-radius: 8px;
  box-shadow: 0 2px 4px rgba(0,0,0,0.1);
}

.no-notifications {
  text-align: center;
  color: #7f8c8d;
  padding: 2rem;
  font-style: italic;
}

.notifications-list {
  display: flex;
  flex-direction: column;
  gap: 1rem;
}

.notification-card {
  background: #f8f9fa;
  border: 1px solid #e9ecef;
  border-radius: 8px;
  padding: 1rem;
  transition: all 0.2s;
}

.notification-card.acknowledged {
  opacity: 0.6;
  background: #f0f0f0;
}

.notification-card:hover:not(.acknowledged) {
  box-shadow: 0 4px 8px rgba(0,0,0,0.1);
}

.notification-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 0.5rem;
}

.notification-title {
  font-weight: 600;
  font-size: 1.1rem;
  color: #2c3e50;
}

.notification-badge {
  padding: 0.25rem 0.75rem;
  border-radius: 20px;
  font-size: 0.8rem;
  font-weight: 600;
}

.priority-high {
  background-color: #fee;
  color: #c0392b;
}

.priority-medium {
  background-color: #fff3cd;
  color: #856404;
}

.priority-low, .priority-normal {
  background-color: #d4edda;
  color: #155724;
}

.notification-message {
  color: #555;
  margin-bottom: 1rem;
}

.notification-footer {
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.notification-date {
  font-size: 0.85rem;
  color: #7f8c8d;
}

.ack-btn {
  padding: 0.35rem 1rem;
  font-size: 0.9rem;
  background-color: #27ae60;
}

.ack-btn:hover {
  background-color: #219a52;
}

.acknowledged-badge {
  color: #27ae60;
  font-weight: 600;
}
</style>
