import { expect, test } from '@playwright/test'

test.describe('Cadastro', () => {
  test('usuário consegue criar uma conta', async ({ page }) => {
    const uniqueId = `${Date.now()}-${test.info().workerIndex}`
    const user = {
      name: 'Usuário E2E',
      email: `e2e-${uniqueId}@rivulus.test`,
      password: 'SenhaE2E123!',
    }

    await page.goto('/register')

    await page.getByLabel('Nome').fill(user.name)
    await page.getByLabel('E-mail').fill(user.email)
    await page.getByLabel('Senha').fill(user.password)
    await page.getByRole('button', { name: 'Criar minha conta' }).click()

    await expect(page).toHaveURL(/\/teams$/)
    await expect(
      page.getByRole('heading', { name: 'Olá, Usuário.' }),
    ).toBeVisible()
  })
})
