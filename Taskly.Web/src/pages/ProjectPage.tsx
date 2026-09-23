import { zodResolver } from '@hookform/resolvers/zod'
import { useInfiniteQuery, useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { ArrowLeft, Check, Circle, CircleStop, Clock3, MoreHorizontal, Pencil, Play, Plus, RotateCcw, Search, Trash2, UserRound } from 'lucide-react'
import { useDeferredValue, useEffect, useMemo, useState } from 'react'
import { useForm } from 'react-hook-form'
import { Link, useNavigate, useParams } from 'react-router'
import { z } from 'zod'
import { useAuth } from '../features/auth/auth-context'
import { deleteProject, getProject, updateProject } from '../features/projects/api'
import {
  assignTask,
  cancelTask,
  completeTask,
  createTask,
  deleteTask,
  getAllProjectTasks,
  getProjectTasks,
  startTask,
  updateTask,
  type ProjectTaskQuery,
} from '../features/tasks/api'
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
  ownerId: z.string().uuid(),
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

const historyPageSize = 20

const sortOptions = {
  recent: { sortBy: 'CreatedAt', sortDirection: 'Descending' },
  oldest: { sortBy: 'CreatedAt', sortDirection: 'Ascending' },
  titleAscending: { sortBy: 'Title', sortDirection: 'Ascending' },
  titleDescending: { sortBy: 'Title', sortDirection: 'Descending' },
} as const satisfies Record<string, Pick<ProjectTaskQuery, 'sortBy' | 'sortDirection'>>

type SortOption = keyof typeof sortOptions

export function ProjectPage() {
  const { projectId = '' } = useParams()
  return <ProjectBoard key={projectId} projectId={projectId} />
}

function ProjectBoard({ projectId }: { projectId: string }) {
  const { user } = useAuth()
  const navigate = useNavigate()
  const queryClient = useQueryClient()
  const { showToast } = useToast()
  const [modal, setModal] = useState<'create' | 'edit-task' | 'edit-project' | null>(null)
  const [selectedTask, setSelectedTask] = useState<TodoTask | null>(null)
  const [deleteTarget, setDeleteTarget] = useState<'project' | TodoTask | null>(null)
  const [titleFilter, setTitleFilter] = useState('')
  const [assigneeId, setAssigneeId] = useState('')
  const [sortOption, setSortOption] = useState<SortOption>('recent')
  const deferredTitle = useDeferredValue(titleFilter.trim())
  const taskForm = useForm<TaskFormData>({ resolver: zodResolver(taskSchema), defaultValues: { title: '', description: '', assignedUserId: '' } })
  const projectForm = useForm<ProjectFormData>({ resolver: zodResolver(projectSchema) })

  const projectQuery = useQuery({ queryKey: queryKeys.project(projectId), queryFn: ({ signal }) => getProject(projectId, signal), enabled: Boolean(projectId) })
  const teamId = projectQuery.data?.teamId ?? ''
  const teamQuery = useQuery({ queryKey: queryKeys.team(teamId), queryFn: ({ signal }) => getTeam(teamId, signal), enabled: Boolean(teamId) })
  const membersQuery = useQuery({ queryKey: queryKeys.members(teamId), queryFn: ({ signal }) => getTeamMembers(teamId, signal), enabled: Boolean(teamId) })
  const taskFilters = useMemo(() => ({
    title: deferredTitle || undefined,
    assigneeId: assigneeId || undefined,
    ...sortOptions[sortOption],
  }), [assigneeId, deferredTitle, sortOption])
  const todoQuery = useQuery({
    queryKey: queryKeys.taskList(projectId, 'todo', taskFilters),
    queryFn: ({ signal }) => getAllProjectTasks(projectId, { ...taskFilters, status: TodoStatus.Todo }, signal),
    enabled: Boolean(projectId),
  })
  const inProgressQuery = useQuery({
    queryKey: queryKeys.taskList(projectId, 'in-progress', taskFilters),
    queryFn: ({ signal }) => getAllProjectTasks(projectId, { ...taskFilters, status: TodoStatus.InProgress }, signal),
    enabled: Boolean(projectId),
  })
  const doneQuery = useInfiniteQuery({
    queryKey: queryKeys.taskList(projectId, 'done', taskFilters),
    initialPageParam: 1,
    queryFn: ({ pageParam, signal }) => getProjectTasks(projectId, {
      ...taskFilters,
      status: TodoStatus.Done,
      page: pageParam,
      pageSize: historyPageSize,
    }, signal),
    getNextPageParam: (lastPage, pages) => {
      const loaded = pages.reduce((total, page) => total + page.items.length, 0)
      return loaded < lastPage.totalCount ? pages.length + 1 : undefined
    },
    enabled: Boolean(projectId),
  })
  const cancelledQuery = useInfiniteQuery({
    queryKey: queryKeys.taskList(projectId, 'cancelled', taskFilters),
    initialPageParam: 1,
    queryFn: ({ pageParam, signal }) => getProjectTasks(projectId, {
      ...taskFilters,
      status: TodoStatus.Cancelled,
      page: pageParam,
      pageSize: historyPageSize,
    }, signal),
    getNextPageParam: (lastPage, pages) => {
      const loaded = pages.reduce((total, page) => total + page.items.length, 0)
      return loaded < lastPage.totalCount ? pages.length + 1 : undefined
    },
    enabled: Boolean(projectId),
  })

  const canManageProject = projectQuery.data?.ownerId === user?.id || teamQuery.data?.ownerId === user?.id
  const doneTasks = doneQuery.data?.pages.flatMap((page) => page.items) ?? []
  const cancelledTasks = cancelledQuery.data?.pages.flatMap((page) => page.items) ?? []
  const boardColumns = [
    {
      ...columns[0],
      tasks: todoQuery.data?.items ?? [],
      totalCount: todoQuery.data?.totalCount ?? 0,
    },
    {
      ...columns[1],
      tasks: inProgressQuery.data?.items ?? [],
      totalCount: inProgressQuery.data?.totalCount ?? 0,
    },
    {
      ...columns[2],
      tasks: doneTasks,
      totalCount: doneQuery.data?.pages[0]?.totalCount ?? 0,
      hasNextPage: doneQuery.hasNextPage,
      isFetchingNextPage: doneQuery.isFetchingNextPage,
      loadMore: () => doneQuery.fetchNextPage(),
    },
    {
      ...columns[3],
      tasks: cancelledTasks,
      totalCount: cancelledQuery.data?.pages[0]?.totalCount ?? 0,
      hasNextPage: cancelledQuery.hasNextPage,
      isFetchingNextPage: cancelledQuery.isFetchingNextPage,
      loadMore: () => cancelledQuery.fetchNextPage(),
    },
  ]
  const taskQueries = [todoQuery, inProgressQuery, doneQuery, cancelledQuery]
  const boardIsPending = taskQueries.some((query) => query.isPending)
  const boardError = taskQueries.find((query) => query.isError)?.error
  const totalTaskCount = boardColumns.reduce((total, column) => total + column.totalCount, 0)
  const hasFilters = Boolean(deferredTitle || assigneeId)

  useEffect(() => {
    if (projectQuery.data) projectForm.reset({ name: projectQuery.data.name, description: projectQuery.data.description, status: projectQuery.data.status, ownerId: projectQuery.data.ownerId })
  }, [projectForm, projectQuery.data])

  const refreshTasks = () => queryClient.invalidateQueries({ queryKey: queryKeys.tasks(projectId) })
  const resetTaskView = () => {
    setTitleFilter('')
    setAssigneeId('')
    setSortOption('recent')
  }
  const createMutation = useMutation({ mutationFn: (data: TaskFormData) => createTask({ title: data.title, description: data.description, projectId, assignedUserId: data.assignedUserId || null }), onSuccess: () => { resetTaskView(); refreshTasks(); showToast('Tarefa criada.'); taskForm.reset(); setModal(null) } })
  const editTaskMutation = useMutation({
    mutationFn: async ({ task, data }: { task: TodoTask; data: TaskFormData }) => {
      const updated = await updateTask(task.id, { version: task.version, title: data.title, description: data.description })
      const nextAssigned = data.assignedUserId || null
      if (nextAssigned !== task.assignedUserId) {
        try {
          await assignTask(task.id, nextAssigned)
        } catch (error) {
          // Preserve the saved version so retrying does not submit a stale edit.
          setSelectedTask(updated)
          throw new Error(`Título e descrição foram salvos, mas a alteração do responsável não foi confirmada. Confira os dados antes de tentar novamente. ${error instanceof Error ? error.message : ''}`)
        }
      }
    },
    onSettled: refreshTasks,
    onSuccess: () => { refreshTasks(); showToast('Tarefa atualizada.'); setModal(null); setSelectedTask(null) },
  })
  const actionMutation = useMutation({
    mutationFn: ({ taskId, action }: { taskId: string; action: 'start' | 'complete' | 'cancel' }) => action === 'start' ? startTask(taskId) : action === 'complete' ? completeTask(taskId) : cancelTask(taskId),
    onSuccess: () => { refreshTasks(); showToast('Status da tarefa atualizado.') },
    onError: (error) => showToast(error instanceof ApiError ? error.message : 'Não foi possível alterar a tarefa.', 'error'),
  })
  const editProjectMutation = useMutation({ mutationFn: (data: ProjectFormData) => updateProject(projectId, { ...data, version: projectQuery.data?.version, status: data.status as ProjectStatus }), onSuccess: () => { queryClient.invalidateQueries({ queryKey: queryKeys.project(projectId) }); queryClient.invalidateQueries({ queryKey: queryKeys.projects(teamId) }); showToast('Projeto atualizado.'); setModal(null) }, onError: (error) => showToast(error instanceof ApiError ? error.message : 'Não foi possível atualizar o projeto.', 'error') })
  const deleteMutation = useMutation({
    mutationFn: () => deleteTarget === 'project' ? deleteProject(projectId) : deleteTarget ? deleteTask(deleteTarget.id) : Promise.resolve(),
    onSuccess: () => {
      if (deleteTarget === 'project') { queryClient.invalidateQueries({ queryKey: queryKeys.projects(teamId) }); showToast('Projeto excluído.'); navigate(`/teams/${teamId}`) }
      else { refreshTasks(); showToast('Tarefa excluída.'); setDeleteTarget(null) }
    },
    onError: (error) => showToast(error instanceof ApiError ? error.message : 'Não foi possível excluir.', 'error'),
  })

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

      <div className="board-toolbar">
        <div className="board-filters">
          <div className="search-input">
            <Search size={17} />
            <input
              value={titleFilter}
              onChange={(event) => setTitleFilter(event.target.value)}
              placeholder="Buscar por título..."
              aria-label="Buscar tarefas por título"
            />
          </div>
          <select
            className="filter-select"
            aria-label="Filtrar por responsável"
            value={assigneeId}
            onChange={(event) => setAssigneeId(event.target.value)}
          >
            <option value="">Todos os responsáveis</option>
            {user && <option value={user.id}>Minhas tarefas</option>}
            {membersQuery.data
              ?.filter((member) => member.id !== user?.id)
              .map((member) => <option value={member.id} key={member.id}>{member.name}</option>)}
          </select>
          <select
            className="filter-select"
            aria-label="Ordenar tarefas"
            value={sortOption}
            onChange={(event) => setSortOption(event.target.value as SortOption)}
          >
            <option value="recent">Mais recentes</option>
            <option value="oldest">Mais antigas</option>
            <option value="titleAscending">Título: A–Z</option>
            <option value="titleDescending">Título: Z–A</option>
          </select>
          <Button
            variant="ghost"
            size="sm"
            icon={<RotateCcw size={14} />}
            disabled={!hasFilters && sortOption === 'recent'}
            onClick={resetTaskView}
          >
            Limpar
          </Button>
        </div>
        <span aria-live="polite">
          {boardIsPending ? 'Carregando tarefas...' : `${totalTaskCount} ${totalTaskCount === 1 ? 'tarefa encontrada' : 'tarefas encontradas'}`}
        </span>
      </div>

      <p className="pagination-hint">Tarefas ativas aparecem por completo. Concluídas e canceladas são carregadas em blocos de 20.</p>
      {boardIsPending && <PageLoader label="Montando o quadro..." />}
      {boardError && <ErrorState message={(boardError as Error).message} onRetry={() => taskQueries.forEach((query) => query.refetch())} />}
      {!boardIsPending && !boardError && totalTaskCount === 0 && (hasFilters
        ? <EmptyState title="Nenhuma tarefa encontrada" description="Ajuste os filtros para encontrar outras tarefas." action={<Button variant="secondary" onClick={resetTaskView}>Limpar filtros</Button>} />
        : <EmptyState title="O quadro está vazio" description="Crie a primeira tarefa para dar forma ao trabalho deste projeto." action={<Button icon={<Plus size={16} />} onClick={openCreateTask} disabled={!activeProject}>Criar tarefa</Button>} />)}

      {!boardIsPending && !boardError && totalTaskCount > 0 && <div className="kanban-board">
        {boardColumns.map((column) => {
          const Icon = column.icon
          return <section className="kanban-column" key={column.status} aria-label={column.label}>
            <header><span><Icon size={16} /> {column.label}</span><small>{column.totalCount}</small></header>
            <div className="kanban-column__body">
              {column.tasks.map((task) => {
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
              {!column.tasks.length && <div className="column-empty">Nenhuma tarefa</div>}
              {'loadMore' in column && column.hasNextPage && <div className="column-pagination">
                <span>{column.tasks.length} de {column.totalCount}</span>
                <Button
                  variant="secondary"
                  size="sm"
                  loading={column.isFetchingNextPage}
                  onClick={column.loadMore}
                >
                  Carregar mais {column.status === TodoStatus.Done ? 'concluídas' : 'canceladas'}
                </Button>
              </div>}
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
        <form onSubmit={projectForm.handleSubmit((data) => editProjectMutation.mutate(data))}><Field label="Nome" htmlFor="edit-project-name" error={projectForm.formState.errors.name?.message}><Input id="edit-project-name" {...projectForm.register('name')} /></Field><Field label="Descrição" htmlFor="edit-project-description" error={projectForm.formState.errors.description?.message}><Textarea id="edit-project-description" rows={4} {...projectForm.register('description')} /></Field><Field label="Proprietário do projeto" htmlFor="project-owner"><select id="project-owner" className="input" {...projectForm.register('ownerId')}>{!membersQuery.data?.some(member => member.id === projectQuery.data?.ownerId) && <option value={projectQuery.data?.ownerId}>Proprietário anterior (selecione um membro)</option>}{membersQuery.data?.map(member => <option key={member.id} value={member.id}>{member.name}</option>)}</select></Field><Field label="Status" htmlFor="project-status"><select id="project-status" className="input" {...projectForm.register('status', { valueAsNumber: true })}><option value={ProjectStatus.Active}>Ativo</option><option value={ProjectStatus.Inactive}>Inativo</option><option value={ProjectStatus.Completed}>Concluído</option><option value={ProjectStatus.PendingApproval}>Aguardando aprovação</option></select></Field><div className="modal__actions modal__actions--split"><Button type="button" variant="danger" icon={<Trash2 size={15} />} onClick={() => { setModal(null); setDeleteTarget('project') }}>Excluir projeto</Button><div><Button type="button" variant="secondary" onClick={() => setModal(null)}>Cancelar</Button><Button type="submit" loading={editProjectMutation.isPending}>Salvar</Button></div></div></form>
      </Modal>

      <ConfirmDialog open={Boolean(deleteTarget)} title={deleteTarget === 'project' ? 'Excluir este projeto?' : 'Excluir esta tarefa?'} description="Esta ação é permanente e não poderá ser desfeita." loading={deleteMutation.isPending} onClose={() => setDeleteTarget(null)} onConfirm={() => deleteMutation.mutate()} />
    </div>
  )
}
