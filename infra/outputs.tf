output "frontend_bucket" { value = aws_s3_bucket.frontend.bucket }
output "cloudfront_domain" { value = aws_cloudfront_distribution.cdn.domain_name }
output "rds_endpoint" { value = aws_db_instance.postgres.address }
output "elasticbeanstalk_env_url" { value = aws_elastic_beanstalk_environment.env.endpoint_url }
