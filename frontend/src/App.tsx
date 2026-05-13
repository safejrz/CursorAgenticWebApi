import { useCallback, useEffect, useMemo, useState } from 'react';
import { login, me, register } from './api/authApi';
import { createTask, deleteTask, listTasks, updateTask } from './api/tasksApi';
import { readToken, writeToken } from './auth/tokenStorage';
import { ApiError } from './api/http';
import type { TaskItem, TaskStatus } from './types';
import './App.css';

type Screen = 'tasks' | 'login' | 'register';

function formatError(err: unknown): string {
  if (err instanceof ApiError) {
    try {
      const parsed = JSON.parse(err.body) as { detail?: string; title?: string };
      if (parsed.detail) return parsed.detail;
    } catch {
      // ignore
    }
    return err.message;
  }
  if (err instanceof Error) return err.message;
  return 'Something went wrong.';
}

export default function App() {
  const [screen, setScreen] = useState<Screen>('login');
  const [token, setToken] = useState<string | null>(() => readToken());
  const [profileEmail, setProfileEmail] = useState<string | null>(null);
  const [tasks, setTasks] = useState<TaskItem[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const isAuthed = Boolean(token);

  const refreshProfile = useCallback(async (accessToken: string) => {
    const profile = await me(accessToken);
    setProfileEmail(profile.email);
  }, []);

  const refreshTasks = useCallback(async (accessToken: string) => {
    const list = await listTasks(accessToken);
    setTasks(list);
  }, []);

  useEffect(() => {
    if (!token) {
      setProfileEmail(null);
      setTasks([]);
      return;
    }

    let cancelled = false;
    (async () => {
      setLoading(true);
      setError(null);
      try {
        await refreshProfile(token);
        await refreshTasks(token);
        if (!cancelled) setScreen('tasks');
      } catch (err) {
        if (!cancelled) {
          writeToken(null);
          setToken(null);
          setError(formatError(err));
          setScreen('login');
        }
      } finally {
        if (!cancelled) setLoading(false);
      }
    })();

    return () => {
      cancelled = true;
    };
  }, [token, refreshProfile, refreshTasks]);

  const handleLogin = async (email: string, password: string) => {
    setLoading(true);
    setError(null);
    try {
      const response = await login(email, password);
      writeToken(response.accessToken);
      setToken(response.accessToken);
    } catch (err) {
      setError(formatError(err));
    } finally {
      setLoading(false);
    }
  };

  const handleRegister = async (email: string, password: string, displayName: string) => {
    setLoading(true);
    setError(null);
    try {
      await register(email, password, displayName);
      await handleLogin(email, password);
    } catch (err) {
      setError(formatError(err));
    } finally {
      setLoading(false);
    }
  };

  const handleLogout = () => {
    writeToken(null);
    setToken(null);
    setScreen('login');
    setTasks([]);
    setProfileEmail(null);
  };

  const handleCreateOrUpdate = async (draft: TaskDraft, editingId: string | null) => {
    if (!token) return;
    setLoading(true);
    setError(null);
    try {
      const payload = {
        title: draft.title,
        description: draft.description,
        status: draft.status,
        dueDateUtc: draft.dueDateUtc,
      };
      if (editingId) await updateTask(token, editingId, payload);
      else await createTask(token, payload);
      await refreshTasks(token);
    } catch (err) {
      setError(formatError(err));
    } finally {
      setLoading(false);
    }
  };

  const handleDelete = async (id: string) => {
    if (!token) return;
    if (!window.confirm('Delete this task?')) return;
    setLoading(true);
    setError(null);
    try {
      await deleteTask(token, id);
      await refreshTasks(token);
    } catch (err) {
      setError(formatError(err));
    } finally {
      setLoading(false);
    }
  };

  const header = useMemo(
    () => (
      <header className="app-header">
        <div className="brand">
          <strong>Task Desk</strong>
          <span className="muted">CursorAgenticWebApi</span>
        </div>
        <div className="header-actions">
          {isAuthed ? (
            <>
              <span className="muted">{profileEmail}</span>
              <button type="button" className="ghost" onClick={() => setScreen('tasks')}>
                Tasks
              </button>
              <button type="button" onClick={handleLogout}>
                Log out
              </button>
            </>
          ) : (
            <>
              <button type="button" className={screen === 'login' ? 'active' : 'ghost'} onClick={() => setScreen('login')}>
                Log in
              </button>
              <button
                type="button"
                className={screen === 'register' ? 'active' : 'ghost'}
                onClick={() => setScreen('register')}
              >
                Register
              </button>
            </>
          )}
        </div>
      </header>
    ),
    [isAuthed, profileEmail, screen],
  );

  return (
    <div className="app-shell">
      {header}
      <main className="app-main">
        {error ? (
          <div className="banner error" role="alert">
            {error}
          </div>
        ) : null}

        {!isAuthed && screen === 'login' ? (
          <AuthCard
            title="Log in"
            submitLabel={loading ? 'Signing in…' : 'Sign in'}
            onSubmit={handleLogin}
            disabled={loading}
            mode="login"
          />
        ) : null}

        {!isAuthed && screen === 'register' ? (
          <AuthCard
            title="Create an account"
            submitLabel={loading ? 'Creating…' : 'Create account'}
            onSubmit={handleRegister}
            disabled={loading}
            mode="register"
          />
        ) : null}

        {isAuthed && screen === 'tasks' ? (
          <TaskBoard
            tasks={tasks}
            loading={loading}
            onSave={handleCreateOrUpdate}
            onDelete={handleDelete}
          />
        ) : null}
      </main>
    </div>
  );
}

interface TaskDraft {
  title: string;
  description: string;
  status: TaskStatus;
  dueDateUtc: string | null;
}

function AuthCard(props: {
  title: string;
  submitLabel: string;
  disabled: boolean;
  mode: 'login' | 'register';
  onSubmit: (...args: string[]) => Promise<void> | void;
}) {
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [displayName, setDisplayName] = useState('');

  return (
    <section className="card auth-card">
      <h1>{props.title}</h1>
      <form
        className="form"
        onSubmit={(e) => {
          e.preventDefault();
          if (props.mode === 'register') void props.onSubmit(email, password, displayName);
          else void props.onSubmit(email, password);
        }}
      >
        {props.mode === 'register' ? (
          <label className="field">
            <span>Display name</span>
            <input
              value={displayName}
              onChange={(e) => setDisplayName(e.target.value)}
              autoComplete="name"
              required
            />
          </label>
        ) : null}
        <label className="field">
          <span>Email</span>
          <input
            type="email"
            value={email}
            onChange={(e) => setEmail(e.target.value)}
            autoComplete="email"
            required
          />
        </label>
        <label className="field">
          <span>Password</span>
          <input
            type="password"
            value={password}
            onChange={(e) => setPassword(e.target.value)}
            autoComplete={props.mode === 'register' ? 'new-password' : 'current-password'}
            required
            minLength={8}
          />
        </label>
        <button type="submit" className="primary" disabled={props.disabled}>
          {props.submitLabel}
        </button>
      </form>
    </section>
  );
}

function TaskBoard(props: {
  tasks: TaskItem[];
  loading: boolean;
  onSave: (draft: TaskDraft, editingId: string | null) => Promise<void>;
  onDelete: (id: string) => Promise<void>;
}) {
  const [editing, setEditing] = useState<TaskItem | null>(null);
  const [draft, setDraft] = useState<TaskDraft>({
    title: '',
    description: '',
    status: 'Pending',
    dueDateUtc: null,
  });

  const resetDraft = () => {
    setEditing(null);
    setDraft({ title: '', description: '', status: 'Pending', dueDateUtc: null });
  };

  const startEdit = (task: TaskItem) => {
    setEditing(task);
    setDraft({
      title: task.title,
      description: task.description,
      status: task.status,
      dueDateUtc: task.dueDateUtc,
    });
  };

  return (
    <section className="tasks-layout">
      <div className="card form-card">
        <div className="form-card-header">
          <h2>{editing ? 'Edit task' : 'New task'}</h2>
          {editing ? (
            <button type="button" className="ghost" onClick={resetDraft}>
              Cancel edit
            </button>
          ) : null}
        </div>
        <form
          className="form"
          onSubmit={(e) => {
            e.preventDefault();
            void props.onSave(draft, editing?.id ?? null).then(() => {
              if (editing) resetDraft();
              else setDraft({ title: '', description: '', status: 'Pending', dueDateUtc: null });
            });
          }}
        >
          <label className="field">
            <span>Title</span>
            <input
              value={draft.title}
              onChange={(e) => setDraft((d) => ({ ...d, title: e.target.value }))}
              required
              maxLength={500}
            />
          </label>
          <label className="field">
            <span>Description</span>
            <textarea
              value={draft.description}
              onChange={(e) => setDraft((d) => ({ ...d, description: e.target.value }))}
              rows={4}
              maxLength={4000}
            />
          </label>
          <div className="field-row">
            <label className="field">
              <span>Status</span>
              <select
                value={draft.status}
                onChange={(e) => setDraft((d) => ({ ...d, status: e.target.value as TaskStatus }))}
              >
                <option value="Pending">Pending</option>
                <option value="InProgress">In progress</option>
                <option value="Done">Done</option>
                <option value="Cancelled">Cancelled</option>
              </select>
            </label>
            <label className="field">
              <span>Due (UTC)</span>
              <input
                type="datetime-local"
                value={draft.dueDateUtc ? toLocalInput(draft.dueDateUtc) : ''}
                onChange={(e) =>
                  setDraft((d) => ({
                    ...d,
                    dueDateUtc: e.target.value ? fromLocalInput(e.target.value) : null,
                  }))
                }
              />
            </label>
          </div>
          <button type="submit" className="primary" disabled={props.loading}>
            {editing ? 'Save changes' : 'Create task'}
          </button>
        </form>
      </div>

      <div className="card list-card">
        <div className="list-header">
          <h2>Your tasks</h2>
          {props.loading ? <span className="muted">Updating…</span> : null}
        </div>
        <div className="task-grid">
          {props.tasks.map((task) => (
            <article key={task.id} className="task-card">
              <header>
                <h3>{task.title}</h3>
                <span className={`pill status-${task.status}`}>{task.status}</span>
              </header>
              <p className="muted">{task.description || 'No description'}</p>
              <dl className="meta">
                <div>
                  <dt>Due</dt>
                  <dd>{new Date(task.dueDateUtc).toLocaleString()}</dd>
                </div>
                <div>
                  <dt>Updated</dt>
                  <dd>{new Date(task.updatedAtUtc).toLocaleString()}</dd>
                </div>
              </dl>
              <div className="task-actions">
                <button type="button" className="ghost" onClick={() => startEdit(task)}>
                  Edit
                </button>
                <button type="button" className="danger" onClick={() => void props.onDelete(task.id)}>
                  Delete
                </button>
              </div>
            </article>
          ))}
          {props.tasks.length === 0 && !props.loading ? (
            <p className="muted empty">No tasks yet. Add your first one on the left.</p>
          ) : null}
        </div>
      </div>
    </section>
  );
}

function toLocalInput(iso: string): string {
  const date = new Date(iso);
  const pad = (n: number) => n.toString().padStart(2, '0');
  return `${date.getFullYear()}-${pad(date.getMonth() + 1)}-${pad(date.getDate())}T${pad(date.getHours())}:${pad(date.getMinutes())}`;
}

function fromLocalInput(value: string): string {
  const date = new Date(value);
  return date.toISOString();
}
