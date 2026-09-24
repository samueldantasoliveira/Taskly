export type Id = string

export interface PagedResult<T> {
  items: T[]
  totalCount: number
}

export const TodoStatus = {
  Todo: 0,
  InProgress: 1,
  Done: 2,
  Cancelled: 3,
} as const

export type TodoStatus = (typeof TodoStatus)[keyof typeof TodoStatus]

export const TaskPriority = {
  Low: 0,
  Medium: 1,
  High: 2,
} as const

export type TaskPriority = (typeof TaskPriority)[keyof typeof TaskPriority]

export const ProjectStatus = {
  Active: 0,
  Inactive: 1,
  Completed: 2,
  PendingApproval: 3,
} as const

export type ProjectStatus = (typeof ProjectStatus)[keyof typeof ProjectStatus]

export interface User {
  version?: number
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
  version?: number
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
  version?: number
  id: Id
  name: string
  description: string
  ownerId: Id
  status: ProjectStatus
  teamId: Id
}

export interface TodoTask {
  version?: number
  id: Id
  title: string
  description: string | null
  status: TodoStatus
  priority: TaskPriority
  dueDate: string | null
  projectId: Id
  assignedUserId: Id | null
  createdAt: string
  updatedAt: string
}

export interface MyWorkItem {
  id: Id
  title: string
  status: TodoStatus
  priority: TaskPriority
  dueDate: string | null
  projectId: Id
  projectName: string
  teamName: string
}

export interface MyWorkDashboard {
  todoCount: number
  inProgressCount: number
  overdueCount: number
  items: MyWorkItem[]
}

export interface ProjectActivity {
  id: Id
  actorId: Id
  actorName: string
  taskId: Id | null
  taskTitle: string
  description: string
  createdAt: string
}

export interface TaskComment { id: Id; authorId: Id; authorName: string; content: string; createdAt: string }

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
  ownerId?: string
  version?: number
  name?: string
  isActive?: boolean
}

export interface CreateProjectInput {
  name: string
  description: string
  teamId: Id
}

export interface UpdateProjectInput {
  ownerId?: string
  version?: number
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
  priority?: TaskPriority
  dueDate?: string | null
}

export interface UpdateTaskInput {
  version?: number
  title: string
  description: string
  priority: TaskPriority
  dueDate?: string | null
}

export interface UpdateUserInput {
  version?: number
  name?: string
  email?: string
  password?: string
}
