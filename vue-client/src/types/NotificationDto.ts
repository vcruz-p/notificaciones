export interface NotificationDto {
  id: number;
  claveId: number | null;
  solicitudId: number | null;
  nombreProceso: string | null;
  titulo: string | null;
  mensaje: string | null;
  notificationTipo: string | null;
  isAcknowledged: boolean;
  relativeNotifiedDateAndTime: string | null;
  createdOnUtc: Date;
  estado: number | null;
  usuario: string | null;
  prioridad: number | null;
}
