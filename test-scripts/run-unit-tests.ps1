#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Run unit tests only (excludes integration tests)

.DESCRIPTION
    Executes all unit tests while excluding integration tests.
    Fast execution (~1 second) - perfect for quick feedback during development.

.PARAMETER Coverage
    Generate code coverage report

.PARAMETER Verbose
    Show detailed test output

.EXAMPLE
    .\run-unit-tests.ps1
    
.EXAMPLE
    .\run-unit-tests.ps1 -Coverage
    
.EXAMPLE
    .\run-unit-tests.ps1 -Verbose
#>

param(
    [switch]$Coverage,
    [switch]$Verbose
)

$ErrorActionPreference = "Stop"

Write-Host "?? Running Unit Tests (excluding Integration tests)..." -ForegroundColor Cyan
Write-Host ""

$testProject = "../CleanApiTemplate.Test/CleanApiTemplate.Test.csproj"
$filter = "Category!=Integration"

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
    Write-Host "? Unit tests passed! Duration: $($duration.TotalSeconds.ToString('F2'))s" -ForegroundColor Green
    
    if ($Coverage) {
        Write-Host ""
        Write-Host "?? Coverage reports generated in TestResults folder" -ForegroundColor Yellow
        Write-Host "To view HTML report, run: .\generate-coverage-report.ps1" -ForegroundColor Yellow
    }
} else {
    Write-Host "? Unit tests failed!" -ForegroundColor Red
    exit $exitCode
}

exit 0
