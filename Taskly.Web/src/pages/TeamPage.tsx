import { zodResolver } from '@hookform/resolvers/zod'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { ArrowLeft, Crown, FolderKanban, LogOut, Pencil, Plus, Search, Trash2, UserPlus, UsersRound } from 'lucide-react'
import { useEffect, useState } from 'react'
import { useForm } from 'react-hook-form'
import { Link, useNavigate, useParams } from 'react-router'
import { z } from 'zod'
import { useAuth } from '../features/auth/auth-context'
import { createProject, getTeamProjects } from '../features/projects/api'
import { addTeamMember, deleteTeam, getTeam, getTeamMembers, leaveTeam, removeTeamMember, updateTeam } from '../features/teams/api'
import { searchUser } from '../features/users/api'
import { ApiError } from '../shared/api/client'
import { Avatar } from '../shared/components/Avatar'
import { Button } from '../shared/components/Button'
import { ConfirmDialog } from '../shared/components/ConfirmDialog'
import { EmptyState, ErrorState, PageLoader } from '../shared/components/Feedback'
import { Field, Input, Textarea } from '../shared/components/Field'
import { Modal } from '../shared/components/Modal'
import { ProjectStatusBadge } from '../shared/components/StatusBadge'
import { useToast } from '../shared/components/toast-context'
import { queryKeys } from '../shared/lib/query-keys'
import type { User } from '../shared/types/api'

const teamSchema = z.object({ name: z.string().trim().min(2, 'Informe um nome.') })
const projectSchema = z.object({ name: z.string().trim().min(2, 'Informe um nome.'), description: z.string().trim().min(1, 'Informe uma descrição.').max(500) })
const searchSchema = z.object({ email: z.string().trim().email('Informe um e-mail válido.') })

