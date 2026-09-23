import { expect, test } from '@playwright/test'
import { createUser, signIn } from './helpers'

test('usuário cria equipe, projeto e tarefa pelo navegador', async ({ page, request }) => {
  const user = await createUser(request)
  await signIn(page, user)

  await page.getByRole('button', { name: 'Nova equipe' }).click()
  const teamDialog = page.getByRole('dialog', { name: 'Criar uma equipe' })
  await teamDialog.getByLabel('Nome da equipe').fill('Equipe do fluxo completo')
  await teamDialog.getByRole('button', { name: 'Criar equipe', exact: true }).click()
  await page.getByRole('link').filter({
    has: page.getByRole('heading', { name: 'Equipe do fluxo completo', exact: true }),
  }).click()

  await page.getByRole('button', { name: 'Novo projeto' }).click()
  const projectDialog = page.getByRole('dialog', { name: 'Novo projeto' })
  await projectDialog.getByLabel('Nome', { exact: true }).fill('Projeto do fluxo completo')
  await projectDialog.getByLabel('Descrição').fill('Validar o caminho principal do Taskly')
  await projectDialog.getByRole('button', { name: 'Criar projeto', exact: true }).click()
  await page.getByRole('link').filter({ hasText: 'Projeto do fluxo completo' }).click()

  await page.getByRole('button', { name: 'Nova tarefa' }).click()
  const taskDialog = page.getByRole('dialog', { name: 'Nova tarefa' })
  await taskDialog.getByLabel('Título').fill('Minha primeira tarefa')
  await taskDialog.getByLabel('Descrição').fill('Criada pelo navegador')
  await taskDialog.getByLabel('Prioridade').selectOption('2')
  await taskDialog.getByLabel('Prazo').fill('2026-12-10')
  await taskDialog.getByRole('button', { name: 'Criar tarefa', exact: true }).click()
  const column = page.getByRole('region', { name: 'A fazer', exact: true })
  await expect(column.getByRole('heading', { name: 'Minha primeira tarefa' })).toBeVisible()
  await expect(column).toContainText('Alta')
  await expect(column).toContainText('10/12/2026')

  await page.reload()
  await expect(column.getByRole('heading', { name: 'Minha primeira tarefa' })).toBeVisible()
})
