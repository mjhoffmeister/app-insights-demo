# Infrastructure prerequisites

## Azure Workload Identity Federation (WIF) Setup for GitHub Actions

These instructions will help you configure two Azure AD app registrations for
secure, secretless deployments from GitHub Actions using Workload Identity
Federation (WIF). You will create one identity for infrastructure (Terraform)
and one for application deployments.

### 1. Set Variables

```powershell
$SUBSCRIPTION_ID = "your-subscription-id"
$GH_ORG = "your-github-org-or-user"
$GH_REPO = "your-github-repo-name"
$ENVIRONMENT = "demo" # or your environment name

# Names for app registrations
$INFRA_APP_NAME = "iam-appidemo-infra-gh"
$APPS_APP_NAME = "iam-appidemo-apps-gh"
```

### 2. Create App Registrations

```powershell
az ad app create --display-name $INFRA_APP_NAME
az ad app create --display-name $APPS_APP_NAME
```

Get their client IDs:
```powershell
$INFRA_CLIENT_ID = az ad app list --display-name $INFRA_APP_NAME --query "[0].appId" -o tsv
$APPS_CLIENT_ID = az ad app list --display-name $APPS_APP_NAME --query "[0].appId" -o tsv
```

### 3. Create Service Principals

```powershell
az ad sp create --id $INFRA_CLIENT_ID
az ad sp create --id $APPS_CLIENT_ID
```

### 4. Assign Roles

**Infra (Contributor on subscription):**
```powershell
az role assignment create --assignee $INFRA_CLIENT_ID --role "Contributor" --scope /subscriptions/$SUBSCRIPTION_ID
```

**Apps (Website Contributor on subscription):**
```powershell
az role assignment create --assignee $APPS_CLIENT_ID --role "Website Contributor" --scope /subscriptions/$SUBSCRIPTION_ID
```

### 5. Add Federated Credentials

**For each app registration:**

The following would need to be adjusted to fill in the placeholders.

```cmd
az ad app federated-credential create --id $INFRA_CLIENT_ID --parameters "{\"name\":\"github-wif-infra\",\"issuer\":\"https://token.actions.githubusercontent.com\",\"subject\":\"repo:${GH_ORG}/${GH_REPO}:environment:${ENVIRONMENT}\",\"description\":\"GitHub Actions WIF for infra\",\"audiences\":[\"api://AzureADTokenExchange\"]}"

az ad app federated-credential create --id $APPS_CLIENT_ID --parameters "{\"name\":\"github-wif-apps\",\"issuer\":\"https://token.actions.githubusercontent.com\",\"subject\":\"repo:${GH_ORG}/${GH_REPO}:environment:${ENVIRONMENT}\",\"description\":\"GitHub Actions WIF for apps\",\"audiences\":[\"api://AzureADTokenExchange\"]}"
```

The following won't work because of PowerShell's issue with escaping quotes.

```powershell
az ad app federated-credential create --id $INFRA_CLIENT_ID `
  --parameters "{\"name\":\"github-wif-infra\",\"issuer\":\"https://token.actions.githubusercontent.com\",\"subject\":\"repo:${GH_ORG}/${GH_REPO}:environment:${ENVIRONMENT}\",\"description\":\"GitHub Actions WIF for infra\",\"audiences\":[\"api://AzureADTokenExchange\"]}"

az ad app federated-credential create --id $APPS_CLIENT_ID `
  --parameters "{\"name\":\"github-wif-apps\",\"issuer\":\"https://token.actions.githubusercontent.com\",\"subject\":\"repo:${GH_ORG}/${GH_REPO}:environment:${ENVIRONMENT}\",\"description\":\"GitHub Actions WIF for apps\",\"audiences\":[\"api://AzureADTokenExchange\"]}"
```

### 6. Gather Values for GitHub Secrets

```powershell
$TENANT_ID = az account show --query tenantId -o tsv
```

Add these as GitHub Actions secrets:
- `AZURE_TENANT_ID` = $TENANT_ID
- `AZURE_SUBSCRIPTION_ID` = $SUBSCRIPTION_ID
- `AZURE_INFRA_CLIENT_ID` = $INFRA_CLIENT_ID
- `AZURE_APPS_CLIENT_ID` = $APPS_CLIENT_ID

### 7. Scope down permissions

After the first deployment, the WIF app registration permissions can be scoped
down to the resource group.

az role assignment delete --assignee $INFRA_CLIENT_ID --role "Contributor" --subscription $SUBSCRIPTION_ID
az role assignment create --assignee $INFRA_CLIENT_ID --role "Contributor" --resource-group $RESOURCE_GROUP

az role assignment delete --assignee $APPS_CLIENT_ID --role "Website Contributor" --subscription $SUBSCRIPTION_ID
az role assignment create --assignee $APPS_CLIENT_ID --role "Website Contributor" --resource-group $RESOURCE_GROUP

---

You are now ready to use Workload Identity Federation with your GitHub Actions
workflows for secure, secretless Azure deployments!
