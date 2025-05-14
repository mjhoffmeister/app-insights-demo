# App Insights Demo

This repository demonstrates how to enable and use logging and telemetry with **Azure Application Insights** in a modern .NET application, deployed to Azure App Service using infrastructure-as-code and secure DevOps practices.

## Purpose

The goal of this project is to provide a reference implementation for:
- Instrumenting .NET APIs with Application Insights for distributed tracing, logging, and metrics.
- Deploying both application code and infrastructure to Azure using GitHub Actions and Terraform.
- Using Azure Workload Identity Federation (WIF) for secure, secretless CI/CD pipelines.

## Structure

- **src/**  
  Contains the .NET API projects (`OmegaApi` and `PsiApi`) instrumented with Application Insights via the Azure Monitor OpenTelemetry SDK.

- **infrastructure/**  
  Contains Terraform code to provision Azure resources, including:
  - Resource group
  - App Service Plan
  - App Services for each API
  - Application Insights (workspace-based)
  - Log Analytics workspace

- **.github/workflows/**  
  Contains GitHub Actions workflows for:
  - Deploying infrastructure (`deploy-infra.yml`)
  - Building and deploying application code (`deploy-apps.yml`)

## Features

- **Application Insights Integration:**  
  Both APIs are configured to send telemetry (requests, traces, exceptions, dependencies) to Azure Application Insights.

- **Secure CI/CD:**  
  Uses Azure Workload Identity Federation for GitHub Actions, eliminating the need for secrets in your pipeline.

- **Infrastructure as Code:**  
  All Azure resources are provisioned and managed via Terraform.

- **Best Practices:**  
  - Uses `WEBSITE_RUN_FROM_PACKAGE=1` for efficient and reliable App Service deployments.
  - Scopes down permissions after initial deployment for least-privilege access.

## Getting Started

1. **Provision Azure resources** using the Terraform code in `infrastructure/`.
2. **Configure Azure AD and GitHub** for Workload Identity Federation as described in `infrastructure/README.md`.
3. **Deploy the APIs** using the provided GitHub Actions workflows.
4. **View telemetry** in the Azure Portal under Application Insights.

## What You'll See

- End-to-end request traces between APIs.
- Automatic and custom logs, exceptions, and metrics in Application Insights.
- Secure, automated deployments with no secrets in your pipeline.

### Sample KQL queries

**Custom metrics**
```k
customMetrics
| where name == "fib.life_universe_everything.counter"
```

**Dependency tracking**
```k
// Single request
(requests | union dependencies)
| where operation_Id == "{operation_id}"
```

```k
// All requests
(requests | union dependencies)
| where success == false
  and url !contains "robot"
| project timestamp, operation_Id, url, resultCode, target
| order by operation_Id
```

**Visualization**
```k
traces
| where message contains "Omega API call succeeded"
  and message !contains "n=1"
| extend fibSeqNum = split(message, "n=")[1]
| summarize requestCount = count() by toint(fibSeqNum)
| order by fibSeqNum asc
| render barchart
    with(
        title = "Requests by Fibonnaci sequence number",
        xtitle = "Fibonacci sequence number",
        ytitle = "Request count")
```
---

**This repo is intended as a learning and reference resource for teams adopting Application Insights and secure Azure DevOps