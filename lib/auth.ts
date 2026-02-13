"use client";

const TOKEN_KEY = "taskly_token";

export function getToken(): string | null {
  if (typeof window === "undefined") return null;
  return localStorage.getItem(TOKEN_KEY);
}

export function setToken(token: string): void {
  localStorage.setItem(TOKEN_KEY, token);
}

export function removeToken(): void {
  localStorage.removeItem(TOKEN_KEY);
}

export function isAuthenticated(): boolean {
  const token = getToken();
  if (!token) return false;

  try {
    const payload = JSON.parse(atob(token.split(".")[1]));
    const exp = payload.exp * 1000;
    if (Date.now() >= exp) {
      removeToken();
      return false;
    }
    return true;
  } catch {
    return false;
  }
}

export function getUserFromToken(): { name?: string; email?: string } | null {
  const token = getToken();
  if (!token) return null;

  try {
    const payload = JSON.parse(atob(token.split(".")[1]));
    return {
      name:
        payload[
          "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name"
        ] || payload.unique_name,
      email:
        payload[
          "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress"
        ] || payload.email,
    };
  } catch {
    return null;
  }
}
