locals {
  name_prefix = "${var.project_name}-${var.environment}"
}

# S3 para frontend
resource "aws_s3_bucket" "frontend" {
  bucket = "${local.name_prefix}-frontend"
  force_destroy = true
}

resource "aws_s3_bucket_website_configuration" "frontend" {
  bucket = aws_s3_bucket.frontend.id
  index_document { suffix = "index.html" }
  error_document { key = "index.html" }
}

resource "aws_s3_bucket_policy" "frontend_policy" {
  bucket = aws_s3_bucket.frontend.id
  policy = jsonencode({
    Version = "2012-10-17"
    Statement = [
      {
        Effect = "Allow"
        Principal = "*"
        Action = ["s3:GetObject"]
        Resource = ["${aws_s3_bucket.frontend.arn}/*"]
      }
    ]
  })
}

# CloudFront para el bucket frontend
resource "aws_cloudfront_origin_access_control" "oac" {
  name                              = "${local.name_prefix}-oac"
  description                       = "OAC for frontend"
  origin_access_control_origin_type = "s3"
  signing_behavior                  = "always"
  signing_protocol                  = "sigv4"
}

resource "aws_cloudfront_distribution" "cdn" {
  enabled             = true
  default_root_object = "index.html"

  origin {
    domain_name              = aws_s3_bucket.frontend.bucket_regional_domain_name
    origin_id                = "frontend-s3"
    origin_access_control_id = aws_cloudfront_origin_access_control.oac.id
  }

  default_cache_behavior {
    allowed_methods  = ["GET", "HEAD"]
    cached_methods   = ["GET", "HEAD"]
    target_origin_id = "frontend-s3"
    viewer_protocol_policy = "redirect-to-https"
    forwarded_values { query_string = false cookies { forward = "none" } }
  }

  restrictions { geo_restriction { restriction_type = "none" } }

  viewer_certificate {
    cloudfront_default_certificate = true
  }
}

# RDS PostgreSQL
resource "aws_db_subnet_group" "db_subnet" {
  name       = "${local.name_prefix}-db-subnet"
  subnet_ids = data.aws_subnets.default.ids
}

data "aws_vpc" "default" { default = true }

data "aws_subnets" "default" { filter { name = "vpc-id" values = [data.aws_vpc.default.id] } }

data "aws_availability_zones" "available" {}

resource "aws_security_group" "db_sg" {
  name        = "${local.name_prefix}-db-sg"
  description = "DB access"
  vpc_id      = data.aws_vpc.default.id

  ingress {
    description = "Postgres"
    from_port   = 5432
    to_port     = 5432
    protocol    = "tcp"
    cidr_blocks = ["0.0.0.0/0"] # Simplificado (restringir en prod)
  }

  egress {
    from_port   = 0
    to_port     = 0
    protocol    = "-1"
    cidr_blocks = ["0.0.0.0/0"]
  }
}

resource "aws_db_instance" "postgres" {
  identifier              = "${local.name_prefix}-db"
  engine                  = "postgres"
  engine_version          = "15.5"
  instance_class          = var.instance_class
  username                = var.rds_username
  password                = var.rds_password
  allocated_storage       = var.allocated_storage
  db_subnet_group_name    = aws_db_subnet_group.db_subnet.name
  vpc_security_group_ids  = [aws_security_group.db_sg.id]
  publicly_accessible     = true
  skip_final_snapshot     = true
}

# Elastic Beanstalk (Aplicación y Entorno) - backend
resource "aws_iam_role" "eb_role" {
  name               = "${local.name_prefix}-eb-role"
  assume_role_policy = data.aws_iam_policy_document.eb_assume.json
}

data "aws_iam_policy_document" "eb_assume" {
  statement {
    actions = ["sts:AssumeRole"]
    principals { type = "Service" identifiers = ["ec2.amazonaws.com"] }
  }
}

resource "aws_iam_instance_profile" "eb_instance_profile" {
  name = "${local.name_prefix}-eb-instance-profile"
  role = aws_iam_role.eb_role.name
}

resource "aws_elastic_beanstalk_application" "app" {
  name        = "${local.name_prefix}-api"
  description = "Backend API"
}

resource "aws_elastic_beanstalk_environment" "env" {
  name                = "${local.name_prefix}-env"
  application         = aws_elastic_beanstalk_application.app.name
  solution_stack_name = "64bit Amazon Linux 2023 v4.0.1 running .NET 8"

  setting { namespace = "aws:autoscaling:launchconfiguration" name = "IamInstanceProfile" value = aws_iam_instance_profile.eb_instance_profile.name }
  setting { namespace = "aws:elasticbeanstalk:application:environment" name = "ASPNETCORE_ENVIRONMENT" value = var.environment }
  setting { namespace = "aws:elasticbeanstalk:application:environment" name = "ConnectionStrings__Default" value = "Host=${aws_db_instance.postgres.address};Database=app;Username=${var.rds_username};Password=${var.rds_password}" }
}
