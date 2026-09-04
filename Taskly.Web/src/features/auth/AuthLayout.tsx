import type { ReactNode } from 'react'
import { CheckCircle2, Layers3, UsersRound } from 'lucide-react'
import { Logo } from '../../shared/components/Logo'

export function AuthLayout({ children }: { children: ReactNode }) {
  return (
    <main className="auth-layout">
      <section className="auth-showcase">
        <Logo className="auth-showcase__logo" />
        <div className="auth-showcase__content">
          <span className="auth-showcase__pill">Planeje. Colabore. Conclua.</span>
          <h1>Trabalho em equipe,<br />sem o caos.</h1>
          <p>Organize equipes, projetos e tarefas em um espaço simples que mantém todo mundo em movimento.</p>
          <div className="auth-feature-grid">
            <article><UsersRound /><strong>Equipes alinhadas</strong><span>Pessoas e projetos no mesmo lugar.</span></article>
            <article><Layers3 /><strong>Visão clara</strong><span>Do planejamento até a entrega.</span></article>
            <article><CheckCircle2 /><strong>Foco no progresso</strong><span>Próximos passos sempre visíveis.</span></article>
          </div>
        </div>
        <p className="auth-showcase__footer">Seu trabalho merece um fluxo mais leve.</p>
      </section>
      <section className="auth-panel">{children}</section>
    </main>
  )
}
