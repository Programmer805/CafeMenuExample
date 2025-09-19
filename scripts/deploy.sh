#!/bin/bash
# CafeMenu Deployment Script for Linux/Mac
# This script deploys the application to different environments

ENVIRONMENT=$1
CONFIGURATION="Release"

if [ -z "$ENVIRONMENT" ]; then
    echo "Usage: ./deploy.sh [Development|Test|Production]"
    exit 1
fi

echo "Deploying CafeMenu to $ENVIRONMENT environment..."

# Restore NuGet packages
echo "Restoring NuGet packages..."
nuget restore CafeMenu.sln

# Build with environment-specific configuration
echo "Building solution for $ENVIRONMENT..."
case $ENVIRONMENT in
    "Development")
        msbuild CafeMenu.sln /p:Configuration=$CONFIGURATION /p:DeployOnBuild=true /p:WebPublishMethod=Package /p:PackageAsSingleFile=true /p:SkipInvalidConfigurations=true /p:PackageLocation="./published/dev" /p:TransformWebConfigEnabled=True /p:WebConfigTransformFileName=Web.Development.config
        ;;
    "Test")
        msbuild CafeMenu.sln /p:Configuration=$CONFIGURATION /p:DeployOnBuild=true /p:WebPublishMethod=Package /p:PackageAsSingleFile=true /p:SkipInvalidConfigurations=true /p:PackageLocation="./published/test" /p:TransformWebConfigEnabled=True /p:WebConfigTransformFileName=Web.Test.config
        ;;
    "Production")
        msbuild CafeMenu.sln /p:Configuration=$CONFIGURATION /p:DeployOnBuild=true /p:WebPublishMethod=Package /p:PackageAsSingleFile=true /p:SkipInvalidConfigurations=true /p:PackageLocation="./published/prod" /p:TransformWebConfigEnabled=True /p:WebConfigTransformFileName=Web.Production.config
        ;;
    *)
        echo "Invalid environment. Use Development, Test, or Production."
        exit 1
        ;;
esac

echo "Deployment package created successfully in published/$ENVIRONMENT/"

# Deployment instructions
echo ""
echo "To deploy:"
echo "1. For Development: Use IIS Express or local IIS"
echo "2. For Test: Deploy to test server using Web Deploy"
echo "3. For Production: Deploy to production server using Web Deploy"
echo ""
echo "Example Web Deploy command:"
echo "msdeploy -verb:sync -source:package='\$(pwd)/published/$ENVIRONMENT/CafeMenu.zip' -dest:auto,ComputerName='https://yourserver.com:8172/msdeploy.axd?site=CafeMenu',UserName='username',Password='password',AuthType='Basic' -allowUntrusted"