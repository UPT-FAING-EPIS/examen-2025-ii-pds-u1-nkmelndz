# Plataforma de Matrícula de Cursos Online

![Review Assignment Due Date](https://classroom.github.com/assets/deadline-readme-button-22041afd0340ce965d47ae6ef1cefeee28c7c493a6346c4f15d667ab976d596c.svg)
![Open in Codespaces](https://classroom.github.com/assets/launch-codespace-2972f46106e565e64193e422d61a12cf1da4916b45550586e14ef0a7c637dd04.svg)

## Objetivo
Aplicación web para gestionar un catálogo de cursos, permitir la matrícula de usuarios, el seguimiento básico de progreso y la administración de cursos por instructores / administradores.

## Stack Tecnológico (Propuesto)
- Backend: ASP.NET Core 8 (REST API) siguiendo principios de Clean Architecture.
- Persistencia: Entity Framework Core (PostgreSQL en Amazon RDS). Para desarrollo inicial se podrá usar InMemory / SQLite.
- Frontend: React + Vite + TypeScript.
- Autenticación / Autorización: JWT (roles: Student, Instructor, Admin).
- Infraestructura: Terraform (AWS) – S3 (static website), CloudFront (CDN), Backend en AWS Elastic Beanstalk (o AWS App Runner), RDS (PostgreSQL), IAM Roles/Policies, Security Groups, CloudWatch Logs.
- CI/CD: GitHub Actions (workflows para infraestructura, build/test, análisis SonarQube, diagramas, documentación, release y despliegue).
- Documentación: DocFX + GitHub Pages.
- Calidad: SonarQube (0 bugs, 0 vulnerabilidades, 0 hotspots, ≥90% coverage, ≤10 líneas duplicadas).

## Principales Entidades (Dominio Inicial)
- Course: Id, Title, Description, Category, Level, Syllabus, DurationHours, Price, CreatedAt, InstructorId, IsPublished.
- Enrollment: Id, CourseId, UserId, EnrolledAt, ProgressPercent, Status.
- User (simplificado al inicio): Id, Email, PasswordHash, FullName, Role.

## Endpoints Iniciales (MVP)
```
GET    /api/courses              -> Listar cursos (filtros: category, level, q)
GET    /api/courses/{id}         -> Detalle de curso
POST   /api/courses              -> Crear curso (Instructor/Admin)
PUT    /api/courses/{id}         -> Editar curso (Instructor/Admin)
DELETE /api/courses/{id}         -> Eliminar curso (Admin)
POST   /api/enrollments          -> Matricular usuario autenticado
GET    /api/enrollments/my       -> Listar mis matrículas
POST   /api/auth/login           -> Obtener JWT
POST   /api/auth/register        -> Registrar usuario (estudiante)
```

## Arquitectura (Clean Architecture / Hexagonal Inspirada)
Capas propuestas:
1. Domain: Entidades, Value Objects, Interfaces (abstracciones de repositorios), reglas de negocio puras.
2. Application: Casos de uso (services / handlers), DTOs, validaciones, mapeos.
3. Infrastructure: EF Core DbContext, implementaciones de repositorios, migraciones, proveedores externos (RDS, S3 si aplica para assets en el futuro).
4. Api (Presentation): Controladores, DTOs request/response, configuración DI, autenticación, validación.

Beneficios: Testeabilidad, separación de responsabilidades, facilidad de evolución.

## Estructura de Carpetas (Objetivo)
```
backend/
  src/
    OnlineCourses.sln
    OnlineCourses.Domain/
    OnlineCourses.Application/
    OnlineCourses.Infrastructure/
    OnlineCourses.Api/
  tests/
    OnlineCourses.Tests/
frontend/
  package.json
  src/
infra/
  main.tf
  variables.tf
  outputs.tf
docs/
  diagrams/
    infra.png (auto-generado)
    class_diagram.puml
    class_diagram.png (auto-generado)
  api/ (docfx site source luego)
.github/workflows/
  infra.yml
  infra_diagram.yml
  class_diagram.yml
  sonar.yml
  publish_doc.yml
  deploy_app.yml
  release.yml
sonar-project.properties
.editorconfig
```

## Flujo CI/CD Resumido
1. Pull Request: Compila backend, ejecuta pruebas, análisis Sonar (quality gate).
2. Merge a main: Despliega infraestructura (si cambia) y genera diagramas + documentación.
3. Tag versión (vX.Y.Z): Genera release y despliega aplicación (backend a Elastic Beanstalk/App Runner, frontend a S3 + invalidación CloudFront).

## Workflows (Detalle)
| Workflow | Archivo | Trigger principal | Propósito | Artefactos / Resultado |
|----------|---------|-------------------|-----------|------------------------|
| Infraestructura | `.github/workflows/infra.yml` | push a `infra/**` o dispatch | Terraform fmt, init, validate, plan (apply manual) | plan.tfplan (artifact), outputs |
| Diagrama Infra | `.github/workflows/infra_diagram.yml` | push `infra/**/*.tf` | Generar `infra.dot` y `infra.png` | `docs/diagrams/infra.png` commit |
| Diagrama Clases | `.github/workflows/class_diagram.yml` | push dominios / puml | PlantUML -> PNG | `docs/diagrams/class_diagram.png` commit |
| Sonar Calidad | `.github/workflows/sonar.yml` | PR, push main | Build, tests, cobertura, análisis | Reporte SonarQube / SonarCloud |
| Publicar Docs | `.github/workflows/publish_doc.yml` | push main docs/backend | Generar DocFX y publicar Pages | Sitio GitHub Pages |
| Despliegue App | `.github/workflows/deploy_app.yml` | push main (backend/frontend) | Construir y desplegar backend + frontend | Binarios publish / artefactos S3 |
| Release | `.github/workflows/release.yml` | tag `v*` | Empaquetar y crear release | `backend.zip`, `frontend.zip` |

Notas:
- `infra.yml` sólo hace `apply` vía `workflow_dispatch` con input `apply=true` (control manual).
- Los placeholders `echo` en `deploy_app.yml` deben reemplazarse por comandos reales `aws` una vez configurados los buckets y nombres definitivos.
- Los diagramas sólo generan commits si se detectan cambios (evita ruido en el historial).

## Secrets y Variables Requeridas

AWS / Despliegue:
- `AWS_ACCESS_KEY_ID`
- `AWS_SECRET_ACCESS_KEY`
- `AWS_REGION` (ej: `us-east-1`)
- `FRONTEND_BUCKET` (salida Terraform `frontend_bucket`)
- `CLOUDFRONT_DISTRIBUTION_ID` (salida Terraform)
- `EB_APPLICATION_NAME` (Elastic Beanstalk Application)
- `EB_ENVIRONMENT_NAME` (Elastic Beanstalk Environment)
- (Opcional) `EB_DEPLOY_BUCKET` (bucket para versiones EB si se usa flujo empaquetado)

Base de Datos:
- `RDS_USERNAME`
- `RDS_PASSWORD`

Calidad:
- `SONAR_TOKEN`
- `SONAR_HOST_URL` (ej. `https://sonarcloud.io` o instancia propia)

Documentación / Opcionales:
- `DOCFX_BASE_URL` (si se usa dominio personalizado para Pages)
- `JWT_SECRET` (para producción, no comprometer en código)

## Ejecución Local (Resumida) de Workflows
Simulaciones (sin apply):
```
gh workflow run infraestructura --ref main
gh workflow run sonar --ref main
```
Reemplazar secretos localmente exportándolos en terminal antes de scripts manuales.

## Buenas Prácticas Adoptadas
- Separación de capas (Clean Architecture) permite testear servicios sin stack infra.
- Terraform como fuente única de verdad infraestructura (versionado y reproducible).
- Workflows idempotentes y con control manual para operaciones destructivas (apply infra).
- Diagramas auto-generados garantizan documentación actualizada.
- SonarQube para feedback continuo y métricas de calidad objetivas.

## Cobertura de Requisitos (Mapa)
| Criterio / Requisito | Implementación / Archivo / Workflow |
|----------------------|--------------------------------------|
| Backend API REST (Cursos, Auth, Matrícula) | `OnlineCourses.Api/Program.cs`, servicios en `Application` |
| Clean Architecture | Estructura `Domain`, `Application`, `Infrastructure`, `Api` |
| Autenticación JWT | `AuthService`, configuración JWT en `Program.cs` |
| Matrícula de cursos | `EnrollmentService`, endpoints `/api/enrollments` |
| Frontend Catálogo / Matrícula | React Vite en `frontend/src` (componentes `CourseList`, `EnrollmentList`, `Login`) |
| IaC Terraform AWS | `infra/*.tf` (S3, CloudFront, RDS, EB, IAM) |
| Workflow Infra | `infra.yml` |
| Diagrama Infraestructura | `infra_diagram.yml` + `docs/diagrams/infra.png` |
| Diagrama de Clases | `class_diagram.yml` + `docs/diagrams/class_diagram.puml/png` |
| Documentación Automatizada | DocFX (`docs/docfx/**`) + `publish_doc.yml` |
| Análisis Estático / Calidad | `sonar-project.properties` + `sonar.yml` |
| Cobertura Pruebas | `dotnet test` con coverlet en workflow Sonar |
| Despliegue Aplicación | `deploy_app.yml` (placeholders a completar) |
| Release Automatizada | `release.yml` empaqueta frontend/backend |
| Calidad Objetivos (0 bugs, ≥90% coverage) | Política definida en README + Sonar (ajustar quality gate en servidor) |
| Diagrama actualizado continuamente | Workflows commit sólo si hay cambios |
| Seguridad secrets | Uso de GitHub Secrets (lista en README) |

## Próximos Pasos (Sugerencias Futuras)
- Reemplazar repositorios EF InMemory por PostgreSQL real y migraciones.
- Añadir roles y autorización granular (Instructor/Admin) en endpoints restringidos.
- Integrar OpenAPI (Swashbuckle) y publicar spec en DocFX.
- Automatizar invalidación CloudFront sólo si cambia `frontend/dist` (hash diff).
- Añadir pruebas de integración (WebApplicationFactory) y pruebas e2e ligeras.
- Mejorar monitoreo (logs estructurados + métricas / tracing futuro).


## Variables y Secrets (GitHub Actions)
Requeridos (nombres sugeridos):
- AWS_ACCESS_KEY_ID
- AWS_SECRET_ACCESS_KEY
- AWS_REGION (ej. us-east-1)
- SONAR_TOKEN
- DOCFX_BASE_URL (opcional para custom domain)
- RDS_USERNAME / RDS_PASSWORD (para cadena de conexión)
- EB_APP_NAME (si se usa Elastic Beanstalk)
- EB_ENV_NAME (entorno Elastic Beanstalk)
- APP_RUNNER_SERVICE_ARN (si se opta por App Runner en lugar de EB)
- CLOUDFRONT_DISTRIBUTION_ID (para invalidaciones tras deploy frontend)

## Roadmap (Corto Plazo)
- [ ] Scaffold backend (solución + proyectos) y entidades básicas.
- [ ] React frontend base con catálogo mock.
- [ ] Terraform recursos mínimos (S3, CloudFront, Beanstalk/App Runner, RDS, IAM roles).
- [ ] Workflows iniciales (infra, sonar, deploy, release).
- [ ] Documentación y diagramas automáticos.

## Calidad y Pruebas
- xUnit + FluentAssertions.
- Coverage con coverlet en pipeline (enviar a Sonar).
- Reglas de Sonar: sin bugs/vulnerabilidades/hotspots, ≥90% coverage, duplicación baja.

## Generación Diagrama de Clases (PlantUML)
Workflow tomará `docs/diagrams/class_diagram.puml` y producirá `class_diagram.png` automáticamente.

## Generación Diagrama Infraestructura
`terraform graph` -> DOT -> conversión a PNG (Graphviz). Workflow lo actualizará en cada cambio de infra.

## Documentación API / Código
DocFX generará sitio estático (GitHub Pages). Más adelante se integrará Swagger/OpenAPI para endpoints.

## Cómo Ejecutar Localmente (Esquema Inicial)
Backend (cuando exista):
```
cd backend/src/OnlineCourses.Api
dotnet run
```
Frontend (cuando exista):
```
cd frontend
npm install
npm run dev
```
Base de datos local (opcional): usar `dotnet ef database update` una vez existan migraciones.

## Licencia
Uso académico / educativo.

---
Este README se irá ampliando conforme se implementen los componentes (versión AWS).
