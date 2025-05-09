# Configure the Azure provider
terraform {
  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "~> 4.0"
    }
  }

  backend "azurerm" {
    resource_group_name  = "rg-tfstate"
    storage_account_name = "sttfstateschemata"
    container_name       = "tfstate"
    key                  = "terraform.tfstate"
    use_oidc             = true
  }

  required_version = "1.11.4"
}

provider "azurerm" {
  features {
    resource_group {
      prevent_deletion_if_contains_resources = false
    }
  }

  use_oidc        = true
  subscription_id = var.subscription_id
}

# This ensures we have unique CAF compliant names for our resources.
module "naming" {
  source  = "Azure/naming/azurerm"
  version = "0.4.2"

  suffix = ["appidemo", var.location]
}

# Resource group for the demo
resource "azurerm_resource_group" "this" {
  name     = module.naming.resource_group.name
  location = var.location
}

# Log analytics workspace
module "law" {
  source  = "Azure/avm-res-operationalinsights-workspace/azurerm"
  version = "0.4.2"

  name                = module.naming.log_analytics_workspace.name
  location            = var.location
  resource_group_name = azurerm_resource_group.this.name
  log_analytics_workspace_identity = {
    type = "SystemAssigned"
  }
}

# Application Insights for the Psi API
module "appi_psi" {
  source  = "Azure/avm-res-insights-component/azurerm"
  version = "0.1.5"

  name                = join("-", ["appi", "appidemo", "psi", var.location])
  location            = var.location
  resource_group_name = azurerm_resource_group.this.name
  workspace_id        = module.law.resource_id
}

# Application Insights for the Omega API
module "appi_omega" {
  source  = "Azure/avm-res-insights-component/azurerm"
  version = "0.1.5"

  name                = join("-", ["appi", "appidemo", "omega", var.location])
  location            = var.location
  resource_group_name = azurerm_resource_group.this.name
  workspace_id        = module.law.resource_id
}

# App Service Plan for the APIs
module "asp" {
  source  = "Azure/avm-res-web-serverfarm/azurerm"
  version = "0.5.0"

  name                = module.naming.app_service_plan.name
  location            = var.location
  resource_group_name = azurerm_resource_group.this.name
  os_type             = "Linux"
  sku_name            = "S1"
  worker_count        = 1
}

# App Service for the Psi API
module "app_psi" {
  source  = "Azure/avm-res-web-site/azurerm"
  version = "0.16.0"

  name                = join("-", ["app", "appidemo", "psi", var.location])
  location            = var.location
  resource_group_name = azurerm_resource_group.this.name
  kind                = "webapp"

  os_type                  = module.asp.resource.os_type
  service_plan_resource_id = module.asp.resource_id
  zone_balancing_enabled   = false

  application_insights = {
    name                  = join("-", ["appi", "appidemo", "psi", var.location])
    workspace_resource_id = module.law.resource_id
  }
}

# App Service for the Omega API
module "app_omega" {
  source  = "Azure/avm-res-web-site/azurerm"
  version = "0.16.0"

  name                = join("-", ["app", "appidemo", "omega", var.location])
  location            = var.location
  resource_group_name = azurerm_resource_group.this.name
  kind                = "webapp"

  os_type                  = module.asp.resource.os_type
  service_plan_resource_id = module.asp.resource_id

  application_insights = {
    name                  = join("-", ["appi", "appidemo", "omega", var.location])
    workspace_resource_id = module.law.resource_id
  }
}