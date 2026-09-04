import { zodResolver } from '@hookform/resolvers/zod'
import { useMutation } from '@tanstack/react-query'
import { ArrowRight } from 'lucide-react'
import { useForm } from 'react-hook-form'
import { Link, useLocation, useNavigate } from 'react-router'
import { z } from 'zod'
import { AuthLayout } from '../features/auth/AuthLayout'
import { useAuth } from '../features/auth/auth-context'
import { login } from '../features/auth/api'
import { ApiError } from '../shared/api/client'
import { Button } from '../shared/components/Button'
import { Field, Input } from '../shared/components/Field'

const schema = z.object({
  email: z.string().trim().email('Informe um e-mail válido.'),
  password: z.string().min(1, 'Informe sua senha.'),
})

type FormData = z.infer<typeof schema>

export function LoginPage() {
  const { signIn } = useAuth()
  const navigate = useNavigate()
  const location = useLocation()
  const form = useForm<FormData>({ resolver: zodResolver(schema) })
  const mutation = useMutation({
    mutationFn: login,
    onSuccess: (session) => {
      signIn(session)
      const destination = (location.state as { from?: string } | null)?.from ?? '/teams'
      navigate(destination, { replace: true })
    },
  })

  return (
    <AuthLayout>
      <div className="auth-card">
        <div className="auth-card__heading"><span className="eyebrow">Bem-vindo de volta</span><h2>Entre na sua conta</h2><p>Continue de onde sua equipe parou.</p></div>
        <form onSubmit={form.handleSubmit((data) => mutation.mutate(data))}>
          <Field label="E-mail" htmlFor="email" error={form.formState.errors.email?.message}>
            <Input id="email" type="email" autoComplete="email" placeholder="voce@exemplo.com" {...form.register('email')} />
          </Field>
          <Field label="Senha" htmlFor="password" error={form.formState.errors.password?.message}>
            <Input id="password" type="password" autoComplete="current-password" placeholder="Sua senha" {...form.register('password')} />
          </Field>
          {mutation.isError && <div className="form-alert" role="alert">{mutation.error instanceof ApiError ? mutation.error.message : 'Não foi possível entrar.'}</div>}
          <Button type="submit" className="button--full" loading={mutation.isPending} icon={<ArrowRight size={18} />}>Entrar</Button>
        </form>
        <p className="auth-card__switch">Ainda não tem uma conta? <Link to="/register">Criar conta</Link></p>
      </div>
    </AuthLayout>
  )
}
