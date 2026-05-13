export class ApiError extends Error {
  readonly status: number;

  readonly body: string;

  constructor(status: number, body: string) {
    super(body || `HTTP ${status}`);
    this.status = status;
    this.body = body;
  }
}

export async function apiJson<T>(
  url: string,
  init: RequestInit = {},
  token?: string | null,
): Promise<T> {
  const headers = new Headers(init.headers);
  if (!headers.has('Content-Type') && init.body && !(init.body instanceof FormData)) {
    headers.set('Content-Type', 'application/json');
  }
  if (token) headers.set('Authorization', `Bearer ${token}`);

  const response = await fetch(url, { ...init, headers });
  const text = await response.text();

  if (!response.ok) {
    throw new ApiError(response.status, text);
  }

  if (response.status === 204 || text.length === 0) {
    return undefined as T;
  }

  return JSON.parse(text) as T;
}
