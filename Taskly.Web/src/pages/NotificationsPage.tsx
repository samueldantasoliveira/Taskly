import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { Bell } from 'lucide-react'
import { useNavigate } from 'react-router'
import { getNotifications, markNotificationRead } from '../features/notifications/api'
import { EmptyState, ErrorState, PageLoader } from '../shared/components/Feedback'
import { queryKeys } from '../shared/lib/query-keys'

export function NotificationsPage() {
  const navigate = useNavigate(); const client = useQueryClient()
  const query = useQuery({ queryKey: queryKeys.notifications, queryFn: ({ signal }) => getNotifications(signal) })
  const read = useMutation({ mutationFn: markNotificationRead, onSuccess: () => client.invalidateQueries({ queryKey: queryKeys.notifications }) })
  if (query.isPending) return <PageLoader label="Carregando notificações..." />
  if (query.isError) return <ErrorState message={(query.error as Error).message} onRetry={() => query.refetch()} />
  return <div className="page-stack"><section className="welcome-banner"><div><span className="eyebrow">Atualizações</span><h1>Notificações</h1><p>Atribuições e conversas que precisam da sua atenção.</p></div></section>{query.data?.length === 0 ? <EmptyState title="Nenhuma notificação" description="As novidades do seu trabalho aparecerão aqui." /> : <div className="notification-list">{query.data?.map(item => <button key={item.id} className={item.readAt ? 'notification-item' : 'notification-item notification-item--unread'} onClick={async () => { if (!item.readAt) await read.mutateAsync(item.id); navigate(item.link) }}><Bell size={18} /><span><strong>{item.message}</strong><small>{new Date(item.createdAt).toLocaleString('pt-BR')}</small></span></button>)}</div>}</div>
}
