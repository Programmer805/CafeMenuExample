# CafeMenu Production Environment Deployment Script
# This script deploys the application to the production environment

param(
    [string]$BuildConfiguration = "Release",
    [string]$DeployPath = "C:\inetpub\wwwroot\CafeMenu-Prod"
)

Write-Host "Starting CafeMenu Production Deployment..." -ForegroundColor Green

# Confirmation prompt
$confirmation = Read-Host "Are you sure you want to deploy to PRODUCTION? (yes/no)"
if ($confirmation -ne "yes") {
    Write-Host "Deployment cancelled." -ForegroundColor Red
    exit
}

# Stop IIS if running
Write-Host "Stopping IIS..." -ForegroundColor Yellow
iisreset /stop

# Create deployment directory if it doesn't exist
if (!(Test-Path $DeployPath)) {
    Write-Host "Creating deployment directory..." -ForegroundColor Yellow
    New-Item -ItemType Directory -Path $DeployPath -Force
}

# Build the project
Write-Host "Building project..." -ForegroundColor Yellow
msbuild ..\CafeMenu.sln /p:Configuration=$BuildConfiguration /p:DeployOnBuild=true /p:DeployDefaultTarget=WebPublish /p:WebPublishMethod=FileSystem /p:PublishUrl=$DeployPath

# Copy environment specific config
Write-Host "Copying environment configuration..." -ForegroundColor Yellow
Copy-Item "..\CafeMenu\App.Prod.config" "$DeployPath\web.config" -Force

# Backup current production (optional)
Write-Host "Creating backup..." -ForegroundColor Yellow
$backupPath = "$DeployPath-Backup-$(Get-Date -Format 'yyyyMMdd-HHmmss')"
Copy-Item $DeployPath $backupPath -Recurse -Force

# Start IIS
Write-Host "Starting IIS..." -ForegroundColor Yellow
iisreset /start

Write-Host "Production deployment completed successfully!" -ForegroundColor Green
Write-Host "Backup created at: $backupPath" -ForegroundColor Cyan