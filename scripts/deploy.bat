@echo off
REM CafeMenu Deployment Script
REM This script deploys the application to different environments

set ENVIRONMENT=%1
set CONFIGURATION=Release

if "%ENVIRONMENT%"=="" (
    echo Usage: deploy.bat [Development^|Test^|Production]
    exit /b 1
)

echo Deploying CafeMenu to %ENVIRONMENT% environment...

REM Restore NuGet packages
echo Restoring NuGet packages...
nuget restore CafeMenu.sln

REM Build with environment-specific configuration
echo Building solution for %ENVIRONMENT%...
if "%ENVIRONMENT%"=="Development" (
    msbuild CafeMenu.sln /p:Configuration=%CONFIGURATION% /p:DeployOnBuild=true /p:WebPublishMethod=Package /p:PackageAsSingleFile=true /p:SkipInvalidConfigurations=true /p:PackageLocation=".\published\dev" /p:TransformWebConfigEnabled=True /p:WebConfigTransformFileName=Web.Development.config
) else if "%ENVIRONMENT%"=="Test" (
    msbuild CafeMenu.sln /p:Configuration=%CONFIGURATION% /p:DeployOnBuild=true /p:WebPublishMethod=Package /p:PackageAsSingleFile=true /p:SkipInvalidConfigurations=true /p:PackageLocation=".\published\test" /p:TransformWebConfigEnabled=True /p:WebConfigTransformFileName=Web.Test.config
) else if "%ENVIRONMENT%"=="Production" (
    msbuild CafeMenu.sln /p:Configuration=%CONFIGURATION% /p:DeployOnBuild=true /p:WebPublishMethod=Package /p:PackageAsSingleFile=true /p:SkipInvalidConfigurations=true /p:PackageLocation=".\published\prod" /p:TransformWebConfigEnabled=True /p:WebConfigTransformFileName=Web.Production.config
) else (
    echo Invalid environment. Use Development, Test, or Production.
    exit /b 1
)

echo Deployment package created successfully in published\%ENVIRONMENT%\

REM Deployment instructions
echo.
echo To deploy:
echo 1. For Development: Use IIS Express or local IIS
echo 2. For Test: Deploy to test server using Web Deploy
echo 3. For Production: Deploy to production server using Web Deploy
echo.
echo Example Web Deploy command:
echo msdeploy -verb:sync -source:package='%cd%\published\%ENVIRONMENT%\CafeMenu.zip' -dest:auto,ComputerName='https://yourserver.com:8172/msdeploy.axd?site=CafeMenu',UserName='username',Password='password',AuthType='Basic' -allowUntrusted

pause