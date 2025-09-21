import { useState } from 'react';
import { CourseList } from '../components/CourseList';
import { EnrollmentList } from '../components/EnrollmentList';
import { Login } from '../components/Login';

export function App() {
  const [token, setToken] = useState<string | null>(null);
  const [view, setView] = useState<'courses' | 'enrollments'>('courses');

  return (
    <div style={{ fontFamily: 'system-ui', margin: '1rem' }}>
      <h1>Online Courses</h1>
      <Login onToken={setToken} />
      <nav style={{ marginBottom: '1rem' }}>
        <button onClick={() => setView('courses')}>Cursos</button>
        <button onClick={() => setView('enrollments')} disabled={!token}>Mis Matrículas</button>
      </nav>
      {view === 'courses' && <CourseList token={token} />}
      {view === 'enrollments' && token && <EnrollmentList token={token} />}
    </div>
  );
}
