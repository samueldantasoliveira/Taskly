import { getToken } from "./auth";
import type {
  CreateProjectDto,
  CreateTeamDto,
  CreateTodoTaskDto,
  CreateUserDto,
  Project,
  Team,
  TodoTask,
  UpdateTodoTaskDto,
} from "./types";

const BASE = "/api";

async function request<T>(
  url: string,
  options: RequestInit = {}
): Promise<T> {
  const token = getToken();
  const headers: Record<string, string> = {
    "Content-Type": "application/json",
    ...(options.headers as Record<string, string>),
  };

  if (token) {
    headers["Authorization"] = `Bearer ${token}`;
  }

  const res = await fetch(`${BASE}${url}`, {
    ...options,
    headers,
  });

  if (!res.ok) {
    const text = await res.text();
    throw new Error(text || `Request failed with status ${res.status}`);
  }

  const contentType = res.headers.get("Content-Type");
  if (contentType && contentType.includes("application/json")) {
    return res.json();
  }

  const text = await res.text();
  return text as unknown as T;
}

// ---- Auth ----

export async function login(
  email: string,
  password: string
): Promise<string> {
  return request<string>(
    `/Login?email=${encodeURIComponent(email)}&password=${encodeURIComponent(password)}`,
    { method: "POST" }
  );
}

// ---- Users ----

export async function createUser(dto: CreateUserDto) {
  return request<unknown>("/User", {
    method: "POST",
    body: JSON.stringify(dto),
  });
}

// ---- Teams ----

export async function createTeam(dto: CreateTeamDto): Promise<Team> {
  return request<Team>("/Team", {
    method: "POST",
    body: JSON.stringify(dto),
  });
}

export async function addMemberToTeam(
  teamId: string,
  userId: string
): Promise<unknown> {
  return request<unknown>(
    `/Team/${teamId}/add-member?userId=${encodeURIComponent(userId)}`,
    { method: "POST" }
  );
}

// ---- Projects ----

export async function createProject(
  dto: CreateProjectDto
): Promise<Project> {
  return request<Project>("/Project", {
    method: "POST",
    body: JSON.stringify(dto),
  });
}

// ---- Tasks ----

export async function createTodoTask(
  dto: CreateTodoTaskDto
): Promise<TodoTask> {
  return request<TodoTask>("/TodoTask", {
    method: "POST",
    body: JSON.stringify(dto),
  });
}

export async function getTodoTask(id: string): Promise<TodoTask> {
  return request<TodoTask>(`/TodoTask/${id}`);
}

export async function getTasksByProject(
  projectId: string
): Promise<TodoTask[]> {
  return request<TodoTask[]>(`/TodoTask/project/${projectId}`);
}

export async function updateTodoTask(
  id: string,
  dto: UpdateTodoTaskDto
): Promise<TodoTask> {
  return request<TodoTask>(`/TodoTask/${id}`, {
    method: "PUT",
    body: JSON.stringify(dto),
  });
}
