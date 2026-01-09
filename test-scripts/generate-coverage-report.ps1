#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Generate HTML coverage report from test results

.DESCRIPTION
    Generates an HTML code coverage report using ReportGenerator.
    Installs ReportGenerator tool if not already installed.

.EXAMPLE
    .\generate-coverage-report.ps1
#>

$ErrorActionPreference = "Stop"

Write-Host "?? Generating Coverage Report..." -ForegroundColor Cyan
Write-Host ""

# Check if ReportGenerator is installed
$reportGeneratorInstalled = dotnet tool list -g | Select-String "reportgenerator"

if (-not $reportGeneratorInstalled) {
    Write-Host "Installing ReportGenerator tool..." -ForegroundColor Yellow
    dotnet tool install -g dotnet-reportgenerator-globaltool
    if ($LASTEXITCODE -ne 0) {
        Write-Host "? Failed to install ReportGenerator" -ForegroundColor Red
        exit 1
    }
}

# Find coverage files
$coverageFiles = Get-ChildItem -Path "." -Filter "coverage.cobertura.xml" -Recurse | Select-Object -ExpandProperty FullName

if ($coverageFiles.Count -eq 0) {
    Write-Host "? No coverage files found!" -ForegroundColor Red
    Write-Host "   Run tests with -Coverage flag first:" -ForegroundColor Yellow
    Write-Host "   .\run-unit-tests.ps1 -Coverage" -ForegroundColor Yellow
    Write-Host "   .\run-all-tests.ps1 -Coverage" -ForegroundColor Yellow
    exit 1
}

Write-Host "Found $($coverageFiles.Count) coverage file(s)" -ForegroundColor Green
$coverageFiles | ForEach-Object { Write-Host "  - $_" -ForegroundColor Gray }
Write-Host ""

# Generate report
$reports = $coverageFiles -join ";"
$targetDir = "coveragereport"

Write-Host "Generating HTML report to: $targetDir" -ForegroundColor Yellow
reportgenerator "-reports:$reports" "-targetdir:$targetDir" "-reporttypes:Html"

if ($LASTEXITCODE -eq 0) {
    Write-Host ""
    Write-Host "? Coverage report generated successfully!" -ForegroundColor Green
    Write-Host ""
    Write-Host "?? Report location: $targetDir\index.html" -ForegroundColor Cyan
    Write-Host ""
    
    $openReport = Read-Host "Open report in browser? (Y/n)"
    if ($openReport -ne 'n' -and $openReport -ne 'N') {
        $indexPath = Join-Path $targetDir "index.html"
        if (Test-Path $indexPath) {
            Start-Process $indexPath
        }
    }
} else {
    Write-Host "? Failed to generate coverage report" -ForegroundColor Red
    exit 1
}

exit 0
