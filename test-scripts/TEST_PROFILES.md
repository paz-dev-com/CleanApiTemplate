# Test Profiles - Quick Reference

## ?? Available Test Execution Options

### ?? Test Categories

| Category | Tests | Duration | Description |
|----------|-------|----------|-------------|
| **Unit Tests** | 90 | ~1s | Fast, isolated tests with mocked dependencies |
| **Integration Tests** | 14 | ~60-70s | Real database tests with SQL Server |
| **All Tests** | 104 | ~70s | Complete test suite |

## ?? Quick Start

### Windows (PowerShell)
```powershell
# Fast unit tests (use during development)
.\run-unit-tests.ps1

# Full validation (use before commit)
.\run-all-tests.ps1

# With coverage (use before PR)
.\run-all-tests.ps1 -Coverage
.\generate-coverage-report.ps1
```

### Linux/Mac (Bash)
```bash
# Fast unit tests (use during development)
./run-unit-tests.sh

# Full validation (use before commit)
./run-all-tests.sh

# With coverage (use before PR)
./run-all-tests.sh --coverage
```

### Command Line (All Platforms)
```bash
# Unit tests only
dotnet test --filter "Category!=Integration"

# Integration tests only
dotnet test --filter "Category=Integration"

# All tests
dotnet test

# With coverage
dotnet test --collect:"XPlat Code Coverage"
```

## ?? Files Overview

```
Project Root/
??? run-unit-tests.ps1           # Run unit tests (Windows)
??? run-integration-tests.ps1    # Run integration tests (Windows)
??? run-all-tests.ps1            # Run all tests (Windows)
??? generate-coverage-report.ps1 # Generate HTML coverage report (Windows)
??? run-unit-tests.sh            # Run unit tests (Linux/Mac)
??? run-integration-tests.sh     # Run integration tests (Linux/Mac)
??? run-all-tests.sh             # Run all tests (Linux/Mac)
??? CleanApiTemplate.Test/
    ??? UnitTests.runsettings         # Visual Studio profile for unit tests
    ??? IntegrationTests.runsettings  # Visual Studio profile for integration tests
    ??? appsettings.Test.json         # Test database configuration
```

## ?? Script Options

### PowerShell Scripts

```powershell
# Run with coverage
.\run-unit-tests.ps1 -Coverage

# Run with verbose output
.\run-unit-tests.ps1 -Verbose

# Both
.\run-unit-tests.ps1 -Coverage -Verbose
```

### Bash Scripts

```bash
# Run with coverage
./run-unit-tests.sh --coverage

# Run with verbose output
./run-unit-tests.sh --verbose

# Both
./run-unit-tests.sh --coverage --verbose
```

## ?? Recommended Workflow

```
???????????????????????????????????????????
?  During Development (Every Few Minutes) ?
?  .\run-unit-tests.ps1                   ?
?  ? ~1 second                            ?
???????????????????????????????????????????
                  ?
???????????????????????????????????????????
?  Before Commit (Every Commit)           ?
?  .\run-all-tests.ps1                    ?
?  ? ~70 seconds                          ?
???????????????????????????????????????????
                  ?
???????????????????????????????????????????
?  Before Push/PR (Major Changes)         ?
?  .\run-all-tests.ps1 -Coverage          ?
?  .\generate-coverage-report.ps1         ?
?  ?? ~70 seconds + report generation     ?
???????????????????????????????????????????
```

## ?? Visual Studio Integration

### Using Test Explorer
1. Open Test Explorer: `Ctrl+E, T`
2. Group by: **Traits**
3. Filter:
   - **Integration** - Shows only integration tests
   - **Not Integration** - Shows only unit tests

### Using .runsettings Files
1. Test ? Configure Run Settings ? Select Solution Wide runsettings File
2. Choose:
   - `UnitTests.runsettings` - For unit tests
   - `IntegrationTests.runsettings` - For integration tests
3. Run: `Ctrl+R, A`

## ?? Code Coverage Reports

### Generate Coverage
```powershell
# Windows
.\run-all-tests.ps1 -Coverage
.\generate-coverage-report.ps1

# Linux/Mac
./run-all-tests.sh --coverage
```

### View Report
- Windows: `coveragereport\index.html`
- Linux/Mac: `coveragereport/index.html`

## ?? Integration Tests Requirements

### Prerequisites
- ? SQL Server running on `localhost:1433`
- ? SA password: `P@ssw0rd` (or custom in `appsettings.Test.json`)

### Connection String
Location: `CleanApiTemplate.Test/appsettings.Test.json`

```json
{
  "ConnectionStrings": {
    "TestConnection": "Server=tcp:localhost,1433;Initial Catalog=CleanApiTemplate_Test;User ID=sa;Password=YourPassword;MultipleActiveResultSets=True;Connection Timeout=30;TrustServerCertificate=True;"
  }
}
```

**Important**: `MultipleActiveResultSets=True` is required!

### Auto-Configuration
Integration tests automatically:
- ? Create test database
- ? Apply migrations
- ? Clean data between tests (Respawn)
- ? Delete database on disposal

## ?? Troubleshooting

### SQL Server Not Running (Integration Tests)
```powershell
# Check status
Get-Service | Where-Object {$_.Name -like "*SQL*"}

# Start SQL Server
Start-Service MSSQLSERVER
```

### Script Execution Policy Error (Windows)
```powershell
# Temporarily bypass
powershell -ExecutionPolicy Bypass -File .\run-unit-tests.ps1

# Or set policy for current user
Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope CurrentUser
```

### Scripts Not Executable (Linux/Mac)
```bash
chmod +x *.sh
```

### Coverage Report Not Generated
```powershell
# Install ReportGenerator
dotnet tool install -g dotnet-reportgenerator-globaltool

# Run tests with coverage first
.\run-all-tests.ps1 -Coverage

# Then generate report
.\generate-coverage-report.ps1
```

## ?? More Information

For comprehensive documentation, see:
- **[CleanApiTemplate.Test/README.md](CleanApiTemplate.Test/README.md)** - Complete test suite documentation

---

**Quick Tip**: Add unit tests to your pre-commit hook for fastest feedback!

```bash
# .git/hooks/pre-commit
#!/bin/sh
dotnet test --filter "Category!=Integration" --verbosity quiet
```
