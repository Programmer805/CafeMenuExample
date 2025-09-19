# CafeMenu CI/CD Pipeline Documentation

## Overview
This document describes the CI/CD pipeline and multi-environment deployment strategy for the CafeMenu application.

## Environments

### 1. Development Environment
- **Purpose**: Local development and testing
- **Database**: LocalDB (CafeMenu_Dev)
- **Features**: 
  - Debug mode enabled
  - Detailed error messages
  - Test data seeding
  - Caching disabled
- **Access**: Developers only
- **URL**: https://localhost:44367/

### 2. Test Environment
- **Purpose**: Integration testing and QA
- **Database**: Azure SQL Database (CafeMenu_Test)
- **Features**:
  - Debug mode disabled
  - Detailed error messages for internal users
  - Test data seeding
  - Caching enabled
- **Access**: QA team and internal stakeholders
- **URL**: https://cafemenu-test.azurewebsites.net/

### 3. Production Environment
- **Purpose**: Live customer-facing application
- **Database**: Azure SQL Database (CafeMenu_Prod)
- **Features**:
  - Debug mode disabled
  - Minimal error information
  - Caching enabled
  - CDN integration
  - Security headers
  - HTTPS enforcement
- **Access**: Public
- **URL**: https://cafemenu.com/

## CI/CD Pipeline

### GitHub Actions Workflow
The pipeline is defined in `.github/workflows/ci-cd.yml` and consists of the following stages:

1. **Build Stage**
   - Checkout code
   - Restore NuGet packages
   - Build solution
   - Run tests
   - Create deployment package

2. **Deploy to Development**
   - Triggered on push to `develop` or `main` branches
   - Deploys to development environment

3. **Deploy to Test**
   - Triggered on push to `main` or `release/*` branches
   - Deploys to test environment

4. **Deploy to Production**
   - Triggered on push to `main` branch only
   - Manual approval required
   - Deploys to production environment

### Branch Strategy
- `develop`: Development branch - auto deploy to dev
- `main`: Main branch - auto deploy to test and prod (with approval)
- `release/*`: Release branches - deploy to test
- `feature/*`: Feature branches - build only

## Configuration Management

### Web.config Transformations
Each environment has its own configuration transformation file:
- `Web.Development.config`
- `Web.Test.config` 
- `Web.Production.config`

These files transform the base `Web.config` during deployment.

### Environment Variables
Sensitive information like connection strings and API keys are stored as:
- GitHub Secrets for CI/CD pipeline
- Azure App Service Configuration for deployed environments

## Deployment Process

### Automated Deployment
1. Code is pushed to GitHub
2. GitHub Actions workflow is triggered
3. Application is built and tested
4. Deployment packages are created
5. Application is deployed to target environment

### Manual Deployment
Use the deployment scripts in the `scripts` folder:
```bash
# Windows
scripts\deploy.bat Development

# Linux/Mac
./scripts/deploy.sh Test
```

## Security Considerations

### Secrets Management
- Never commit secrets to source control
- Use GitHub Secrets for CI/CD
- Use Azure Key Vault for production secrets
- Rotate secrets regularly

### Production Security
- HTTPS enforced
- Security headers enabled
- Detailed errors disabled
- Request validation enabled

## Monitoring and Logging

### Application Insights
Each environment has its own Application Insights resource for monitoring.

### Log Levels
- Development: Debug
- Test: Information
- Production: Warning

## Rollback Strategy

### Automated Rollback
- GitHub Actions can redeploy previous versions
- Azure App Service has deployment slots for easy rollback

### Manual Rollback
- Use Azure Portal to redeploy previous versions
- Keep deployment packages for at least 30 days

## Testing Strategy

### Unit Tests
Run during build process

### Integration Tests
Run in test environment after deployment

### Load Testing
Performed in test environment before production deployment

## Maintenance Windows

### Development
- No scheduled maintenance
- Can be updated at any time

### Test
- Weekly maintenance on Sundays 02:00-04:00 UTC

### Production
- Monthly maintenance on first Sunday 02:00-06:00 UTC
- Emergency maintenance as needed with 4-hour notice

## Contact Information

### CI/CD Pipeline Owner
- Name: [Your Name]
- Email: [your.email@company.com]

### Production Support
- Team: [Support Team Name]
- Email: [support@company.com]
- Phone: [Support Phone Number]