variable "location" {
  description = "The location for the resources."
  type        = string
  validation {
    condition     = contains(["eastus", "westus", "centralus", "eastus2", "westus2", "northcentralus", "southcentralus", "northeurope", "westeurope", "southeastasia", "eastasia", "japaneast", "japanwest", "australiaeast", "australiasoutheast", "brazilsouth", "southindia", "centralindia", "canadacentral", "canadaeast", "uksouth", "ukwest", "koreacentral", "koreasouth"], var.location)
    error_message = "The location must be one of the following: eastus, westus, centralus, eastus2, westus2, northcentralus, southcentralus, northeurope, westeurope, southeastasia, eastasia, japaneast, japanwest, australiaeast, australiasoutheast, brazilsouth, southindia, centralindia, canadacentral, canadaeast, uksouth, ukwest, koreacentral, koreasouth."
  }
}

variable "subscription_id" {
  description = "The Azure subscription id."
  type        = string
  validation {
    condition     = can(regex("^([0-9a-fA-F]{8})-([0-9a-fA-F]{4})-([0-9a-fA-F]{4})-([0-9a-fA-F]{4})-([0-9a-fA-F]{12})$", var.subscription_id))
    error_message = "The subscription id must be a valid GUID."
  }
}