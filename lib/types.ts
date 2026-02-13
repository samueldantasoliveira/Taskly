// ---- Enums ----

export enum TodoStatus {
  Pending = 0,
  InProgress = 1,
  Done = 2,
}

export enum ProjectStatus {
  Active = 0,
  Inactive = 1,
  Completed = 2,
  PendingApproval = 3,
}

// ---- Entities ----

export interface User {
  id: string;
  name: string;
  email: string;
  password?: string;
}

export interface Team {
  id: string;
  name: string;
  memberIds: string[];
}

export interface Project {
  id: string;
  name: string;
  description: string;
  status: ProjectStatus;
  teamId: string;
}

export interface TodoTask {
  id: string;
  title: string;
  description: string;
  status: TodoStatus;
  projectId: string;
  assignedUserId?: string;
}

// ---- DTOs ----

export interface CreateUserDto {
  name: string;
  password: string;
  email: string;
}

export interface CreateTeamDto {
  name: string;
}

export interface CreateProjectDto {
  name: string;
  description: string;
  teamId: string;
}

export interface CreateTodoTaskDto {
  title: string;
  description: string;
  projectId: string;
  assignedUserId?: string;
}

export interface UpdateTodoTaskDto {
  title: string;
  description: string;
  status: TodoStatus;
  projectId: string;
  assignedUserId?: string;
}

// ---- Auth ----

export interface LoginResponse {
  token: string;
}

export const TODO_STATUS_LABELS: Record<TodoStatus, string> = {
  [TodoStatus.Pending]: "Pendente",
  [TodoStatus.InProgress]: "Em Progresso",
  [TodoStatus.Done]: "Concluido",
};

export const PROJECT_STATUS_LABELS: Record<ProjectStatus, string> = {
  [ProjectStatus.Active]: "Ativo",
  [ProjectStatus.Inactive]: "Inativo",
  [ProjectStatus.Completed]: "Concluido",
  [ProjectStatus.PendingApproval]: "Aguardando Aprovacao",
};
