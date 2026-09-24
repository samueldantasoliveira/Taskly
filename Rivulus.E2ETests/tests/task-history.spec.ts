import { expect, test } from '@playwright/test'
import { createProject, createTask, post, signIn } from './helpers'

test('históricos de concluídas e canceladas carregam independentemente', async ({ page, request }) => {
  test.setTimeout(90_000)
  const context = await createProject(request)

  // Uma tarefa além do limite inicial em cada coluna força a segunda página.
  for (let index = 1; index <= 21; index += 1) {
    const done = await createTask(request, context, `Concluída ${index}`)
    await post(request, context.user.token, `todotask/${done.id}/start`)
    await post(request, context.user.token, `todotask/${done.id}/complete`)
    const cancelled = await createTask(request, context, `Cancelada ${index}`)
    await post(request, context.user.token, `todotask/${cancelled.id}/cancel`)
  }

  await signIn(page, context.user)
  await page.goto(`/projects/${context.projectId}`)
  const done = page.getByRole('region', { name: 'Concluídas', exact: true })
  const cancelled = page.getByRole('region', { name: 'Canceladas', exact: true })
  await expect(done.getByRole('article')).toHaveCount(20)
  await expect(cancelled.getByRole('article')).toHaveCount(20)

  await done.getByRole('button', { name: 'Carregar mais concluídas' }).click()
  await expect(done.getByRole('article')).toHaveCount(21)
  await expect(done.getByRole('heading', { name: 'Concluída 1', exact: true })).toBeVisible()
  await expect(done.getByRole('button', { name: 'Carregar mais concluídas' })).toHaveCount(0)
  await expect(cancelled.getByRole('article')).toHaveCount(20)

  await cancelled.getByRole('button', { name: 'Carregar mais canceladas' }).click()
  await expect(cancelled.getByRole('article')).toHaveCount(21)
  await expect(cancelled.getByRole('heading', { name: 'Cancelada 1', exact: true })).toBeVisible()
  await expect(cancelled.getByRole('button', { name: 'Carregar mais canceladas' })).toHaveCount(0)
  await expect(done.getByRole('article')).toHaveCount(21)
})
