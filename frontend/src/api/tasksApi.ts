import { config } from '../config';
import { apiJson } from './http';
import type { TaskItem, TaskStatus } from '../types';

export async function listTasks(token: string): Promise<TaskItem[]> {
  return apiJson<TaskItem[]>(config.tasksUrl('/api/tasks'), { method: 'GET' }, token);
}

export async function createTask(
  token: string,
  payload: { title: string; description: string; status: TaskStatus; dueDateUtc: string | null },
): Promise<TaskItem> {
  return apiJson<TaskItem>(
    config.tasksUrl('/api/tasks'),
    { method: 'POST', body: JSON.stringify(payload) },
    token,
  );
}

export async function updateTask(
  token: string,
  id: string,
  payload: { title: string; description: string; status: TaskStatus; dueDateUtc: string | null },
): Promise<TaskItem> {
  return apiJson<TaskItem>(
    config.tasksUrl(`/api/tasks/${id}`),
    { method: 'PUT', body: JSON.stringify(payload) },
    token,
  );
}

export async function deleteTask(token: string, id: string): Promise<void> {
  await apiJson<void>(
    config.tasksUrl(`/api/tasks/${id}`),
    { method: 'DELETE' },
    token,
  );
}
