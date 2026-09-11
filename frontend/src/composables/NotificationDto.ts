export interface NotificationDto {
  id: number;
  userId: string;
  message: string;
  type: string;
  isRead: boolean;
  createdAt: Date;
}
