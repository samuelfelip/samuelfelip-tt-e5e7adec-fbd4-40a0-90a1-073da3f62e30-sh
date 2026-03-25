# Minimal Terraform scaffold — adapt provider/region to your cloud.
# Stack: .NET 10 container + managed PostgreSQL (Npgsql) + secrets.
#
# This file documents inputs you would wire to Azure Container Apps, AWS ECS,
# Google Cloud Run, etc. It does not create resources until you add a provider block
# and real resources (keeps the repo free of cloud lock-in).

terraform {
  required_version = ">= 1.6"
  # Add required_providers when you create real resources (azurerm, aws, google, etc.).
}

variable "environment_name" {
  type        = string
  description = "Deployment environment label (e.g. prod, staging)"
  default     = "prod"
}

variable "postgres_connection_string" {
  type        = string
  sensitive   = true
  description = "Npgsql connection string (matches appsettings ConnectionStrings:PostgreSql)"
}

variable "leaderboard_window_days" {
  type        = number
  description = "Leaderboard aggregation window (LeaderboardSettings:WindowDays)"
  default     = 7
}

variable "leaderboard_max_score_batch_size" {
  type        = number
  description = "Max rows per POST /api/scores/bulk (LeaderboardSettings:MaxScoreBatchSize)"
  default     = 10000
}

# Wire these as container env vars in your platform:
#   ASPNETCORE_ENVIRONMENT     = var.environment_name == "prod" ? "Production" : "Staging"
#   ASPNETCORE_URLS            = "http://+:8080"
#   ConnectionStrings__PostgreSql = var.postgres_connection_string
#   LeaderboardSettings__WindowDays = tostring(var.leaderboard_window_days)
#   LeaderboardSettings__MaxScoreBatchSize = tostring(var.leaderboard_max_score_batch_size)

output "container_env_snippet" {
  description = "Environment variables to set on the API container"
  value = {
    ASPNETCORE_ENVIRONMENT              = "Production"
    ASPNETCORE_URLS                     = "http://+:8080"
    ConnectionStrings__PostgreSql       = "(set from var.postgres_connection_string — sensitive)"
    LeaderboardSettings__WindowDays     = tostring(var.leaderboard_window_days)
    LeaderboardSettings__MaxScoreBatchSize = tostring(var.leaderboard_max_score_batch_size)
  }
}
