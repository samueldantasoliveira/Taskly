import { render, screen } from '@testing-library/react'
import { describe, expect, it } from 'vitest'
import { ProjectStatus, TodoStatus } from '../types/api'
import { ProjectStatusBadge, TaskStatusBadge } from './StatusBadge'

describe('status badges', () => {
  it.each([
    [TodoStatus.Todo, 'A fazer'],
    [TodoStatus.InProgress, 'Em andamento'],
    [TodoStatus.Done, 'Concluída'],
    [TodoStatus.Cancelled, 'Cancelada'],
  ])('traduz o status de tarefa %s', (status, label) => {
    render(<TaskStatusBadge status={status} />)
    expect(screen.getByText(label)).toBeInTheDocument()
  })

  it('traduz o status do projeto', () => {
    render(<ProjectStatusBadge status={ProjectStatus.PendingApproval} />)
    expect(screen.getByText('Aguardando')).toBeInTheDocument()
  })
})
