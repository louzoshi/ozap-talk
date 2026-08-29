// Thin fetch wrapper. Same-origin in dev (via Vite proxy) and in production
// (SPA served by the ASP.NET host), so no base URL is needed.
export class ApiError extends Error {
  constructor(
    public readonly status: number,
    public readonly problem: unknown,
  ) {
    super(`API ${status}`);
  }
}

export async function api<T>(path: string, init?: RequestInit): Promise<T> {
  const res = await fetch(`/api${path}`, {
    ...init,
    headers: { "Content-Type": "application/json", ...init?.headers },
  });

  if (!res.ok) {
    throw new ApiError(res.status, await res.json().catch(() => null));
  }

  return res.status === 204 ? (undefined as T) : ((await res.json()) as T);
}
