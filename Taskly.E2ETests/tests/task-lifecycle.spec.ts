import { expect, test } from '@playwright/test'
import { createProject, createTask, signIn } from './helpers'

test('usuário assume, inicia e conclui uma tarefa', async ({ page, request }) => {
  const context = await createProject(request)
  const title = 'Validar ciclo da tarefa'
  await createTask(request, context, title, null)
  await signIn(page, context.user)
  await page.goto(`/projects/${context.projectId}`)

  await page.getByRole('button', { name: `Editar ${title}`, exact: true }).click()
  const dialog = page.getByRole('dialog', { name: 'Editar tarefa' })
  await dialog.getByLabel('Responsável').selectOption(context.user.id)
  await dialog.getByRole('button', { name: 'Salvar', exact: true }).click()
  await expect(dialog).not.toBeVisible()

  const todo = page.getByRole('region', { name: 'A fazer', exact: true })
  await todo.getByRole('button', { name: 'Iniciar', exact: true }).click()
  const inProgress = page.getByRole('region', { name: 'Em andamento', exact: true })
  await expect(inProgress.getByRole('heading', { name: title })).toBeVisible()
  await expect(todo.getByRole('heading', { name: title })).toHaveCount(0)

  await inProgress.getByRole('button', { name: 'Concluir', exact: true }).click()
  const done = page.getByRole('region', { name: 'Concluídas', exact: true })
  await expect(done.getByRole('heading', { name: title })).toBeVisible()
  await expect(inProgress.getByRole('heading', { name: title })).toHaveCount(0)
  await page.reload()
  await expect(done.getByRole('heading', { name: title })).toBeVisible()
})
