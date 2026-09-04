import { zodResolver } from '@hookform/resolvers/zod'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { ArrowLeft, Check, Circle, CircleStop, Clock3, MoreHorizontal, Pencil, Play, Plus, Search, Trash2, UserRound } from 'lucide-react'
import { useEffect, useMemo, useState } from 'react'
import { useForm } from 'react-hook-form'
import { Link, useNavigate, useParams } from 'react-router'
import { z } from 'zod'
import { useAuth } from '../features/auth/auth-context'
import { deleteProject, getProject, updateProject } from '../features/projects/api'
import { assignTask, cancelTask, completeTask, createTask, deleteTask, getProjectTasks, startTask, updateTask } from '../features/tasks/api'
import { getTeam, getTeamMembers } from '../features/teams/api'
import { ApiError } from '../shared/api/client'
import { Avatar } from '../shared/components/Avatar'
import { Button } from '../shared/components/Button'
import { ConfirmDialog } from '../shared/components/ConfirmDialog'
import { EmptyState, ErrorState, PageLoader } from '../shared/components/Feedback'
import { Field, Input, Textarea } from '../shared/components/Field'
import { Modal } from '../shared/components/Modal'
import { ProjectStatusBadge, TaskStatusBadge } from '../shared/components/StatusBadge'
import { useToast } from '../shared/components/toast-context'
import { queryKeys } from '../shared/lib/query-keys'
import { ProjectStatus, TodoStatus, type TodoTask } from '../shared/types/api'

const taskSchema = z.object({
  title: z.string().trim().min(1, 'Informe um título.').max(100),
  description: z.string().trim().max(500),
  assignedUserId: z.string(),
})
type TaskFormData = z.infer<typeof taskSchema>

const projectSchema = z.object({
  name: z.string().trim().min(2, 'Informe um nome.'),
  description: z.string().trim().min(1, 'Informe uma descrição.'),
  status: z.number(),
})
type ProjectFormData = z.infer<typeof projectSchema>

const columns = [
  { status: TodoStatus.Todo, label: 'A fazer', icon: Circle },
  { status: TodoStatus.InProgress, label: 'Em andamento', icon: Clock3 },
  { status: TodoStatus.Done, label: 'Concluídas', icon: Check },
  { status: TodoStatus.Cancelled, label: 'Canceladas', icon: CircleStop },
]

