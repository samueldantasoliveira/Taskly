import { ProjectStatus, TodoStatus, type Project, type TodoTask } from '../types/api'

const taskStatus = {
  [TodoStatus.Todo]: ['A fazer', 'neutral'],
  [TodoStatus.InProgress]: ['Em andamento', 'info'],
  [TodoStatus.Done]: ['Concluída', 'success'],
  [TodoStatus.Cancelled]: ['Cancelada', 'danger'],
} as const

const projectStatus = {
  [ProjectStatus.Active]: ['Ativo', 'success'],
  [ProjectStatus.Inactive]: ['Inativo', 'neutral'],
  [ProjectStatus.Completed]: ['Concluído', 'info'],
  [ProjectStatus.PendingApproval]: ['Aguardando', 'warning'],
} as const

export function TaskStatusBadge({ status }: Pick<TodoTask, 'status'>) {
  const [label, tone] = taskStatus[status] ?? ['Desconhecido', 'neutral']
  return <span className={`badge badge--${tone}`}>{label}</span>
}

export function ProjectStatusBadge({ status }: Pick<Project, 'status'>) {
  const [label, tone] = projectStatus[status] ?? ['Desconhecido', 'neutral']
  return <span className={`badge badge--${tone}`}>{label}</span>
}
