# CafeMenu Development Environment Deployment Script
# This script deploys the application to the development environment

param(
    [string]$BuildConfiguration = "Debug",
    [string]$DeployPath = "C:\inetpub\wwwroot\CafeMenu-Dev"
)

Write-Host "Starting CafeMenu Development Deployment..." -ForegroundColor Green

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
Copy-Item "..\CafeMenu\App.Dev.config" "$DeployPath\web.config" -Force

# Start IIS
Write-Host "Starting IIS..." -ForegroundColor Yellow
iisreset /start

Write-Host "Development deployment completed successfully!" -ForegroundColor Green