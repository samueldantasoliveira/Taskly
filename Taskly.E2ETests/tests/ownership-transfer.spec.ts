import { expect, test } from '@playwright/test'
import { createProject, createUser, post, signIn } from './helpers'

test('proprietário transfere projeto e equipe pela interface', async ({ page, request }) => {
  const context = await createProject(request)
  const member = await createUser(request)
  await post(request, context.user.token, `team/${context.teamId}/add-member?userId=${member.id}`)
  await signIn(page, context.user)
  await page.goto(`/projects/${context.projectId}`)
  await page.getByRole('button', { name: 'Editar projeto' }).click()
  const projectDialog = page.getByRole('dialog', { name: 'Editar projeto' })
  await projectDialog.getByLabel('Equipe').selectOption(context.teamId)
  await projectDialog.getByLabel('Proprietário do projeto').selectOption(member.id)
  await projectDialog.getByRole('button', { name: 'Salvar', exact: true }).click()
  await expect(projectDialog).not.toBeVisible()

  await page.goto(`/teams/${context.teamId}`)
  await page.getByRole('button', { name: 'Editar', exact: true }).click()
  const teamDialog = page.getByRole('dialog', { name: 'Editar equipe' })
  await teamDialog.getByLabel('Proprietário da equipe').selectOption(member.id)
  await teamDialog.getByRole('button', { name: 'Salvar', exact: true }).click()
  await expect(teamDialog).not.toBeVisible()
  await expect(page.getByRole('button', { name: 'Sair da equipe' })).toBeVisible()
  await page.reload()
  await expect(page.getByRole('button', { name: 'Editar', exact: true })).not.toBeVisible()

  for (const path of [`team/${context.teamId}`, `project/${context.projectId}`]) {
    const response = await request.get(`http://127.0.0.1:5220/api/${path}`, {
      headers: { Authorization: `Bearer ${member.token}` },
    })
    await expect(response).toBeOK()
    expect((await response.json()).ownerId).toBe(member.id)
  }
})
