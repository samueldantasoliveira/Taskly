import { zodResolver } from '@hookform/resolvers/zod'
import { useMutation } from '@tanstack/react-query'
import { ArrowRight } from 'lucide-react'
import { useForm } from 'react-hook-form'
import { Link, useNavigate } from 'react-router'
import { z } from 'zod'
import { AuthLayout } from '../features/auth/AuthLayout'
import { useAuth } from '../features/auth/auth-context'
import { login, register } from '../features/auth/api'
import { ApiError } from '../shared/api/client'
import { Button } from '../shared/components/Button'
import { Field, Input } from '../shared/components/Field'

const schema = z.object({
  name: z.string().trim().min(2, 'Informe seu nome.'),
  email: z.string().trim().email('Informe um e-mail válido.'),
  password: z.string().min(6, 'Use pelo menos 6 caracteres.'),
})

type FormData = z.infer<typeof schema>

export function RegisterPage() {
  const { signIn } = useAuth()
  const navigate = useNavigate()
  const form = useForm<FormData>({ resolver: zodResolver(schema) })
  const mutation = useMutation({
    mutationFn: async (data: FormData) => {
      await register(data)
      return login({ email: data.email, password: data.password })
    },
    onSuccess: (session) => {
      signIn(session)
      navigate('/teams', { replace: true })
    },
  })

  return (
    <AuthLayout>
      <div className="auth-card">
        <div className="auth-card__heading"><span className="eyebrow">Comece agora</span><h2>Crie sua conta</h2><p>Seu primeiro workspace fica pronto em segundos.</p></div>
        <form onSubmit={form.handleSubmit((data) => mutation.mutate(data))}>
          <Field label="Nome" htmlFor="name" error={form.formState.errors.name?.message}><Input id="name" autoComplete="name" placeholder="Seu nome" {...form.register('name')} /></Field>
          <Field label="E-mail" htmlFor="email" error={form.formState.errors.email?.message}><Input id="email" type="email" autoComplete="email" placeholder="voce@exemplo.com" {...form.register('email')} /></Field>
          <Field label="Senha" htmlFor="password" hint="Use pelo menos 6 caracteres." error={form.formState.errors.password?.message}><Input id="password" type="password" autoComplete="new-password" placeholder="Crie uma senha" {...form.register('password')} /></Field>
          {mutation.isError && <div className="form-alert" role="alert">{mutation.error instanceof ApiError ? mutation.error.message : 'Não foi possível criar sua conta.'}</div>}
          <Button type="submit" className="button--full" loading={mutation.isPending} icon={<ArrowRight size={18} />}>Criar minha conta</Button>
        </form>
        <p className="auth-card__switch">Já tem uma conta? <Link to="/login">Entrar</Link></p>
      </div>
    </AuthLayout>
  )
}
