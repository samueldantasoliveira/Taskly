export type Id = string

export const TodoStatus = {
  Todo: 0,
  InProgress: 1,
  Done: 2,
  Cancelled: 3,
} as const

export type TodoStatus = (typeof TodoStatus)[keyof typeof TodoStatus]

export const ProjectStatus = {
  Active: 0,
  Inactive: 1,
  Completed: 2,
  PendingApproval: 3,
} as const

export type ProjectStatus = (typeof ProjectStatus)[keyof typeof ProjectStatus]

export interface User {
  id: Id
  name: string
  email: string
}

export interface LoginResponse {
  token: string
  expiresAt: string
  user: User
}

export interface Team {
  id: Id
  name: string
  isActive: boolean
  ownerId: Id
  userIds: Id[]
}

export interface TeamMember extends User {
  isOwner: boolean
}

export interface Project {
  id: Id
  name: string
  description: string
  ownerId: Id
  status: ProjectStatus
  teamId: Id
}

export interface TodoTask {
  id: Id
  title: string
  description: string | null
  status: TodoStatus
  projectId: Id
  assignedUserId: Id | null
  createdAt: string
  updatedAt: string
}

export interface LoginInput {
  email: string
  password: string
}

export interface RegisterInput extends LoginInput {
  name: string
}

export interface CreateTeamInput {
  name: string
}

export interface UpdateTeamInput {
  name?: string
  isActive?: boolean
}

export interface CreateProjectInput {
  name: string
  description: string
  teamId: Id
}

export interface UpdateProjectInput {
  name?: string
  description?: string
  status?: ProjectStatus
  teamId?: Id
}

export interface CreateTaskInput {
  title: string
  description: string
  projectId: Id
  assignedUserId?: Id | null
}

export interface UpdateTaskInput {
  title: string
  description: string
}

export interface UpdateUserInput {
  name?: string
  email?: string
  password?: string
}
