// Thin fetch wrapper. Same-origin in dev (via Vite proxy) and in production
// (SPA served by the ASP.NET host), so no base URL is needed.

const TOKEN_KEY = "ozaptalk.token";

export const tokenStore = {
  get: () => localStorage.getItem(TOKEN_KEY),
  set: (t: string) => localStorage.setItem(TOKEN_KEY, t),
  clear: () => localStorage.removeItem(TOKEN_KEY),
};

export class ApiError extends Error {
  constructor(
    public readonly status: number,
    public readonly problem: { detail?: string; code?: string } | null,
  ) {
    super(problem?.detail ?? `API ${status}`);
  }
}

export async function api<T>(path: string, init?: RequestInit): Promise<T> {
  const token = tokenStore.get();
  const res = await fetch(`/api${path}`, {
    ...init,
    headers: {
      "Content-Type": "application/json",
      ...(token ? { Authorization: `Bearer ${token}` } : {}),
      ...init?.headers,
    },
  });

  if (res.status === 401) {
    tokenStore.clear();
    if (!location.pathname.startsWith("/login")) location.assign("/login");
    throw new ApiError(401, null);
  }

  if (!res.ok) {
    throw new ApiError(res.status, await res.json().catch(() => null));
  }

  return res.status === 204 ? (undefined as T) : ((await res.json()) as T);
}
