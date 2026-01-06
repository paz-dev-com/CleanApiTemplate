#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Run integration tests only

.DESCRIPTION
    Executes only integration tests that require database connectivity.
    Slower execution (~60-70 seconds) - run before commits/PRs.

.PARAMETER Coverage
    Generate code coverage report

.PARAMETER Verbose
    Show detailed test output

.EXAMPLE
    .\run-integration-tests.ps1
    
.EXAMPLE
    .\run-integration-tests.ps1 -Coverage
    
.EXAMPLE
    .\run-integration-tests.ps1 -Verbose
#>

param(
    [switch]$Coverage,
    [switch]$Verbose
)

$ErrorActionPreference = "Stop"

Write-Host "?? Running Integration Tests (requires SQL Server)..." -ForegroundColor Cyan
Write-Host ""

# Check if SQL Server is running
Write-Host "Checking SQL Server connectivity..." -ForegroundColor Yellow
$sqlCheck = Test-NetConnection -ComputerName localhost -Port 1433 -InformationLevel Quiet -WarningAction SilentlyContinue

if (-not $sqlCheck) {
    Write-Host "??  Warning: Cannot connect to SQL Server on localhost:1433" -ForegroundColor Yellow
    Write-Host "   Make sure SQL Server is running before executing integration tests." -ForegroundColor Yellow
    Write-Host ""
    
    $response = Read-Host "Continue anyway? (y/N)"
    if ($response -ne 'y' -and $response -ne 'Y') {
        Write-Host "? Cancelled" -ForegroundColor Red
        exit 1
    }
}

$testProject = "../CleanApiTemplate.Test/CleanApiTemplate.Test.csproj"
$filter = "Category=Integration"

# Build command
$command = "dotnet test `"$testProject`" --filter `"$filter`""

# Add verbosity
if ($Verbose) {
    $command += " --verbosity detailed"
} else {
    $command += " --verbosity minimal"
}

# Add code coverage
if ($Coverage) {
    Write-Host "?? Code coverage enabled" -ForegroundColor Yellow
    $command += " --collect:`"XPlat Code Coverage`""
}

Write-Host "Command: $command" -ForegroundColor Gray
Write-Host ""

# Execute tests
$startTime = Get-Date
Invoke-Expression $command
$exitCode = $LASTEXITCODE
$duration = (Get-Date) - $startTime

Write-Host ""
if ($exitCode -eq 0) {
    Write-Host "? Integration tests passed! Duration: $($duration.TotalSeconds.ToString('F2'))s" -ForegroundColor Green
    
    if ($Coverage) {
        Write-Host ""
        Write-Host "?? Coverage reports generated in TestResults folder" -ForegroundColor Yellow
        Write-Host "To view HTML report, run: .\generate-coverage-report.ps1" -ForegroundColor Yellow
    }
} else {
    Write-Host "? Integration tests failed!" -ForegroundColor Red
    exit $exitCode
}

exit 0
