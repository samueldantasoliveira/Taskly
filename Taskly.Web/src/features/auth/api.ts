import { apiRequest, jsonBody } from '../../shared/api/client'
import type {
  LoginInput,
  LoginResponse,
  RegisterInput,
  User,
} from '../../shared/types/api'

export function login(input: LoginInput) {
  return apiRequest<LoginResponse>('/api/login', {
    method: 'POST',
    body: jsonBody(input),
  })
}

export function register(input: RegisterInput) {
  return apiRequest<User>('/api/user', {
    method: 'POST',
    body: jsonBody(input),
  })
}

export function getCurrentUser(signal?: AbortSignal) {
  return apiRequest<User>('/api/user/me', { signal })
}
