# ? Test Organization - Complete!

All test-related files have been organized into the `test-scripts/` folder for better project structure and readability.

## ?? Project Structure

```
CleanApiTemplate/
??? test-scripts/                    # ?? All test execution scripts and docs
?   ??? README.md                    # Quick start guide
?   ??? TEST_PROFILES.md             # Quick reference
?   ??? TEST_PROFILES_COMPLETE.md    # Implementation details
?   ??? run-unit-tests.ps1           # PowerShell: Unit tests
?   ??? run-integration-tests.ps1    # PowerShell: Integration tests
?   ??? run-all-tests.ps1            # PowerShell: All tests
?   ??? generate-coverage-report.ps1 # PowerShell: Coverage report
?   ??? run-unit-tests.sh            # Bash: Unit tests
?   ??? run-integration-tests.sh     # Bash: Integration tests
?   ??? run-all-tests.sh             # Bash: All tests
??? CleanApiTemplate.Test/           # Test project
?   ??? README.md                    # Complete test documentation
?   ??? UnitTests.runsettings        # VS profile for unit tests
?   ??? IntegrationTests.runsettings # VS profile for integration tests
?   ??? appsettings.Test.json        # Test database config
?   ??? [Test files...]
??? [Other projects...]
```

## ?? Quick Start

### Windows (PowerShell)
```powershell
# Navigate to test scripts folder
cd test-scripts

# Run unit tests (fast ~1s)
.\run-unit-tests.ps1

# Run all tests
.\run-all-tests.ps1

# Run with coverage and generate report
.\run-all-tests.ps1 -Coverage
.\generate-coverage-report.ps1
```

### Linux/Mac (Bash)
```bash
# Navigate to test scripts folder
cd test-scripts

# Make scripts executable (first time only)
chmod +x *.sh

# Run unit tests (fast ~1s)
./run-unit-tests.sh

# Run all tests
./run-all-tests.sh
```

## ?? Documentation

| Document | Location | Purpose |
|----------|----------|---------|
| **Quick Start** | `test-scripts/README.md` | How to run test scripts |
| **Quick Reference** | `test-scripts/TEST_PROFILES.md` | Command cheat sheet |
| **Implementation** | `test-scripts/TEST_PROFILES_COMPLETE.md` | Complete implementation details |
| **Full Test Docs** | `CleanApiTemplate.Test/README.md` | Comprehensive test documentation |

## ?? Benefits of This Organization

### ? Clean Project Root
- All test execution scripts in one place
- Easy to find and use
- Doesn't clutter the main project directory

### ? Logical Separation
- **`test-scripts/`** - How to run tests
- **`CleanApiTemplate.Test/`** - Test implementation and details

### ? Better Developer Experience
- Clear entry point (`test-scripts/README.md`)
- Scripts are self-contained
- Easy to add to `.gitignore` or exclude from deployments

### ? Platform Support
- PowerShell scripts for Windows
- Bash scripts for Linux/Mac
- Visual Studio integration via `.runsettings`
- Direct `dotnet test` commands

## ?? Recommended Workflow

```
Development:
  cd test-scripts
  .\run-unit-tests.ps1          # Fast feedback (~1s)

Before Commit:
  cd test-scripts
  .\run-all-tests.ps1            # Full validation (~70s)

Before PR:
  cd test-scripts
  .\run-all-tests.ps1 -Coverage
  .\generate-coverage-report.ps1 # Metrics + report
```

## ?? Test Statistics

```
Total Tests: 104 ?
??? Unit Tests: 90 (~1 second)
??? Integration Tests: 14 (~60-70 seconds)
```

## ?? Related Files

### At Project Root
- **`.gitignore`** - Excludes test artifacts (`coveragereport/`, `TestResults/`)
- **`CleanApiTemplate.sln`** - Solution file

### Test Configuration
- **`CleanApiTemplate.Test/appsettings.Test.json`** - Test database connection
- **`CleanApiTemplate.Test/UnitTests.runsettings`** - VS unit test profile
- **`CleanApiTemplate.Test/IntegrationTests.runsettings`** - VS integration test profile

## ? Alternative Execution

### From Project Root (Without Scripts)
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

### From Visual Studio
1. Test ? Configure Run Settings ? Select Solution Wide runsettings File
2. Choose `CleanApiTemplate.Test/UnitTests.runsettings` or `IntegrationTests.runsettings`
3. Test ? Run All Tests (Ctrl+R, A)

---

**For complete documentation, see:**
- Quick Start: [test-scripts/README.md](test-scripts/README.md)
- Full Documentation: [CleanApiTemplate.Test/README.md](CleanApiTemplate.Test/README.md)
