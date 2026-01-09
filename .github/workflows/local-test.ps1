# CI/CD Pipeline - Local Test Runner (PowerShell)
# This script simulates the GitHub Actions pipeline locally

param(
    [Parameter(Position=0)]
    [ValidateSet('all', 'prereq', 'restore', 'build', 'format', 'unit', 'integration', 'coverage', 'completeness', 'security')]
    [string]$Command = 'all'
)

# Configuration
$DotNetVersion = "8.0"
$SolutionPath = "./CleanApiTemplate.sln"
$TestProject = "./CleanApiTemplate.Test/CleanApiTemplate.Test.csproj"

# Helper functions
function Write-Header {
    param([string]$Message)
    Write-Host ""
    Write-Host "============================================" -ForegroundColor Blue
    Write-Host " $Message" -ForegroundColor Blue
    Write-Host "============================================" -ForegroundColor Blue
    Write-Host ""
}

function Write-Success {
    param([string]$Message)
    Write-Host "[PASS] $Message" -ForegroundColor Green
}

function Write-ErrorMessage {
    param([string]$Message)
    Write-Host "[FAIL] $Message" -ForegroundColor Red
}

function Write-Warning {
    param([string]$Message)
    Write-Host "[WARN] $Message" -ForegroundColor Yellow
}

function Write-Info {
    param([string]$Message)
    Write-Host "[INFO] $Message" -ForegroundColor Cyan
}

# Check prerequisites
function Test-Prerequisites {
    Write-Header "Checking Prerequisites"
    
    # Check .NET SDK
    try {
        $dotnetVersion = dotnet --version
        Write-Success ".NET SDK found: $dotnetVersion"
    }
    catch {
        Write-ErrorMessage ".NET SDK not found. Please install .NET $DotNetVersion SDK"
        return $false
    }
    
    # Check for solution file
    if (-not (Test-Path $SolutionPath)) {
        Write-ErrorMessage "Solution file not found: $SolutionPath"
        return $false
    }
    Write-Success "Solution file found"
    
    # Check for test project
    if (-not (Test-Path $TestProject)) {
        Write-ErrorMessage "Test project not found: $TestProject"
        return $false
    }
    Write-Success "Test project found"
    
    return $true
}

# Restore dependencies
function Invoke-Restore {
    Write-Header "Restoring Dependencies"
    
    dotnet restore $SolutionPath
    
    if ($LASTEXITCODE -eq 0) {
        Write-Success "Dependencies restored successfully"
        return $true
    }
    else {
        Write-ErrorMessage "Failed to restore dependencies"
        return $false
    }
}

# Build solution
function Invoke-Build {
    Write-Header "Building Solution"
    
    dotnet build $SolutionPath --configuration Release --no-restore
    
    if ($LASTEXITCODE -eq 0) {
        Write-Success "Build completed successfully"
        return $true
    }
    else {
        Write-ErrorMessage "Build failed"
        return $false
    }
}

# Check code formatting
function Test-Formatting {
    Write-Header "Checking Code Formatting"
    
    # Check if dotnet format is available
    try {
        $null = dotnet format --version 2>$null
    }
    catch {
        Write-Info "Installing dotnet format..."
        dotnet tool install -g dotnet-format
    }
    
    dotnet format $SolutionPath --verify-no-changes --verbosity diagnostic
    
    if ($LASTEXITCODE -eq 0) {
        Write-Success "Code formatting is correct"
        return $true
    }
    else {
        Write-Warning "Code formatting issues detected. Run 'dotnet format' to fix"
        return $false
    }
}

# Run unit tests
function Invoke-UnitTests {
    Write-Header "Running Unit Tests"
    
    dotnet test $TestProject `
        --configuration Release `
        --no-build `
        --filter "Category!=Integration" `
        --logger "console;verbosity=detailed" `
        --collect:"XPlat Code Coverage" `
        --results-directory ./TestResults/Unit
    
    if ($LASTEXITCODE -eq 0) {
        Write-Success "Unit tests passed"
        return $true
    }
    else {
        Write-ErrorMessage "Unit tests failed"
        return $false
    }
}

