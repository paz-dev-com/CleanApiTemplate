# Test Scripts

This folder contains all test execution scripts and documentation for the Clean Architecture Template project.

## ?? Folder Contents

### PowerShell Scripts (Windows)
- **`run-unit-tests.ps1`** - Run unit tests only (~1 second)
- **`run-integration-tests.ps1`** - Run integration tests only (~60-70 seconds)
- **`run-all-tests.ps1`** - Run all tests (~70 seconds)
- **`generate-coverage-report.ps1`** - Generate HTML coverage report

### Bash Scripts (Linux/Mac)
- **`run-unit-tests.sh`** - Run unit tests only
- **`run-integration-tests.sh`** - Run integration tests only
- **`run-all-tests.sh`** - Run all tests

### Documentation
- **`TEST_PROFILES.md`** - Quick reference guide for test execution
- **`TEST_PROFILES_COMPLETE.md`** - Complete implementation summary

## ?? Quick Start

### Windows (PowerShell)
```powershell
# From project root
cd test-scripts

# Run unit tests (fast feedback)
.\run-unit-tests.ps1

# Run all tests
.\run-all-tests.ps1

# Run with coverage
.\run-all-tests.ps1 -Coverage
.\generate-coverage-report.ps1
```

### Linux/Mac (Bash)
```bash
# From project root
cd test-scripts

# Make scripts executable (first time only)
chmod +x *.sh

# Run unit tests (fast feedback)
./run-unit-tests.sh

# Run all tests
./run-all-tests.sh

# Run with coverage
./run-all-tests.sh --coverage
```

## ?? Test Categories

| Category | Count | Duration | Command |
|----------|-------|----------|---------|
| **Unit Tests** | 90 | ~1s | `.\run-unit-tests.ps1` |
| **Integration Tests** | 14 | ~60-70s | `.\run-integration-tests.ps1` |
| **All Tests** | 104 | ~70s | `.\run-all-tests.ps1` |

## ?? Documentation

- **Main Test Documentation**: See [CleanApiTemplate.Test/README.md](../CleanApiTemplate.Test/README.md)
- **Quick Reference**: See [TEST_PROFILES.md](TEST_PROFILES.md)
- **Implementation Details**: See [TEST_PROFILES_COMPLETE.md](TEST_PROFILES_COMPLETE.md)

## ?? Recommended Workflow

```
During Development (frequent)
  ?
  .\run-unit-tests.ps1
  ? Fast feedback (~1 second)

Before Commit (occasional)
  ?
  .\run-all-tests.ps1
  ? Full validation (~70 seconds)

Before Push/PR (always)
  ?
  .\run-all-tests.ps1 -Coverage
  .\generate-coverage-report.ps1
  ?? Complete validation + metrics
```

## ?? Script Features

### PowerShell Scripts
- ? SQL Server connectivity check (integration tests)
- ? Color-coded output
- ? Execution time tracking
- ? Coverage support (`-Coverage` flag)
- ? Verbose output (`-Verbose` flag)

### Bash Scripts
- ? Execution time tracking
- ? Coverage support (`--coverage` flag)
- ? Verbose output (`--verbose` flag)

## ?? Alternative Execution Methods

### From Project Root
```powershell
# Direct dotnet commands
dotnet test --filter "Category!=Integration"  # Unit tests only
dotnet test --filter "Category=Integration"    # Integration tests only
dotnet test                                    # All tests
```

### Visual Studio
1. Test ? Configure Run Settings ? Select Solution Wide runsettings File
2. Choose: `CleanApiTemplate.Test/UnitTests.runsettings` or `IntegrationTests.runsettings`
3. Test ? Run All Tests (Ctrl+R, A)

## ?? Requirements

### Unit Tests
- ? .NET 8 SDK
- ? No additional requirements

### Integration Tests
- ? .NET 8 SDK
- ? SQL Server running on `localhost:1433`
- ? SA password: `P@ssw0rd` (configurable in `CleanApiTemplate.Test/appsettings.Test.json`)

## ?? Troubleshooting

### Scripts Won't Execute (Windows)
```powershell
# Set execution policy
Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope CurrentUser

# Or bypass for single execution
powershell -ExecutionPolicy Bypass -File .\run-unit-tests.ps1
```

### Scripts Won't Execute (Linux/Mac)
```bash
# Make executable
chmod +x *.sh
```

### SQL Server Not Running
```powershell
# Check status
Get-Service | Where-Object {$_.Name -like "*SQL*"}

# Start SQL Server
Start-Service MSSQLSERVER
```

---

**For complete test documentation, see [CleanApiTemplate.Test/README.md](../CleanApiTemplate.Test/README.md)**
