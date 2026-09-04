import { zodResolver } from '@hookform/resolvers/zod'
import { useMutation } from '@tanstack/react-query'
import { KeyRound, Mail, Save, Trash2, UserRound } from 'lucide-react'
import { useForm } from 'react-hook-form'
import { useNavigate } from 'react-router'
import { z } from 'zod'
import { useAuth } from '../features/auth/auth-context'
import { deleteUser, updateUser } from '../features/users/api'
import { ApiError } from '../shared/api/client'
import { Avatar } from '../shared/components/Avatar'
import { Button } from '../shared/components/Button'
import { ConfirmDialog } from '../shared/components/ConfirmDialog'
import { Field, Input } from '../shared/components/Field'
import { useToast } from '../shared/components/toast-context'
import { useState } from 'react'

const schema = z.object({
  name: z.string().trim().min(2, 'Informe seu nome.'),
  email: z.string().trim().email('Informe um e-mail válido.'),
  password: z.string().refine((value) => !value || value.length >= 6, 'Use pelo menos 6 caracteres.'),
})
type FormData = z.infer<typeof schema>

export function ProfilePage() {
  const { user, updateUser: updateSessionUser, signOut } = useAuth()
  const { showToast } = useToast()
  const navigate = useNavigate()
  const [confirmDelete, setConfirmDelete] = useState(false)
  const form = useForm<FormData>({ resolver: zodResolver(schema), defaultValues: { name: user?.name ?? '', email: user?.email ?? '', password: '' } })
  const updateMutation = useMutation({
    mutationFn: (data: FormData) => updateUser(user!.id, { name: data.name, email: data.email, ...(data.password ? { password: data.password } : {}) }),
    onSuccess: (updated) => { updateSessionUser(updated); form.reset({ name: updated.name, email: updated.email, password: '' }); showToast('Perfil atualizado.') },
  })
  const deleteMutation = useMutation({ mutationFn: () => deleteUser(user!.id), onSuccess: () => { signOut(); navigate('/register'); showToast('Sua conta foi excluída.') }, onError: (error) => showToast(error instanceof ApiError ? error.message : 'Não foi possível excluir a conta.', 'error') })

  if (!user) return null

  return (
    <div className="page-stack profile-page">
      <section className="profile-header"><Avatar name={user.name} size="lg" /><div><span className="eyebrow">Conta pessoal</span><h1>{user.name}</h1><p>{user.email}</p></div></section>
      <section className="settings-card">
        <div className="settings-card__heading"><div><h2>Informações pessoais</h2><p>Atualize como você aparece para sua equipe.</p></div><UserRound size={20} /></div>
        <form onSubmit={form.handleSubmit((data) => updateMutation.mutate(data))}>
          <div className="form-grid"><Field label="Nome" htmlFor="profile-name" error={form.formState.errors.name?.message}><div className="input-with-icon"><UserRound size={17} /><Input id="profile-name" {...form.register('name')} /></div></Field><Field label="E-mail" htmlFor="profile-email" error={form.formState.errors.email?.message}><div className="input-with-icon"><Mail size={17} /><Input id="profile-email" type="email" {...form.register('email')} /></div></Field></div>
          <Field label="Nova senha" htmlFor="profile-password" hint="Deixe em branco para manter a senha atual." error={form.formState.errors.password?.message}><div className="input-with-icon"><KeyRound size={17} /><Input id="profile-password" type="password" autoComplete="new-password" placeholder="••••••••" {...form.register('password')} /></div></Field>
          {updateMutation.isError && <div className="form-alert">{updateMutation.error instanceof ApiError ? updateMutation.error.message : 'Não foi possível atualizar.'}</div>}
          <div className="settings-card__actions"><Button type="submit" icon={<Save size={17} />} loading={updateMutation.isPending}>Salvar alterações</Button></div>
        </form>
      </section>
      <section className="settings-card settings-card--danger"><div className="settings-card__heading"><div><h2>Excluir conta</h2><p>Remove seu acesso e seus dados permanentemente.</p></div><Trash2 size={20} /></div><Button variant="danger" onClick={() => setConfirmDelete(true)}>Excluir minha conta</Button></section>
      <ConfirmDialog open={confirmDelete} title="Excluir sua conta?" description="Esta ação não poderá ser desfeita. Você perderá acesso às suas equipes e projetos." confirmLabel="Excluir minha conta" loading={deleteMutation.isPending} onClose={() => setConfirmDelete(false)} onConfirm={() => deleteMutation.mutate()} />
    </div>
  )
}
