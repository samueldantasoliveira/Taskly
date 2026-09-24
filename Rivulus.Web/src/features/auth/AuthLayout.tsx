import type { ReactNode } from 'react'
import { CheckCircle2, Layers3, UsersRound } from 'lucide-react'
import { Logo } from '../../shared/components/Logo'
import { ThemeToggle } from '../../shared/components/ThemeToggle'

export function AuthLayout({ children }: { children: ReactNode }) {
  return (
    <main className="auth-layout">
      <section className="auth-showcase">
        <Logo className="auth-showcase__logo" />
        <div className="auth-showcase__content">
          <span className="auth-showcase__pill">Projetos e equipes em movimento</span>
          <h1>Faça o trabalho<br />seguir em frente.</h1>
          <p>Transforme planos em um fluxo claro, conectando equipes, projetos e tarefas do início à entrega.</p>
          <div className="auth-feature-grid">
            <article><UsersRound /><strong>Equipes alinhadas</strong><span>Pessoas e projetos no mesmo lugar.</span></article>
            <article><Layers3 /><strong>Visão clara</strong><span>Do planejamento até a entrega.</span></article>
            <article><CheckCircle2 /><strong>Foco no progresso</strong><span>Próximos passos sempre visíveis.</span></article>
          </div>
        </div>
        <p className="auth-showcase__footer">Clareza para começar. Ritmo para entregar.</p>
      </section>
      <section className="auth-panel">
        <div className="auth-panel__theme"><ThemeToggle /></div>
        {children}
      </section>
    </main>
  )
}
