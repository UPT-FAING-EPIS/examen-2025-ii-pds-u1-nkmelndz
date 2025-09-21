const API_BASE = import.meta.env.VITE_API_BASE || 'http://localhost:5000';

export async function fetchCourses() {
  const r = await fetch(`${API_BASE}/api/courses`);
  if (!r.ok) throw new Error('Error al obtener cursos');
  return r.json();
}

export async function enrollInCourse(courseId: string, token: string) {
  const r = await fetch(`${API_BASE}/api/enrollments?courseId=${courseId}`, {
    method: 'POST',
    headers: { 'Authorization': `Bearer ${token}` }
  });
  if (!r.ok) throw new Error('No se pudo matricular');
  return r.json();
}

export async function fetchEnrollments(token: string) {
  const r = await fetch(`${API_BASE}/api/enrollments/my`, {
    headers: { 'Authorization': `Bearer ${token}` }
  });
  if (!r.ok) throw new Error('Error al obtener matrículas');
  return r.json();
}

export async function login(email: string, password: string) {
  const r = await fetch(`${API_BASE}/api/auth/login`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ email, password })
  });
  if (!r.ok) throw new Error('Credenciales inválidas');
  return r.json();
}

export async function registerUser(email: string, fullName: string, password: string) {
  const r = await fetch(`${API_BASE}/api/auth/register`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ email, fullName, password })
  });
  if (!r.ok) throw new Error('No se pudo registrar');
  return r.json();
}
