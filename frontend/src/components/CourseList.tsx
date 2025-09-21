import { useEffect, useState } from 'react';
import { enrollInCourse, fetchCourses } from '../lib/api';

interface Course {
  id: string;
  title: string;
  description: string;
  category: string;
  level: string;
  durationHours: number;
  price: number;
  isPublished: boolean;
}

export function CourseList({ token }: { token: string | null }) {
  const [courses, setCourses] = useState<Course[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    setLoading(true);
    fetchCourses()
      .then(setCourses)
      .catch(e => setError(e.message))
      .finally(() => setLoading(false));
  }, []);

  if (loading) return <p>Cargando cursos...</p>;
  if (error) return <p style={{ color: 'red' }}>{error}</p>;

  return (
    <div>
      <h2>Cursos</h2>
      {courses.length === 0 && <p>No hay cursos.</p>}
      <ul>
        {courses.map(c => (
          <li key={c.id} style={{ marginBottom: '0.5rem' }}>
            <strong>{c.title}</strong> ({c.level}) - {c.category} - {c.durationHours}h - ${'{'}c.price{'}'}
            <br />
            <small>{c.description}</small>
            <br />
            {token && (
              <button onClick={async () => {
                try {
                  await enrollInCourse(c.id, token);
                  alert('Matriculado');
                } catch (e: any) {
                  alert(e.message);
                }
              }}>Matricular</button>
            )}
          </li>
        ))}
      </ul>
    </div>
  );
}
