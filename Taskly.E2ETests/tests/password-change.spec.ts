import { expect, test } from '@playwright/test'
import { createUser, signIn } from './helpers'

test('trocar a senha encerra a sessão e permite entrar com a nova senha', async ({ page, request }) => {
  const user = await createUser(request)
  await signIn(page, user)
  await page.goto('/profile')
  await page.getByLabel('Nova senha').fill('NewPassword123!')
  await page.getByRole('button', { name: 'Salvar alterações' }).click()
  await expect(page).toHaveURL(/\/login$/)
  await signIn(page, { ...user, password: 'NewPassword123!' })
  await expect(page.getByRole('heading', { name: 'Suas equipes' })).toBeVisible()
})
