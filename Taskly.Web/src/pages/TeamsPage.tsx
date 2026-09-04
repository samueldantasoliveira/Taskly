import { zodResolver } from '@hookform/resolvers/zod'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { ArrowUpRight, Crown, Plus, UsersRound } from 'lucide-react'
import { useState } from 'react'
import { useForm } from 'react-hook-form'
import { Link } from 'react-router'
import { z } from 'zod'
import { useAuth } from '../features/auth/auth-context'
import { createTeam, getTeams } from '../features/teams/api'
import { ApiError } from '../shared/api/client'
import { Avatar } from '../shared/components/Avatar'
import { Button } from '../shared/components/Button'
import { EmptyState, ErrorState, PageLoader } from '../shared/components/Feedback'
import { Field, Input } from '../shared/components/Field'
import { Modal } from '../shared/components/Modal'
import { useToast } from '../shared/components/toast-context'
import { queryKeys } from '../shared/lib/query-keys'

const schema = z.object({ name: z.string().trim().min(2, 'Informe um nome para a equipe.') })
type FormData = z.infer<typeof schema>

export function TeamsPage() {
  const { user } = useAuth()
  const { showToast } = useToast()
  const queryClient = useQueryClient()
  const [createOpen, setCreateOpen] = useState(false)
  const form = useForm<FormData>({ resolver: zodResolver(schema) })
  const teamsQuery = useQuery({
    queryKey: queryKeys.teams,
    queryFn: ({ signal }) => getTeams(signal),
  })
  const createMutation = useMutation({
    mutationFn: createTeam,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.teams })
      showToast('Equipe criada com sucesso.')
      form.reset()
      setCreateOpen(false)
    },
  })

  const firstName = user?.name.split(' ')[0] ?? 'Olá'

  return (
    <div className="page-stack">
      <section className="welcome-banner">
        <div><span className="eyebrow">Visão geral</span><h1>Olá, {firstName}.</h1><p>Escolha uma equipe ou crie um novo espaço para começar.</p></div>
        <Button icon={<Plus size={18} />} onClick={() => setCreateOpen(true)}>Nova equipe</Button>
      </section>

      <section>
        <div className="section-heading"><div><h2>Suas equipes</h2><p>Todos os espaços dos quais você participa.</p></div>{teamsQuery.data?.length ? <span className="count-pill">{teamsQuery.data.length}</span> : null}</div>
        {teamsQuery.isPending && <PageLoader label="Buscando suas equipes..." />}
        {teamsQuery.isError && <ErrorState message={(teamsQuery.error as Error).message} onRetry={() => teamsQuery.refetch()} />}
        {teamsQuery.data?.length === 0 && (
          <EmptyState title="Nenhuma equipe por aqui" description="Crie sua primeira equipe para reunir pessoas, projetos e tarefas." action={<Button icon={<Plus size={17} />} onClick={() => setCreateOpen(true)}>Criar equipe</Button>} />
        )}
        <div className="team-grid">
          {teamsQuery.data?.map((team, index) => {
            const isOwner = team.ownerId === user?.id
            return (
              <Link className="team-card" to={`/teams/${team.id}`} key={team.id}>
                <div className={`team-card__visual team-card__visual--${(index % 4) + 1}`}><span>{team.name.slice(0, 1).toUpperCase()}</span></div>
                <div className="team-card__body">
                  <div className="team-card__title"><h3>{team.name}</h3><ArrowUpRight size={18} /></div>
                  <div className="team-card__meta">
                    <span><UsersRound size={15} /> {team.userIds.length} {team.userIds.length === 1 ? 'membro' : 'membros'}</span>
                    {isOwner && <span><Crown size={15} /> Você gerencia</span>}
                  </div>
                  <div className="team-card__footer"><div className="avatar-stack"><Avatar name={user?.name ?? 'Você'} size="sm" /></div><span className={`badge ${team.isActive ? 'badge--success' : 'badge--neutral'}`}>{team.isActive ? 'Ativa' : 'Inativa'}</span></div>
                </div>
              </Link>
            )
          })}
        </div>
      </section>

      <Modal open={createOpen} title="Criar uma equipe" description="Você será o responsável pelo novo espaço." onClose={() => setCreateOpen(false)}>
        <form onSubmit={form.handleSubmit((data) => createMutation.mutate(data))}>
          <Field label="Nome da equipe" htmlFor="team-name" error={form.formState.errors.name?.message}><Input id="team-name" autoFocus placeholder="Ex.: Produto, Marketing..." {...form.register('name')} /></Field>
          {createMutation.isError && <div className="form-alert">{createMutation.error instanceof ApiError ? createMutation.error.message : 'Não foi possível criar a equipe.'}</div>}
          <div className="modal__actions"><Button type="button" variant="secondary" onClick={() => setCreateOpen(false)}>Cancelar</Button><Button type="submit" loading={createMutation.isPending}>Criar equipe</Button></div>
        </form>
      </Modal>
    </div>
  )
}
