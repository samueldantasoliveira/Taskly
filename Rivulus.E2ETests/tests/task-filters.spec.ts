import { expect, test } from '@playwright/test'
import { createProject, createTask, createUser, post, signIn } from './helpers'

test('Kanban combina título e responsável e permite limpar filtros', async ({ page, request }) => {
  const context = await createProject(request)
  const member = await createUser(request)
  await post(request, context.user.token, `team/${context.teamId}/add-member?userId=${member.id}`)
  await createTask(request, context, 'Revisar API')
  await createTask(request, context, 'Revisar frontend', member.id)
  await createTask(request, context, 'Documentar projeto')
  await signIn(page, context.user)
  await page.goto(`/projects/${context.projectId}`)
  const cards = page.locator('.kanban-board .task-card')
  await expect(cards).toHaveCount(3)

  await page.getByRole('textbox', { name: 'Buscar tarefas por título' }).fill('Revisar')
  await expect(cards).toHaveCount(2)
  await expect(page.getByRole('heading', { name: 'Documentar projeto' })).toHaveCount(0)

  const assignee = page.getByRole('combobox', { name: 'Filtrar por responsável' })
  await assignee.selectOption(context.user.id)
  await expect(cards).toHaveCount(1)
  await expect(cards.getByRole('heading', { name: 'Revisar API' })).toBeVisible()
  await assignee.selectOption(member.id)
  await expect(cards.getByRole('heading', { name: 'Revisar frontend' })).toBeVisible()
  await expect(cards).toHaveCount(1)

  await page.getByRole('button', { name: 'Limpar', exact: true }).click()
  await expect(cards).toHaveCount(3)
  await expect(assignee).toHaveValue('')
  await expect(page.getByRole('textbox', { name: 'Buscar tarefas por título' })).toHaveValue('')
})
