import { expect, test } from '@playwright/test'
import { randomUUID } from 'node:crypto'
import { createProject, post } from './helpers'

test('novo usuário retorna ao convite depois de criar a conta', async ({ page, request }) => {
  const context = await createProject(request)
  const invitedUser = {
    name: 'Pessoa Convidada',
    email: `invite-${randomUUID()}@rivulus.test`,
    password: 'SenhaE2E123!',
  }
  const invitation = await (await post(
    request,
    context.user.token,
    `team/${context.teamId}/invitations`,
    { email: invitedUser.email },
  )).json() as { token: string }

  await page.goto(`/invitations/${invitation.token}`)
  await page.getByRole('link', { name: 'Criar conta' }).click()
  await page.getByLabel('Nome').fill(invitedUser.name)
  await page.getByLabel('E-mail').fill(invitedUser.email)
  await page.getByLabel('Senha').fill(invitedUser.password)
  await page.getByRole('button', { name: 'Criar minha conta' }).click()

  await expect(page).toHaveURL(new RegExp(`/invitations/${invitation.token}$`))
  await page.getByRole('button', { name: 'Aceitar convite' }).click()

  await expect(page).toHaveURL(new RegExp(`/teams/${context.teamId}$`))
  await expect(page.getByRole('heading', { name: 'Equipe E2E' })).toBeVisible()
})
