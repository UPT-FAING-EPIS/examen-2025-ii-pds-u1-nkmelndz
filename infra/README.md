# Infraestructura AWS (Terraform)

Recursos incluidos (entorno simplificado para fines académicos):
- S3 (hosting frontend estático)
- CloudFront (CDN para S3)
- RDS PostgreSQL (publicly_accessible=true SOLO para desarrollo; restringir en producción)
- Elastic Beanstalk (aplicación .NET 8 backend)
- IAM Role + Instance Profile para EB

## Variables Principales
| Variable | Descripción | Default |
|----------|-------------|---------|
| project_name | Prefijo de recursos | online-courses |
| environment | Entorno (dev/stage/prod) | dev |
| aws_region | Región AWS | us-east-1 |
| rds_username | Usuario BD | (requerido) |
| rds_password | Password BD | (requerido) |

## Pasos Locales
```bash
terraform init
terraform plan -var "rds_username=USER" -var "rds_password=PASS"
terraform apply -auto-approve -var "rds_username=USER" -var "rds_password=PASS"
```

## Notas de Seguridad
- Cambiar `cidr_blocks` del SG de la base de datos a rangos internos.
- Agregar en producción: Encryption at rest (RDS), Bucket encryption, Versioning, WAF/Shield.
- Considerar App Runner / ECS Fargate como alternativa a Beanstalk.

## Próximas Mejoras
- Parameter Store / Secrets Manager para credenciales.
- SSL cert en CloudFront con dominio custom.
- Módulos reutilizables por entorno.