import { KeyRound, Lock, User } from 'lucide-react';
import { useState } from 'react';

interface LoginFormProps {
  isLoading: boolean;
  error: string | null;
  onSubmit: (email: string, password: string) => Promise<void>;
}

export function LoginForm({ isLoading, error, onSubmit }: LoginFormProps) {
  const [email, setEmail] = useState('admin@visssoft.com.au');
  const [password, setPassword] = useState('Admin123!');

  async function handleSubmit(event: React.FormEvent<HTMLFormElement>) {
    event.preventDefault();
    await onSubmit(email, password);
  }

  return (
    <div className="login-card">
      <div className="login-heading">
        <div className="login-icon"><Lock className="h-6 w-6" /></div>
        <h2>Đăng nhập quản trị</h2>
        <p>Quản lý catalog, nhập hàng, chuyển hàng và engine giá.</p>
      </div>
      {error && <div className="alert alert-error" role="alert">{error}</div>}
      <form onSubmit={handleSubmit} className="form-stack">
        <label className="field-label" htmlFor="login-email">Email hệ thống</label>
        <div className="input-with-icon">
          <User className="h-4 w-4" aria-hidden="true" />
          <input id="login-email" type="email" value={email} onChange={(event) => setEmail(event.target.value)} required autoComplete="email" />
        </div>
        <label className="field-label" htmlFor="login-password">Mật khẩu</label>
        <div className="input-with-icon">
          <KeyRound className="h-4 w-4" aria-hidden="true" />
          <input id="login-password" type="password" value={password} onChange={(event) => setPassword(event.target.value)} required autoComplete="current-password" />
        </div>
        <button className="primary-action" type="submit" disabled={isLoading}>
          {isLoading ? 'Đang xác thực...' : 'Đăng nhập'}
        </button>
      </form>
    </div>
  );
}
