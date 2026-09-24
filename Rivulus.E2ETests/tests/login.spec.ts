import { expect, test } from '@playwright/test'
import { createUser, signIn } from './helpers'

test('usuário existente entra e mantém a sessão ao recarregar', async ({ page, request }) => {
  const user = await createUser(request)
  await signIn(page, user)
  await expect(page.getByRole('heading', { name: 'Suas equipes' })).toBeVisible()

  await page.reload()
  await expect(page).toHaveURL(/\/teams$/)
  await expect(page.getByRole('heading', { name: 'Olá, Usuário.' })).toBeVisible()
})
