import { useEffect, useState } from 'react';
import { fetchEnrollments } from '../lib/api';

interface Enrollment {
  id: string;
  courseId: string;
  courseTitle: string;
  progressPercent: number;
  status: string;
  enrolledAt: string;
}

export function EnrollmentList({ token }: { token: string }) {
  const [items, setItems] = useState<Enrollment[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    setLoading(true);
    fetchEnrollments(token)
      .then(setItems)
      .catch(e => setError(e.message))
      .finally(() => setLoading(false));
  }, [token]);

  if (loading) return <p>Cargando matrículas...</p>;
  if (error) return <p style={{ color: 'red' }}>{error}</p>;

  return (
    <div>
      <h2>Mis Matrículas</h2>
      {items.length === 0 && <p>No tienes matrículas todavía.</p>}
      <ul>
        {items.map(m => (
          <li key={m.id}>
            <strong>{m.courseTitle}</strong> - {m.progressPercent}% - {m.status}
          </li>
        ))}
      </ul>
    </div>
  );
}
