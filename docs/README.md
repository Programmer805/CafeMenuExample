# CafeMenu - Multi-Environment CI/CD Setup

## Overview
This repository contains the CafeMenu application with a complete CI/CD pipeline and multi-environment deployment configuration.

## Environments
- **Development**: Local development environment
- **Test**: Integration testing environment  
- **Production**: Live customer-facing environment

## CI/CD Pipeline
The pipeline is implemented using GitHub Actions and supports automated deployment to all environments.

### Pipeline Flow
```
[Code Push] → [Build & Test] → [Deploy Dev] → [Deploy Test] → [Deploy Prod]
```

## Configuration Files
- `Web.config`: Base configuration
- `Web.Development.config`: Development environment transforms
- `Web.Test.config`: Test environment transforms
- `Web.Production.config`: Production environment transforms

## Deployment
### Automated Deployment
Push to the following branches triggers automated deployments:
- `develop`: Deploys to Development environment
- `main`: Deploys to Test and Production environments
- `release/*`: Deploys to Test environment

### Manual Deployment
Use the deployment scripts:
```bash
# Windows
scripts\deploy.bat Development

# Linux/Mac
./scripts/deploy.sh Test
```

## Environment Variables
Sensitive configuration values are stored as GitHub Secrets:
- `DEV_AZURE_PUBLISH_PROFILE`
- `TEST_AZURE_PUBLISH_PROFILE` 
- `PROD_AZURE_PUBLISH_PROFILE`

## Documentation
See `docs/ci-cd-documentation.md` for complete CI/CD documentation.

## Getting Started
1. Clone the repository
2. Set up the required GitHub Secrets
3. Push to a branch to trigger the pipeline
4. Monitor deployments in GitHub Actions