const identityBase =
  import.meta.env.VITE_IDENTITY_API_URL?.replace(/\/$/, '') ?? 'http://localhost:5001';

const tasksBase =
  import.meta.env.VITE_TASKS_API_URL?.replace(/\/$/, '') ?? 'http://localhost:5002';

export const config = {
  identityBase,
  tasksBase,
  identityUrl: (path: string) => `${identityBase}${path.startsWith('/') ? path : `/${path}`}`,
  tasksUrl: (path: string) => `${tasksBase}${path.startsWith('/') ? path : `/${path}`}`,
};
