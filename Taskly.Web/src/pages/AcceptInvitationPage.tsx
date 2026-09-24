import { useMutation, useQueryClient } from '@tanstack/react-query'
import { MailCheck } from 'lucide-react'
import { useNavigate, useParams } from 'react-router'
import { acceptTeamInvitation } from '../features/teams/api'
import { Button } from '../shared/components/Button'
import { queryKeys } from '../shared/lib/query-keys'

export function AcceptInvitationPage() {
  const { token = '' } = useParams()
  const navigate = useNavigate()
  const queryClient = useQueryClient()
  const mutation = useMutation({ mutationFn: () => acceptTeamInvitation(token), onSuccess: (team) => { queryClient.invalidateQueries({ queryKey: queryKeys.teams }); navigate(`/teams/${team.id}`) } })
  return <div className="invite-page"><section><MailCheck size={36} /><h1>Convite para equipe</h1><p>Confirme para entrar na equipe usando o e-mail da sua conta Taskly.</p>{mutation.isError && <div className="form-alert">{(mutation.error as Error).message}</div>}<Button loading={mutation.isPending} disabled={!token} onClick={() => mutation.mutate()}>Aceitar convite</Button></section></div>
}
