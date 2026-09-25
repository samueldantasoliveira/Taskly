import { expect, test } from '@playwright/test'
import { createUser, signIn } from './helpers'

test('usuário escolhe um avatar e mantém a escolha ao recarregar', async ({ page, request }) => {
  const user = await createUser(request)
  await signIn(page, user)

  await page.goto('/profile')
  await page.getByRole('radio', { name: 'Lontra' }).click()
  await page.getByRole('button', { name: 'Salvar alterações' }).click()
  await expect(page.getByText('Perfil atualizado.')).toBeVisible()

  await page.reload()

  await expect(page.getByRole('radio', { name: 'Lontra' })).toBeChecked()
  await expect(page.locator('.profile-header .avatar img')).toHaveAttribute('src', '/avatars/otter.svg')
})