export function ProjectPage() {
  const { projectId = '' } = useParams()
  const { user } = useAuth()
  const navigate = useNavigate()
  const queryClient = useQueryClient()
  const { showToast } = useToast()
  const [modal, setModal] = useState<'create' | 'edit-task' | 'edit-project' | null>(null)
  const [selectedTask, setSelectedTask] = useState<TodoTask | null>(null)
  const [deleteTarget, setDeleteTarget] = useState<'project' | TodoTask | null>(null)
  const [search, setSearch] = useState('')
  const taskForm = useForm<TaskFormData>({ resolver: zodResolver(taskSchema), defaultValues: { title: '', description: '', assignedUserId: '' } })
  const projectForm = useForm<ProjectFormData>({ resolver: zodResolver(projectSchema) })

  const projectQuery = useQuery({ queryKey: queryKeys.project(projectId), queryFn: ({ signal }) => getProject(projectId, signal), enabled: Boolean(projectId) })
  const teamId = projectQuery.data?.teamId ?? ''
  const teamQuery = useQuery({ queryKey: queryKeys.team(teamId), queryFn: ({ signal }) => getTeam(teamId, signal), enabled: Boolean(teamId) })
  const membersQuery = useQuery({ queryKey: queryKeys.members(teamId), queryFn: ({ signal }) => getTeamMembers(teamId, signal), enabled: Boolean(teamId) })
  const tasksQuery = useQuery({ queryKey: queryKeys.tasks(projectId), queryFn: ({ signal }) => getProjectTasks(projectId, signal), enabled: Boolean(projectId) })
  const canManageProject = projectQuery.data?.ownerId === user?.id || teamQuery.data?.ownerId === user?.id

  useEffect(() => {
    if (projectQuery.data) projectForm.reset({ name: projectQuery.data.name, description: projectQuery.data.description, status: projectQuery.data.status })
  }, [projectForm, projectQuery.data])

  const refreshTasks = () => queryClient.invalidateQueries({ queryKey: queryKeys.tasks(projectId) })
  const createMutation = useMutation({ mutationFn: (data: TaskFormData) => createTask({ title: data.title, description: data.description, projectId, assignedUserId: data.assignedUserId || null }), onSuccess: () => { refreshTasks(); showToast('Tarefa criada.'); taskForm.reset(); setModal(null) } })
  const editTaskMutation = useMutation({
    mutationFn: async ({ task, data }: { task: TodoTask; data: TaskFormData }) => {
      await updateTask(task.id, { title: data.title, description: data.description })
      const nextAssigned = data.assignedUserId || null
      if (nextAssigned !== task.assignedUserId) await assignTask(task.id, nextAssigned)
    },
    onSuccess: () => { refreshTasks(); showToast('Tarefa atualizada.'); setModal(null); setSelectedTask(null) },
  })
  const actionMutation = useMutation({
    mutationFn: ({ taskId, action }: { taskId: string; action: 'start' | 'complete' | 'cancel' }) => action === 'start' ? startTask(taskId) : action === 'complete' ? completeTask(taskId) : cancelTask(taskId),
    onSuccess: () => { refreshTasks(); showToast('Status da tarefa atualizado.') },
    onError: (error) => showToast(error instanceof ApiError ? error.message : 'Não foi possível alterar a tarefa.', 'error'),
  })
  const editProjectMutation = useMutation({ mutationFn: (data: ProjectFormData) => updateProject(projectId, { ...data, status: data.status as ProjectStatus }), onSuccess: () => { queryClient.invalidateQueries({ queryKey: queryKeys.project(projectId) }); queryClient.invalidateQueries({ queryKey: queryKeys.projects(teamId) }); showToast('Projeto atualizado.'); setModal(null) }, onError: (error) => showToast(error instanceof ApiError ? error.message : 'Não foi possível atualizar o projeto.', 'error') })
  const deleteMutation = useMutation({
    mutationFn: () => deleteTarget === 'project' ? deleteProject(projectId) : deleteTarget ? deleteTask(deleteTarget.id) : Promise.resolve(),
    onSuccess: () => {
      if (deleteTarget === 'project') { queryClient.invalidateQueries({ queryKey: queryKeys.projects(teamId) }); showToast('Projeto excluído.'); navigate(`/teams/${teamId}`) }
      else { refreshTasks(); showToast('Tarefa excluída.'); setDeleteTarget(null) }
    },
    onError: (error) => showToast(error instanceof ApiError ? error.message : 'Não foi possível excluir.', 'error'),
  })

  const filteredTasks = useMemo(() => {
    const term = search.trim().toLowerCase()
    if (!term) return tasksQuery.data ?? []
    return (tasksQuery.data ?? []).filter((task) => task.title.toLowerCase().includes(term) || task.description?.toLowerCase().includes(term))
  }, [search, tasksQuery.data])

  const openEditTask = (task: TodoTask) => {
    setSelectedTask(task)
    taskForm.reset({ title: task.title, description: task.description ?? '', assignedUserId: task.assignedUserId ?? '' })
    setModal('edit-task')
  }
  const openCreateTask = () => { setSelectedTask(null); taskForm.reset({ title: '', description: '', assignedUserId: '' }); setModal('create') }

  if (projectQuery.isPending) return <PageLoader label="Abrindo o projeto..." />
  if (projectQuery.isError) return <ErrorState message={(projectQuery.error as Error).message} onRetry={() => projectQuery.refetch()} />
  const project = projectQuery.data
  const activeProject = project.status === ProjectStatus.Active
  const selectedTaskIsReadOnly = selectedTask?.status === TodoStatus.Done || selectedTask?.status === TodoStatus.Cancelled

  return (
    <div className="page-stack page-stack--wide">
      <Link className="back-link" to={`/teams/${project.teamId}`}><ArrowLeft size={16} /> Voltar para a equipe</Link>
      <section className="project-hero">
        <div><div className="project-hero__title"><h1>{project.name}</h1><ProjectStatusBadge status={project.status} /></div><p>{project.description}</p></div>
        <div className="project-hero__actions">{canManageProject && <Button variant="secondary" icon={<Pencil size={16} />} onClick={() => setModal('edit-project')}>Editar projeto</Button>}<Button icon={<Plus size={17} />} onClick={openCreateTask} disabled={!activeProject}>Nova tarefa</Button></div>
      </section>

      <div className="board-toolbar"><div className="search-input"><Search size={17} /><input value={search} onChange={(event) => setSearch(event.target.value)} placeholder="Buscar tarefas..." aria-label="Buscar tarefas" /></div><span>{filteredTasks.length} {filteredTasks.length === 1 ? 'tarefa' : 'tarefas'}</span></div>

      {tasksQuery.isPending && <PageLoader label="Montando o quadro..." />}
      {tasksQuery.isError && <ErrorState message={(tasksQuery.error as Error).message} onRetry={() => tasksQuery.refetch()} />}
      {tasksQuery.data?.length === 0 && <EmptyState title="O quadro está vazio" description="Crie a primeira tarefa para dar forma ao trabalho deste projeto." action={<Button icon={<Plus size={16} />} onClick={openCreateTask} disabled={!activeProject}>Criar tarefa</Button>} />}

      {Boolean(tasksQuery.data?.length) && <div className="kanban-board">
        {columns.map((column) => {
          const columnTasks = filteredTasks.filter((task) => task.status === column.status)
          const Icon = column.icon
          return <section className="kanban-column" key={column.status}>
            <header><span><Icon size={16} /> {column.label}</span><small>{columnTasks.length}</small></header>
            <div className="kanban-column__body">
              {columnTasks.map((task) => {
                const assignedMember = membersQuery.data?.find((member) => member.id === task.assignedUserId)
                const isAssigned = task.assignedUserId === user?.id
                return <article className="task-card" key={task.id}>
                  <div className="task-card__top"><TaskStatusBadge status={task.status} /><button className="icon-button" onClick={() => openEditTask(task)} aria-label={`${task.status === TodoStatus.Done || task.status === TodoStatus.Cancelled ? 'Ver' : 'Editar'} ${task.title}`}><MoreHorizontal size={18} /></button></div>
                  <h3>{task.title}</h3><p>{task.description || 'Sem descrição.'}</p>
                  <div className="task-card__assignee">{assignedMember ? <><Avatar name={assignedMember.name} size="sm" /><span>{assignedMember.name}</span></> : <><span className="unassigned"><UserRound size={14} /></span><span>Sem responsável</span></>}</div>
                  {isAssigned && task.status === TodoStatus.Todo && <div className="task-card__actions"><Button size="sm" variant="ghost" onClick={() => actionMutation.mutate({ taskId: task.id, action: 'cancel' })}>Cancelar</Button><Button size="sm" variant="secondary" icon={<Play size={14} />} loading={actionMutation.isPending && actionMutation.variables?.taskId === task.id} onClick={() => actionMutation.mutate({ taskId: task.id, action: 'start' })}>Iniciar</Button></div>}
                  {isAssigned && task.status === TodoStatus.InProgress && <div className="task-card__actions"><Button size="sm" variant="ghost" onClick={() => actionMutation.mutate({ taskId: task.id, action: 'cancel' })}>Cancelar</Button><Button size="sm" icon={<Check size={14} />} onClick={() => actionMutation.mutate({ taskId: task.id, action: 'complete' })}>Concluir</Button></div>}
                </article>
              })}
              {!columnTasks.length && <div className="column-empty">Nenhuma tarefa</div>}
            </div>
          </section>
        })}
      </div>}

      <Modal open={modal === 'create' || modal === 'edit-task'} title={modal === 'create' ? 'Nova tarefa' : selectedTaskIsReadOnly ? 'Detalhes da tarefa' : 'Editar tarefa'} description={selectedTaskIsReadOnly ? 'Tarefas concluídas ou canceladas não podem mais ser alteradas.' : 'Mantenha o próximo passo claro e objetivo.'} onClose={() => setModal(null)}>
        <form onSubmit={taskForm.handleSubmit((data) => selectedTask ? !selectedTaskIsReadOnly && editTaskMutation.mutate({ task: selectedTask, data }) : createMutation.mutate(data))}>
          <Field label="Título" htmlFor="task-title" error={taskForm.formState.errors.title?.message}><Input id="task-title" autoFocus maxLength={100} disabled={selectedTaskIsReadOnly} placeholder="O que precisa ser feito?" {...taskForm.register('title')} /></Field>
          <Field label="Descrição" htmlFor="task-description" error={taskForm.formState.errors.description?.message}><Textarea id="task-description" rows={4} maxLength={500} disabled={selectedTaskIsReadOnly} placeholder="Adicione contexto e critérios de conclusão." {...taskForm.register('description')} /></Field>
          <Field label="Responsável" htmlFor="task-assignee"><select id="task-assignee" className="input" disabled={selectedTaskIsReadOnly} {...taskForm.register('assignedUserId')}><option value="">Sem responsável</option>{membersQuery.data?.map((member) => <option value={member.id} key={member.id}>{member.name}</option>)}</select></Field>
          {(createMutation.isError || editTaskMutation.isError) && <div className="form-alert">{((createMutation.error || editTaskMutation.error) as Error).message}</div>}
          <div className="modal__actions modal__actions--split">{selectedTask ? <Button type="button" variant="danger" icon={<Trash2 size={15} />} onClick={() => { setModal(null); setDeleteTarget(selectedTask) }}>Excluir</Button> : <span />}<div><Button type="button" variant="secondary" onClick={() => setModal(null)}>{selectedTaskIsReadOnly ? 'Fechar' : 'Cancelar'}</Button>{!selectedTaskIsReadOnly && <Button type="submit" loading={createMutation.isPending || editTaskMutation.isPending}>{selectedTask ? 'Salvar' : 'Criar tarefa'}</Button>}</div></div>
        </form>
      </Modal>

      <Modal open={modal === 'edit-project'} title="Editar projeto" onClose={() => setModal(null)}>
        <form onSubmit={projectForm.handleSubmit((data) => editProjectMutation.mutate(data))}><Field label="Nome" htmlFor="edit-project-name" error={projectForm.formState.errors.name?.message}><Input id="edit-project-name" {...projectForm.register('name')} /></Field><Field label="Descrição" htmlFor="edit-project-description" error={projectForm.formState.errors.description?.message}><Textarea id="edit-project-description" rows={4} {...projectForm.register('description')} /></Field><Field label="Status" htmlFor="project-status"><select id="project-status" className="input" {...projectForm.register('status', { valueAsNumber: true })}><option value={ProjectStatus.Active}>Ativo</option><option value={ProjectStatus.Inactive}>Inativo</option><option value={ProjectStatus.Completed}>Concluído</option><option value={ProjectStatus.PendingApproval}>Aguardando aprovação</option></select></Field><div className="modal__actions modal__actions--split"><Button type="button" variant="danger" icon={<Trash2 size={15} />} onClick={() => { setModal(null); setDeleteTarget('project') }}>Excluir projeto</Button><div><Button type="button" variant="secondary" onClick={() => setModal(null)}>Cancelar</Button><Button type="submit" loading={editProjectMutation.isPending}>Salvar</Button></div></div></form>
      </Modal>

      <ConfirmDialog open={Boolean(deleteTarget)} title={deleteTarget === 'project' ? 'Excluir este projeto?' : 'Excluir esta tarefa?'} description="Esta ação é permanente e não poderá ser desfeita." loading={deleteMutation.isPending} onClose={() => setDeleteTarget(null)} onConfirm={() => deleteMutation.mutate()} />
    </div>
  )
}
