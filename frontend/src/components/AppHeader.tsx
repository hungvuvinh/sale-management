import { Activity, LogOut, Store } from 'lucide-react';
import type { HealthStatus, UserProfile } from '../types/domain';

interface AppHeaderProps {
  user: UserProfile | null;
  health: HealthStatus | null;
  onLogout: () => void;
  onOpenStorefront: () => void;
}

export function AppHeader({ user, health, onLogout, onOpenStorefront }: AppHeaderProps) {
  const isConnected = health?.databaseConnected === true;

  return (
    <header className="app-header">
      <div className="brand-lockup">
        <div className="brand-mark"><Store className="h-5 w-5" /></div>
        <div><strong>Sale Management</strong><span>Store operations workspace</span></div>
      </div>
      <div className="header-actions">
        <button type="button" className="quiet-action" onClick={onOpenStorefront}>Mở storefront</button>
        <div className={`health-badge ${isConnected ? 'health-online' : 'health-offline'}`}>
          <Activity className="h-4 w-4" aria-hidden="true" />
          {isConnected ? 'API connected' : 'API offline'}
        </div>
        {user && <button type="button" className="logout-action" onClick={onLogout}><LogOut className="h-4 w-4" /> Đăng xuất</button>}
      </div>
    </header>
  );
}