export function TeamPage() {
  const { teamId = '' } = useParams()
  const { user } = useAuth()
  const navigate = useNavigate()
  const queryClient = useQueryClient()
  const { showToast } = useToast()
  const [modal, setModal] = useState<'project' | 'edit' | 'members' | null>(null)
  const [confirmAction, setConfirmAction] = useState<'delete' | 'leave' | null>(null)
  const [foundUser, setFoundUser] = useState<User | null>(null)
  const teamForm = useForm<z.infer<typeof teamSchema>>({ resolver: zodResolver(teamSchema) })
  const projectForm = useForm<z.infer<typeof projectSchema>>({ resolver: zodResolver(projectSchema) })
  const searchForm = useForm<z.infer<typeof searchSchema>>({ resolver: zodResolver(searchSchema) })

  const teamQuery = useQuery({ queryKey: queryKeys.team(teamId), queryFn: ({ signal }) => getTeam(teamId, signal), enabled: Boolean(teamId) })
  const membersQuery = useQuery({ queryKey: queryKeys.members(teamId), queryFn: ({ signal }) => getTeamMembers(teamId, signal), enabled: Boolean(teamId) })
  const projectsQuery = useQuery({ queryKey: queryKeys.projects(teamId), queryFn: ({ signal }) => getTeamProjects(teamId, signal), enabled: Boolean(teamId) })
  const isOwner = teamQuery.data?.ownerId === user?.id

  useEffect(() => { if (teamQuery.data) teamForm.reset({ name: teamQuery.data.name }) }, [teamForm, teamQuery.data])

  const refreshTeam = () => {
    queryClient.invalidateQueries({ queryKey: queryKeys.team(teamId) })
    queryClient.invalidateQueries({ queryKey: queryKeys.teams })
  }
  const createProjectMutation = useMutation({ mutationFn: (data: z.infer<typeof projectSchema>) => createProject({ ...data, teamId }), onSuccess: () => { queryClient.invalidateQueries({ queryKey: queryKeys.projects(teamId) }); showToast('Projeto criado.'); projectForm.reset(); setModal(null) } })
  const editTeamMutation = useMutation({ mutationFn: (data: z.infer<typeof teamSchema>) => updateTeam(teamId, data), onSuccess: () => { refreshTeam(); showToast('Equipe atualizada.'); setModal(null) }, onError: (error) => showToast(error instanceof ApiError ? error.message : 'Não foi possível atualizar a equipe.', 'error') })
  const statusMutation = useMutation({ mutationFn: (isActive: boolean) => updateTeam(teamId, { isActive }), onSuccess: () => { refreshTeam(); showToast('Status da equipe atualizado.') }, onError: (error) => showToast(error instanceof ApiError ? error.message : 'Não foi possível alterar o status.', 'error') })
  const searchMutation = useMutation({ mutationFn: (data: z.infer<typeof searchSchema>) => searchUser(data.email), onSuccess: setFoundUser })
  const addMutation = useMutation({ mutationFn: (userId: string) => addTeamMember(teamId, userId), onSuccess: () => { queryClient.invalidateQueries({ queryKey: queryKeys.members(teamId) }); refreshTeam(); setFoundUser(null); searchForm.reset(); showToast('Pessoa adicionada à equipe.') }, onError: (error) => showToast(error instanceof ApiError ? error.message : 'Não foi possível adicionar a pessoa.', 'error') })
  const removeMutation = useMutation({ mutationFn: (userId: string) => removeTeamMember(teamId, userId), onSuccess: () => { queryClient.invalidateQueries({ queryKey: queryKeys.members(teamId) }); refreshTeam(); showToast('Pessoa removida da equipe.') }, onError: (error) => showToast(error instanceof ApiError ? error.message : 'Não foi possível remover a pessoa.', 'error') })
  const destructiveMutation = useMutation({ mutationFn: () => confirmAction === 'delete' ? deleteTeam(teamId) : leaveTeam(teamId), onSuccess: () => { queryClient.invalidateQueries({ queryKey: queryKeys.teams }); showToast(confirmAction === 'delete' ? 'Equipe excluída.' : 'Você saiu da equipe.'); navigate('/teams') }, onError: (error) => showToast(error instanceof ApiError ? error.message : 'Não foi possível concluir a ação.', 'error') })

  if (teamQuery.isPending) return <PageLoader label="Abrindo a equipe..." />
  if (teamQuery.isError) return <ErrorState message={(teamQuery.error as Error).message} onRetry={() => teamQuery.refetch()} />
  const team = teamQuery.data

  return (
    <div className="page-stack">
      <Link className="back-link" to="/teams"><ArrowLeft size={16} /> Todas as equipes</Link>
      <section className="entity-hero">
        <div className="entity-hero__mark">{team.name.slice(0, 1).toUpperCase()}</div>
        <div className="entity-hero__main"><div className="entity-hero__title"><h1>{team.name}</h1><span className={`badge ${team.isActive ? 'badge--success' : 'badge--neutral'}`}>{team.isActive ? 'Ativa' : 'Inativa'}</span></div><p>{team.userIds.length} {team.userIds.length === 1 ? 'pessoa colaborando' : 'pessoas colaborando'} neste espaço.</p></div>
        <div className="entity-hero__actions">
          {isOwner ? <><Button variant="secondary" icon={<UserPlus size={17} />} onClick={() => setModal('members')}>Membros</Button><Button variant="ghost" icon={<Pencil size={17} />} onClick={() => setModal('edit')}>Editar</Button></> : <Button variant="secondary" icon={<LogOut size={17} />} onClick={() => setConfirmAction('leave')}>Sair da equipe</Button>}
        </div>
      </section>

      <div className="content-grid content-grid--team">
        <section className="content-main">
          <div className="section-heading"><div><h2>Projetos</h2><p>Iniciativas em andamento nesta equipe.</p></div><Button size="sm" icon={<Plus size={16} />} onClick={() => setModal('project')} disabled={!team.isActive}>Novo projeto</Button></div>
          {projectsQuery.isPending && <PageLoader label="Carregando projetos..." />}
          {projectsQuery.isError && <ErrorState message={(projectsQuery.error as Error).message} />}
          {projectsQuery.data?.length === 0 && <EmptyState title="Nenhum projeto ainda" description="Crie um projeto para começar a organizar as tarefas da equipe." action={<Button icon={<Plus size={16} />} onClick={() => setModal('project')} disabled={!team.isActive}>Criar projeto</Button>} />}
          <div className="project-list">
            {projectsQuery.data?.map((project) => (
              <Link to={`/projects/${project.id}`} className="project-row" key={project.id}>
                <span className="project-row__icon"><FolderKanban size={19} /></span>
                <span className="project-row__content"><strong>{project.name}</strong><small>{project.description}</small></span>
                <ProjectStatusBadge status={project.status} />
              </Link>
            ))}
          </div>
        </section>
        <aside className="content-aside">
          <div className="section-heading"><div><h2>Membros</h2><p>Quem faz parte deste espaço.</p></div><UsersRound size={19} /></div>
          {membersQuery.isPending && <PageLoader label="Carregando..." />}
          <div className="member-list">
            {membersQuery.data?.slice(0, 8).map((member) => <div className="member-row" key={member.id}><Avatar name={member.name} size="sm" /><span><strong>{member.name}</strong><small>{member.email}</small></span>{member.isOwner && <Crown size={15} className="owner-icon" />}</div>)}
          </div>
          {isOwner && <Button variant="secondary" className="button--full" onClick={() => setModal('members')}>Gerenciar membros</Button>}
        </aside>
      </div>

      <Modal open={modal === 'project'} title="Novo projeto" description={`Crie uma iniciativa dentro de ${team.name}.`} onClose={() => setModal(null)}>
        <form onSubmit={projectForm.handleSubmit((data) => createProjectMutation.mutate(data))}><Field label="Nome" htmlFor="project-name" error={projectForm.formState.errors.name?.message}><Input id="project-name" autoFocus placeholder="Ex.: Novo aplicativo" {...projectForm.register('name')} /></Field><Field label="Descrição" htmlFor="project-description" error={projectForm.formState.errors.description?.message}><Textarea id="project-description" rows={4} placeholder="Qual é o objetivo deste projeto?" {...projectForm.register('description')} /></Field>{createProjectMutation.isError && <div className="form-alert">{(createProjectMutation.error as Error).message}</div>}<div className="modal__actions"><Button type="button" variant="secondary" onClick={() => setModal(null)}>Cancelar</Button><Button type="submit" loading={createProjectMutation.isPending}>Criar projeto</Button></div></form>
      </Modal>

      <Modal open={modal === 'edit'} title="Editar equipe" onClose={() => setModal(null)}>
        <form onSubmit={teamForm.handleSubmit((data) => editTeamMutation.mutate(data))}><Field label="Nome" htmlFor="edit-team-name" error={teamForm.formState.errors.name?.message}><Input id="edit-team-name" {...teamForm.register('name')} /></Field><label className="toggle-row"><span><strong>Equipe ativa</strong><small>Projetos e tarefas dependem deste estado.</small></span><input type="checkbox" checked={team.isActive} disabled={statusMutation.isPending} onChange={(event) => statusMutation.mutate(event.target.checked)} /></label><div className="modal__actions modal__actions--split"><Button type="button" variant="danger" icon={<Trash2 size={16} />} onClick={() => { setModal(null); setConfirmAction('delete') }}>Excluir equipe</Button><div><Button type="button" variant="secondary" onClick={() => setModal(null)}>Cancelar</Button><Button type="submit" loading={editTeamMutation.isPending}>Salvar</Button></div></div></form>
      </Modal>

      <Modal open={modal === 'members'} title="Gerenciar membros" description="Busque uma pessoa pelo e-mail cadastrado." onClose={() => { setModal(null); setFoundUser(null) }} size="lg">
        <form className="search-form" onSubmit={searchForm.handleSubmit((data) => searchMutation.mutate(data))}><Field label="E-mail" htmlFor="member-email" error={searchForm.formState.errors.email?.message}><Input id="member-email" type="email" placeholder="pessoa@exemplo.com" {...searchForm.register('email')} /></Field><Button type="submit" variant="secondary" loading={searchMutation.isPending} icon={<Search size={17} />}>Buscar</Button></form>
        {searchMutation.isError && <div className="form-alert">{searchMutation.error instanceof ApiError ? searchMutation.error.message : 'Usuário não encontrado.'}</div>}
        {foundUser && <div className="search-result"><Avatar name={foundUser.name} /><span><strong>{foundUser.name}</strong><small>{foundUser.email}</small></span><Button size="sm" loading={addMutation.isPending} onClick={() => addMutation.mutate(foundUser.id)}>Adicionar</Button></div>}
        <div className="member-manager"><h3>Membros atuais</h3>{membersQuery.data?.map((member) => <div className="member-row member-row--managed" key={member.id}><Avatar name={member.name} size="sm" /><span><strong>{member.name}</strong><small>{member.email}</small></span>{member.isOwner ? <span className="badge badge--warning">Responsável</span> : <Button variant="ghost" size="sm" loading={removeMutation.isPending && removeMutation.variables === member.id} onClick={() => removeMutation.mutate(member.id)}>Remover</Button>}</div>)}</div>
      </Modal>

      <ConfirmDialog open={Boolean(confirmAction)} title={confirmAction === 'delete' ? 'Excluir esta equipe?' : 'Sair desta equipe?'} description={confirmAction === 'delete' ? 'Projetos e tarefas vinculados não poderão mais ser acessados.' : 'Você perderá acesso aos projetos e tarefas deste espaço.'} confirmLabel={confirmAction === 'delete' ? 'Excluir equipe' : 'Sair da equipe'} loading={destructiveMutation.isPending} onClose={() => setConfirmAction(null)} onConfirm={() => destructiveMutation.mutate()} />
    </div>
  )
}
