import { config } from '../config';
import { apiJson } from './http';
import type { LoginResponse, UserProfile } from '../types';

export async function login(email: string, password: string): Promise<LoginResponse> {
  return apiJson<LoginResponse>(config.identityUrl('/api/auth/login'), {
    method: 'POST',
    body: JSON.stringify({ email, password }),
  });
}

export async function register(
  email: string,
  password: string,
  displayName: string,
): Promise<void> {
  await apiJson<unknown>(config.identityUrl('/api/auth/register'), {
    method: 'POST',
    body: JSON.stringify({ email, password, displayName }),
  });
}

export async function me(token: string): Promise<UserProfile> {
  return apiJson<UserProfile>(config.identityUrl('/api/auth/me'), { method: 'GET' }, token);
}
