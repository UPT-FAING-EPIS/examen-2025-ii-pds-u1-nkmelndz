variable "project_name" { type = string default = "online-courses" }
variable "aws_region" { type = string default = "us-east-1" }
variable "environment" { type = string default = "dev" }
variable "rds_username" { type = string }
variable "rds_password" { type = string sensitive = true }
variable "instance_class" { type = string default = "db.t3.micro" }
variable "allocated_storage" { type = number default = 20 }
