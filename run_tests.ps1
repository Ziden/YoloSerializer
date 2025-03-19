# This script will run the tests directly
Write-Host "Running tests for YoloSerializer using dotnet test"

# Clean the solution
dotnet clean

# Restore packages
dotnet restore

# Build the solution
dotnet build --no-incremental

# Run the tests with detailed output
dotnet test --no-build --verbosity detailed --logger:"console;verbosity=detailed" 