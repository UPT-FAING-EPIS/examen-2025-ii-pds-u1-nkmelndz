import { useState } from 'react';
import { login, registerUser } from '../lib/api';

interface Props { onToken: (t: string) => void; }

export function Login({ onToken }: Props) {
  const [email, setEmail] = useState('');
  const [name, setName] = useState('');
  const [password, setPassword] = useState('');
  const [mode, setMode] = useState<'login' | 'register'>('login');
  const [error, setError] = useState<string | null>(null);

  const submit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);
    try {
      const resp = mode === 'login'
        ? await login(email, password)
        : await registerUser(email, name, password);
      onToken(resp.token);
    } catch (err: any) {
      setError(err.message);
    }
  };

  return (
    <form onSubmit={submit} style={{ marginBottom: '1rem', display: 'flex', gap: '0.5rem', flexWrap: 'wrap' }}>
      <input placeholder="Email" value={email} onChange={e => setEmail(e.target.value)} required />
      {mode === 'register' && (
        <input placeholder="Nombre" value={name} onChange={e => setName(e.target.value)} required />
      )}
      <input placeholder="Password" type="password" value={password} onChange={e => setPassword(e.target.value)} required />
      <button type="submit">{mode === 'login' ? 'Ingresar' : 'Registrar'}</button>
      <button type="button" onClick={() => setMode(m => m === 'login' ? 'register' : 'login')}>
        {mode === 'login' ? 'Crear cuenta' : 'Ya tengo cuenta'}
      </button>
      {error && <span style={{ color: 'red' }}>{error}</span>}
    </form>
  );
}