# Run integration tests
function Invoke-IntegrationTests {
    Write-Header "Running Integration Tests"
    
    # Check if SQL Server is accessible
    Write-Info "Checking SQL Server connectivity..."
    
    try {
        $sqlCmd = Get-Command sqlcmd -ErrorAction SilentlyContinue
        if ($sqlCmd) {
            $result = sqlcmd -S localhost -U sa -P "P@ssw0rd123!" -Q "SELECT 1" 2>$null
            if ($LASTEXITCODE -eq 0) {
                Write-Success "SQL Server is accessible"
            }
            else {
                Write-Warning "SQL Server not accessible. Skipping integration tests"
                Write-Info "To run integration tests, ensure SQL Server is running"
                return $true
            }
        }
        else {
            Write-Warning "sqlcmd not found. Cannot verify SQL Server. Skipping integration tests"
            return $true
        }
    }
    catch {
        Write-Warning "Could not connect to SQL Server. Skipping integration tests"
        return $true
    }
    
    # Set connection string
    $env:ConnectionStrings__TestConnection = "Server=localhost,1433;Database=CleanApiTemplate_Test;User ID=sa;Password=P@ssw0rd123!;TrustServerCertificate=True;MultipleActiveResultSets=True;Connection Timeout=30;"
    
    dotnet test $TestProject `
        --configuration Release `
        --no-build `
        --filter "Category=Integration" `
        --logger "console;verbosity=detailed" `
        --collect:"XPlat Code Coverage" `
        --results-directory ./TestResults/Integration
    
    if ($LASTEXITCODE -eq 0) {
        Write-Success "Integration tests passed"
        return $true
    }
    else {
        Write-ErrorMessage "Integration tests failed"
        return $false
    }
}

# Generate coverage report
function New-CoverageReport {
    Write-Header "Generating Coverage Report"
    
    # Check if reportgenerator is installed
    try {
        $null = dotnet reportgenerator --version 2>$null
    }
    catch {
        Write-Info "Installing ReportGenerator..."
        dotnet tool install -g dotnet-reportgenerator-globaltool
    }
    
    # Check if coverage files exist
    if (-not (Test-Path "TestResults")) {
        Write-Warning "No test results found. Skipping coverage report"
        return $true
    }
    
    # Find all coverage files
    $coverageFiles = Get-ChildItem -Path TestResults -Filter "coverage.cobertura.xml" -Recurse -ErrorAction SilentlyContinue
    
    if ($coverageFiles.Count -eq 0) {
        Write-Warning "No coverage files found. Skipping coverage report"
        return $true
    }
    
    # Generate report
    dotnet reportgenerator `
        -reports:"TestResults/**/coverage.cobertura.xml" `
        -targetdir:"coveragereport" `
        -reporttypes:"Html;TextSummary" `
        -assemblyfilters:"-xunit*;-*.Tests;-*.Test"
    
    if ($LASTEXITCODE -eq 0) {
        Write-Success "Coverage report generated"
        
        # Display summary
        if (Test-Path "coveragereport/Summary.txt") {
            Write-Host ""
            Get-Content "coveragereport/Summary.txt"
            Write-Host ""
        }
        
        Write-Info "Open coveragereport/index.html to view detailed report"
        
        # Check coverage threshold
        if (Test-Path "coveragereport/Summary.txt") {
            $summaryContent = Get-Content "coveragereport/Summary.txt" -Raw
            if ($summaryContent -match "Line coverage: (\d+\.?\d*)%") {
                $lineCoverage = [double]$matches[1]
                $threshold = 70.0
                
                if ($lineCoverage -lt $threshold) {
                    Write-ErrorMessage "Coverage $lineCoverage% is below threshold $threshold%"
                    return $false
                }
                else {
                    Write-Success "Coverage $lineCoverage% meets threshold $threshold%"
                }
            }
        }
        
        return $true
    }
    else {
        Write-Warning "Failed to generate coverage report"
        return $false
    }
}

# Check test completeness
function Test-TestCompleteness {
    Write-Header "Checking Test Completeness"
    
    $handlerCount = 0
    $testCount = 0
    $missingTests = @()
    
    Write-Info "Analyzing handlers..."
    
    # Find all handler classes
    $handlers = Get-ChildItem -Path "CleanApiTemplate.Application" -Filter "*Handler.cs" -Recurse -ErrorAction SilentlyContinue |
                Where-Object { $_.FullName -notmatch "\\obj\\" -and $_.FullName -notmatch "\\bin\\" }
    
    foreach ($handler in $handlers) {
        $handlerName = $handler.BaseName
        $testFile = "CleanApiTemplate.Test/Application/Handlers/${handlerName}Tests.cs"
        
        $handlerCount++
        
        if (Test-Path $testFile) {
            $testMethods = (Select-String -Path $testFile -Pattern "public.*void.*Test|public.*Task.*Test|\[Fact\]|\[Theory\]" -ErrorAction SilentlyContinue).Count
            Write-Success "$handlerName : $testMethods tests"
            $testCount++
        }
        else {
            Write-ErrorMessage "$handlerName : No test file found!"
            $missingTests += $handlerName
        }
    }
    
    Write-Info "Analyzing validators..."
    
    $validatorCount = 0
    $validatorTestCount = 0
    
    # Find all validator classes
    $validators = Get-ChildItem -Path "CleanApiTemplate.Application" -Filter "*Validator.cs" -Recurse -ErrorAction SilentlyContinue |
                  Where-Object { $_.FullName -notmatch "\\obj\\" -and $_.FullName -notmatch "\\bin\\" }
    
    foreach ($validator in $validators) {
        $validatorName = $validator.BaseName
        $testFile = "CleanApiTemplate.Test/Application/Validators/${validatorName}Tests.cs"
        
        $validatorCount++
        
        if (Test-Path $testFile) {
            $testMethods = (Select-String -Path $testFile -Pattern "public.*void.*Test|public.*Task.*Test|\[Fact\]|\[Theory\]" -ErrorAction SilentlyContinue).Count
            Write-Success "$validatorName : $testMethods tests"
            $validatorTestCount++
        }
        else {
            Write-ErrorMessage "$validatorName : No test file found!"
            $missingTests += $validatorName
        }
    }
    
    Write-Host ""
    Write-Info "Summary:"
    Write-Host "  Handlers: $testCount/$handlerCount"
    Write-Host "  Validators: $validatorTestCount/$validatorCount"
    
    $totalClasses = $handlerCount + $validatorCount
    $totalTested = $testCount + $validatorTestCount
    
    if ($totalClasses -gt 0) {
        $completeness = [math]::Round(($totalTested / $totalClasses) * 100, 2)
        Write-Host "  Overall: $completeness%"
        Write-Host ""
        
        if ($missingTests.Count -gt 0) {
            Write-ErrorMessage "Missing tests for $($missingTests.Count) classes"
            return $false
        }
        else {
            Write-Success "All classes have tests!"
        }
        
        if ($completeness -lt 80) {
            Write-ErrorMessage "Test completeness $completeness% is below 80%"
            return $false
        }
        else {
            Write-Success "Test completeness $completeness% meets threshold"
        }
    }
    
    return $true
}

