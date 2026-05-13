export type TaskStatus = 'Pending' | 'InProgress' | 'Done' | 'Cancelled';

export interface TaskItem {
  id: string;
  userId: string;
  title: string;
  description: string;
  status: TaskStatus;
  dueDateUtc: string;
  createdAtUtc: string;
  updatedAtUtc: string;
}

export interface LoginResponse {
  accessToken: string;
  userId: string;
  email: string;
  displayName: string;
  expiresAtUtc: string;
}

export interface UserProfile {
  userId: string;
  email: string;
  displayName: string;
  createdAtUtc: string;
}
