#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Run all tests (unit + integration)

.DESCRIPTION
    Executes all tests including unit tests and integration tests.
    Use this before creating pull requests or major commits.

.PARAMETER Coverage
    Generate code coverage report

.PARAMETER Verbose
    Show detailed test output

.EXAMPLE
    .\run-all-tests.ps1
    
.EXAMPLE
    .\run-all-tests.ps1 -Coverage
    
.EXAMPLE
    .\run-all-tests.ps1 -Verbose
#>

param(
    [switch]$Coverage,
    [switch]$Verbose
)

$ErrorActionPreference = "Stop"

Write-Host "?? Running All Tests (Unit + Integration)..." -ForegroundColor Cyan
Write-Host ""

# Check if SQL Server is running for integration tests
Write-Host "Checking SQL Server connectivity for integration tests..." -ForegroundColor Yellow
$sqlCheck = Test-NetConnection -ComputerName localhost -Port 1433 -InformationLevel Quiet -WarningAction SilentlyContinue

if (-not $sqlCheck) {
    Write-Host "??  Warning: Cannot connect to SQL Server on localhost:1433" -ForegroundColor Yellow
    Write-Host "   Integration tests will fail without SQL Server." -ForegroundColor Yellow
    Write-Host ""
    
    $response = Read-Host "Continue anyway? (y/N)"
    if ($response -ne 'y' -and $response -ne 'Y') {
        Write-Host "? Cancelled" -ForegroundColor Red
        exit 1
    }
}

$testProject = "../CleanApiTemplate.Test/CleanApiTemplate.Test.csproj"

# Build command
$command = "dotnet test `"$testProject`""

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
    Write-Host "? All tests passed! Duration: $($duration.TotalSeconds.ToString('F2'))s" -ForegroundColor Green
    
    if ($Coverage) {
        Write-Host ""
        Write-Host "?? Coverage reports generated in TestResults folder" -ForegroundColor Yellow
        Write-Host "To view HTML report, run: .\generate-coverage-report.ps1" -ForegroundColor Yellow
    }
} else {
    Write-Host "? Tests failed!" -ForegroundColor Red
    exit $exitCode
}

exit 0
