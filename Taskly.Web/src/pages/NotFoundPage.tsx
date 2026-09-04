import { ArrowLeft } from 'lucide-react'
import { Link } from 'react-router'
import { Logo } from '../shared/components/Logo'

export function NotFoundPage() {
  return (
    <main className="not-found">
      <Logo /><span>404</span><h1>Essa página saiu da lista.</h1><p>O endereço pode ter mudado ou não existe mais.</p>
      <Link className="button button--primary button--md" to="/teams"><ArrowLeft size={18} /> Voltar para equipes</Link>
    </main>
  )
}
