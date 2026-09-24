import { apiRequest } from '../../shared/api/client'
import type { Id, UserNotification } from '../../shared/types/api'
export function getNotifications(signal?: AbortSignal) { return apiRequest<UserNotification[]>('/api/notification', { signal }) }
export function markNotificationRead(id: Id) { return apiRequest<void>(`/api/notification/${id}/read`, { method: 'POST' }) }
