import { useQuery } from '@tanstack/react-query'
import { CalendarClock, CircleDot, ListTodo } from 'lucide-react'
import { Link } from 'react-router'
import { getMyWork } from '../features/tasks/api'
import { TaskStatusBadge } from '../shared/components/StatusBadge'
import { EmptyState, ErrorState, PageLoader } from '../shared/components/Feedback'
import { queryKeys } from '../shared/lib/query-keys'
import { TaskPriority, type MyWorkItem } from '../shared/types/api'

const priorityLabel = {
  [TaskPriority.Low]: 'Baixa',
  [TaskPriority.Medium]: 'Média',
  [TaskPriority.High]: 'Alta',
} as const

function dueLabel(item: MyWorkItem) {
  if (!item.dueDate) return 'Sem prazo'
  const date = new Date(`${item.dueDate.slice(0, 10)}T00:00:00`)
  const overdue = item.dueDate.slice(0, 10) < new Date().toISOString().slice(0, 10)
  return `${overdue ? 'Atrasada: ' : 'Prazo: '}${date.toLocaleDateString('pt-BR')}`
}

export function MyWorkPage() {
  const dashboardQuery = useQuery({ queryKey: queryKeys.myWork, queryFn: ({ signal }) => getMyWork(signal) })
  const dashboard = dashboardQuery.data

  return (
    <div className="page-stack">
      <section className="welcome-banner">
        <div><span className="eyebrow">Visão pessoal</span><h1>Meu trabalho</h1><p>As tarefas ativas atribuídas a você, organizadas por prioridade e prazo.</p></div>
      </section>

      {dashboardQuery.isPending && <PageLoader label="Organizando suas tarefas..." />}
      {dashboardQuery.isError && <ErrorState message={(dashboardQuery.error as Error).message} onRetry={() => dashboardQuery.refetch()} />}
      {dashboard && <>
        <section className="my-work-summary" aria-label="Resumo das tarefas">
          <article><ListTodo size={20} /><span><strong>{dashboard.todoCount}</strong><small>A fazer</small></span></article>
          <article><CircleDot size={20} /><span><strong>{dashboard.inProgressCount}</strong><small>Em andamento</small></span></article>
          <article className={dashboard.overdueCount ? 'my-work-summary__overdue' : ''}><CalendarClock size={20} /><span><strong>{dashboard.overdueCount}</strong><small>Em atraso</small></span></article>
        </section>
        {dashboard.items.length === 0 ? <EmptyState title="Tudo em dia" description="Você não tem tarefas ativas atribuídas no momento." /> : (
          <section>
            <div className="section-heading"><div><h2>Suas tarefas ativas</h2><p>Somente tarefas de projetos e equipes aos quais você tem acesso.</p></div><span className="count-pill">{dashboard.items.length}</span></div>
            <div className="my-work-list">
              {dashboard.items.map(task => <Link key={task.id} to={`/projects/${task.projectId}`} className="my-work-item">
                <div className="my-work-item__main"><div className="my-work-item__title"><h3>{task.title}</h3><TaskStatusBadge status={task.status} /></div><p>{task.teamName} <span>•</span> {task.projectName}</p></div>
                <div className="my-work-item__meta"><span className={`task-priority task-priority--${task.priority}`}>{priorityLabel[task.priority]}</span><span className={task.dueDate && task.dueDate.slice(0, 10) < new Date().toISOString().slice(0, 10) ? 'task-due task-due--overdue' : 'task-due'}>{dueLabel(task)}</span></div>
              </Link>)}
            </div>
          </section>
        )}
      </>}
    </div>
  )
}
