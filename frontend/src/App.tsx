import { useEffect, useState } from 'react';
import { apiClient } from './api/client';
import { AdminInventoryPage } from './components/AdminInventoryPage';
import { AppHeader } from './components/AppHeader';
import { LoginForm } from './components/LoginForm';
import { StorefrontPage } from './components/StorefrontPage';
import type { HealthStatus, UserProfile } from './types/domain';

type Workspace = 'admin' | 'storefront';

interface LoginResponse {
  token: string;
  user: UserProfile;
}

function getErrorMessage(error: unknown, fallback: string): string {
  if (typeof error === 'object' && error !== null && 'response' in error) {
    const response = (error as { response?: { data?: { message?: string } } }).response;
    return response?.data?.message ?? fallback;
  }
  return fallback;
}

export function App() {
  const [workspace, setWorkspace] = useState<Workspace>('admin');
  const [user, setUser] = useState<UserProfile | null>(() => {
    const storedUser = localStorage.getItem('user_profile');
    if (!storedUser) return null;
    try {
      return JSON.parse(storedUser) as UserProfile;
    } catch {
      localStorage.removeItem('user_profile');
      return null;
    }
  });
  const [health, setHealth] = useState<HealthStatus | null>(null);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    async function loadHealth(): Promise<void> {
      try {
        const response = await apiClient.get<HealthStatus>('/healthz');
        setHealth(response.data);
      } catch {
        setHealth({ status: 'disconnected', timestamp: new Date().toISOString(), environment: 'development', databaseConnected: false });
      }
    }

    void loadHealth();
  }, []);

  async function handleLogin(email: string, password: string): Promise<void> {
    setIsLoading(true);
    setError(null);
    try {
      const response = await apiClient.post<LoginResponse>('/auth/login', { email, password });
      localStorage.setItem('jwt_token', response.data.token);
      localStorage.setItem('user_profile', JSON.stringify(response.data.user));
      setUser(response.data.user);
    } catch (loginError: unknown) {
      setError(getErrorMessage(loginError, 'Đăng nhập thất bại. Vui lòng kiểm tra lại.'));
    } finally {
      setIsLoading(false);
    }
  }

  function handleLogout(): void {
    localStorage.removeItem('jwt_token');
    localStorage.removeItem('user_profile');
    setUser(null);
  }

  return (
    <div className="app-frame">
      <AppHeader user={user} health={health} onLogout={handleLogout} onOpenStorefront={() => setWorkspace('storefront')} />
      <main className="app-main">
        {!user && workspace === 'admin' ? <LoginForm isLoading={isLoading} error={error} onSubmit={handleLogin} /> : (
          <>
            {user && <div className="workspace-switcher" aria-label="Không gian làm việc">
              <button type="button" aria-pressed={workspace === 'admin'} className={workspace === 'admin' ? 'workspace-tab active' : 'workspace-tab'} onClick={() => setWorkspace('admin')}>Admin hàng hóa</button>
              <button type="button" aria-pressed={workspace === 'storefront'} className={workspace === 'storefront' ? 'workspace-tab active' : 'workspace-tab'} onClick={() => setWorkspace('storefront')}>Storefront</button>
            </div>}
            {workspace === 'storefront' ? <StorefrontPage /> : user ? <AdminInventoryPage /> : <LoginForm isLoading={isLoading} error={error} onSubmit={handleLogin} />}
          </>
        )}
      </main>
      <footer className="app-footer">Sale Management System · Catalog and store operations</footer>
    </div>
  );
}

export default App;
