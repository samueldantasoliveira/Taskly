import { useQuery } from '@tanstack/react-query'
import { ChevronRight, LogOut, Menu, Plus, Settings, UsersRound, X } from 'lucide-react'
import { useState } from 'react'
import { Link, NavLink, Outlet, useLocation } from 'react-router'
import { useAuth } from '../features/auth/auth-context'
import { getTeams } from '../features/teams/api'
import { Avatar } from '../shared/components/Avatar'
import { Logo } from '../shared/components/Logo'
import { queryKeys } from '../shared/lib/query-keys'

function getPageTitle(pathname: string) {
  if (pathname === '/teams') return 'Suas equipes'
  if (pathname === '/profile') return 'Seu perfil'
  if (pathname.startsWith('/projects/')) return 'Projeto'
  if (pathname.startsWith('/teams/')) return 'Equipe'
  return 'Taskly'
}

export function AppShell() {
  const { user, signOut } = useAuth()
  const location = useLocation()
  const [mobileOpen, setMobileOpen] = useState(false)
  const { data: teams = [] } = useQuery({
    queryKey: queryKeys.teams,
    queryFn: ({ signal }) => getTeams(signal),
  })
  const closeMobile = () => setMobileOpen(false)

  return (
    <div className="app-shell">
      {mobileOpen && <button className="sidebar-scrim" onClick={closeMobile} aria-label="Fechar menu" />}
      <aside className={`sidebar ${mobileOpen ? 'sidebar--open' : ''}`}>
        <div className="sidebar__top">
          <Link to="/teams" onClick={closeMobile}><Logo /></Link>
          <button className="icon-button sidebar__close" onClick={closeMobile} aria-label="Fechar menu"><X size={20} /></button>
        </div>
        <nav className="sidebar__nav" aria-label="Navegação principal">
          <NavLink to="/teams" end onClick={closeMobile}><UsersRound size={18} /> Equipes</NavLink>
          <NavLink to="/profile" onClick={closeMobile}><Settings size={18} /> Perfil</NavLink>
        </nav>
        <div className="sidebar__section">
          <div className="sidebar__label"><span>Espaços</span><Link to="/teams" title="Criar equipe"><Plus size={15} /></Link></div>
          <div className="sidebar__teams">
            {teams.slice(0, 7).map((team) => (
              <NavLink key={team.id} to={`/teams/${team.id}`} onClick={closeMobile}>
                <span className="team-dot" /><span>{team.name}</span><ChevronRight size={14} />
              </NavLink>
            ))}
            {!teams.length && <p>Suas equipes aparecerão aqui.</p>}
          </div>
        </div>
        <div className="sidebar__account">
          <Link to="/profile" onClick={closeMobile}>
            <Avatar name={user?.name ?? 'Usuário'} size="sm" />
            <span><strong>{user?.name}</strong><small>{user?.email}</small></span>
          </Link>
          <button className="icon-button" onClick={signOut} title="Sair" aria-label="Sair"><LogOut size={18} /></button>
        </div>
      </aside>
      <div className="app-content">
        <header className="topbar">
          <button className="icon-button topbar__menu" onClick={() => setMobileOpen(true)} aria-label="Abrir menu"><Menu size={21} /></button>
          <div><span className="eyebrow">Workspace</span><strong>{getPageTitle(location.pathname)}</strong></div>
          <Avatar name={user?.name ?? 'Usuário'} size="sm" />
        </header>
        <main className="page-container"><Outlet /></main>
      </div>
    </div>
  )
}
