output "app_service_omega_name" {
  value = module.app_psi.name
}

output "app_service_psi_name" {
  value = module.app_omega.name
}

output "resource_group_name" {
  value = azurerm_resource_group.this.name
}