# Security scan
function Invoke-SecurityScan {
    Write-Header "Running Security Scan"
    
    $output = dotnet list package --vulnerable --include-transitive 2>&1
    $output | Out-File -FilePath "security-scan.txt"
    
    if ($output -match "has the following vulnerable packages") {
        Write-ErrorMessage "Vulnerable packages found!"
        Write-Host $output
        return $false
    }
    else {
        Write-Success "No vulnerable packages found"
        if (Test-Path "security-scan.txt") {
            Remove-Item "security-scan.txt"
        }
        return $true
    }
}

# Main execution
function Invoke-Pipeline {
    Clear-Host
    Write-Host ""
    Write-Host "============================================" -ForegroundColor Blue
    Write-Host "   CI/CD Pipeline - Local Test Runner     " -ForegroundColor Blue
    Write-Host "============================================" -ForegroundColor Blue
    Write-Host ""
    
    $startTime = Get-Date
    $failures = 0
    
    # Run all checks
    if (-not (Test-Prerequisites)) { $failures++ }
    if (-not (Invoke-Restore)) { $failures++ }
    if (-not (Invoke-Build)) { $failures++ }
    if (-not (Test-Formatting)) { $failures++ }
    if (-not (Invoke-UnitTests)) { $failures++ }
    if (-not (Invoke-IntegrationTests)) { $failures++ }
    if (-not (New-CoverageReport)) { $failures++ }
    if (-not (Test-TestCompleteness)) { $failures++ }
    if (-not (Invoke-SecurityScan)) { $failures++ }
    
    # Calculate execution time
    $endTime = Get-Date
    $duration = ($endTime - $startTime).TotalSeconds
    
    # Final summary
    Write-Host ""
    Write-Header "Pipeline Summary"
    
    if ($failures -eq 0) {
        Write-Success "All checks passed!"
        Write-Host ""
        Write-Info "Pipeline completed in $([math]::Round($duration, 2))s"
        exit 0
    }
    else {
        Write-ErrorMessage "$failures check(s) failed"
        Write-Host ""
        Write-Info "Pipeline completed in $([math]::Round($duration, 2))s"
        exit 1
    }
}

# Execute based on command
switch ($Command) {
    'prereq'       { Test-Prerequisites }
    'restore'      { Invoke-Restore }
    'build'        { Invoke-Build }
    'format'       { Test-Formatting }
    'unit'         { Invoke-UnitTests }
    'integration'  { Invoke-IntegrationTests }
    'coverage'     { New-CoverageReport }
    'completeness' { Test-TestCompleteness }
    'security'     { Invoke-SecurityScan }
    'all'          { Invoke-Pipeline }
    default {
        Write-Host "Usage: .\local-test.ps1 [command]"
        Write-Host ""
        Write-Host "Commands:"
        Write-Host "  all          - Run all checks (default)"
        Write-Host "  prereq       - Check prerequisites only"
        Write-Host "  restore      - Restore dependencies only"
        Write-Host "  build        - Build solution only"
        Write-Host "  format       - Check code formatting only"
        Write-Host "  unit         - Run unit tests only"
        Write-Host "  integration  - Run integration tests only"
        Write-Host "  coverage     - Generate coverage report only"
        Write-Host "  completeness - Check test completeness only"
        Write-Host "  security     - Run security scan only"
    }
}
