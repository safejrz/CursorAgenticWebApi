const key = 'ca_access_token';

export function readToken(): string | null {
  try {
    return localStorage.getItem(key);
  } catch {
    return null;
  }
}

export function writeToken(token: string | null): void {
  try {
    if (token) localStorage.setItem(key, token);
    else localStorage.removeItem(key);
  } catch {
    // ignore private mode / quota
  }
}
