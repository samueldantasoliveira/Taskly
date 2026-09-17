import { randomUUID } from 'node:crypto'
import { expect, type APIRequestContext, type Page } from '@playwright/test'

const apiUrl = 'http://127.0.0.1:5220'

// Os dados são preparados pela API real, sempre com um usuário exclusivo.
export async function createUser(request: APIRequestContext) {
  const credentials = {
    name: 'Usuário E2E',
    email: `e2e-${randomUUID()}@taskly.test`,
    password: 'SenhaE2E123!',
  }
  const registration = await request.post(`${apiUrl}/api/user`, { data: credentials })
  await expect(registration).toBeOK()
  const login = await request.post(`${apiUrl}/api/login`, { data: credentials })
  await expect(login).toBeOK()
  const session = await login.json() as { token: string; user: { id: string } }
  return { ...credentials, id: session.user.id, token: session.token }
}

export async function post(
  request: APIRequestContext,
  token: string,
  path: string,
  data?: object,
) {
  const response = await request.post(`${apiUrl}/api/${path}`, {
    headers: { Authorization: `Bearer ${token}` },
    data,
  })
  await expect(response).toBeOK()
  return response
}

export async function createProject(request: APIRequestContext) {
  const user = await createUser(request)
  const team = await (await post(request, user.token, 'team', { name: 'Equipe E2E' })).json()
  const project = await (await post(request, user.token, 'project', {
    name: 'Projeto E2E', description: 'Projeto exclusivo deste teste', teamId: team.id,
  })).json() as { id: string }
  return { user, teamId: team.id as string, projectId: project.id }
}

export async function createTask(
  request: APIRequestContext,
  context: Awaited<ReturnType<typeof createProject>>,
  title: string,
  assignedUserId: string | null = context.user.id,
) {
  return await (await post(request, context.user.token, 'todotask', {
    title, description: 'Tarefa preparada para o teste',
    projectId: context.projectId, assignedUserId,
  })).json() as { id: string }
}

export async function signIn(page: Page, user: { email: string; password: string }) {
  await page.goto('/login')
  await page.getByLabel('E-mail').fill(user.email)
  await page.getByLabel('Senha').fill(user.password)
  await page.getByRole('button', { name: 'Entrar', exact: true }).click()
  await expect(page).toHaveURL(/\/teams$/)
}
