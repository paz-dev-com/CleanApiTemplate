# CI/CD Pipeline Documentation

## Overview

This repository includes a comprehensive CI/CD pipeline using GitHub Actions that ensures code quality, test coverage, and security compliance.

## Workflows

### 1. CI/CD Pipeline (`ci-cd.yml`)

**Triggers:**
- Push to `main` or `develop` branches
- Pull requests to `main` or `develop` branches
- Manual workflow dispatch

**Jobs:**

#### ?? Code Quality & Build
- Checks out code
- Restores dependencies
- Builds solution in Release mode
- Validates code formatting with `dotnet format`
- Uploads build artifacts

#### ?? Unit Tests
- Runs all unit tests (excluding integration tests)
- Uses filter: `Category!=Integration`
- Collects code coverage
- Publishes test results with detailed reporting
- **Duration**: ~1-2 seconds

#### ?? Integration Tests
- Spins up SQL Server container
- Runs integration tests with real database
- Uses filter: `Category=Integration`
- Collects code coverage
- **Duration**: ~60-70 seconds

#### ?? Code Coverage Analysis
- Combines coverage from unit and integration tests
- Generates HTML reports with ReportGenerator
- Creates badges for coverage metrics
- **Threshold**: 70% minimum coverage (configurable)
- Adds coverage summary to PR comments
- Fails if coverage is below threshold

#### ?? Test Completeness Check
- **Scans for missing test files**
- Validates that all handlers have corresponding tests
- Validates that all validators have corresponding tests
- Checks for untested public methods
- Calculates overall test completeness percentage
- **Threshold**: 80% minimum completeness
- Fails if classes without tests are detected

**Example Output:**
```
## ?? Test Completeness Analysis

### Command Handlers
- ? CreateProductCommandHandler: 6 tests
- ? UpdateProductCommandHandler: 13 tests
- ? DeleteProductCommandHandler: 11 tests
- ? GetProductByIdQueryHandler: 8 tests
- ? SomeNewHandler: No test file found!

Summary: 4/5 handlers have tests

### Validators
- ? CreateProductCommandValidator: 12 tests
- ? UpdateProductCommandValidator: 27 tests
- ? DeleteProductCommandValidator: 5 tests

Summary: 3/3 validators have tests

### ?? Overall Test Completeness: 87.5%
```

#### ?? Security Scan
- Scans for vulnerable NuGet packages
- Checks both direct and transitive dependencies
- Fails pipeline if vulnerabilities are found
- Uploads security report as artifact

#### ?? Pipeline Status
- Aggregates results from all jobs
- Provides final summary in job summary
- Fails if any critical job fails

### 2. PR Validation (`pr-validation.yml`)

**Triggers:**
- Pull request opened, synchronized, or reopened

**Purpose:**
Fast validation for pull requests with just build and unit tests.

**Jobs:**
- Quick build validation
- Unit tests only (fast feedback)
- **Duration**: ~5-10 seconds

## Configuration

### Environment Variables

```yaml
DOTNET_VERSION: '8.0.x'
SOLUTION_PATH: './CleanApiTemplate.sln'
TEST_PROJECT: './CleanApiTemplate.Test/CleanApiTemplate.Test.csproj'
```

### Coverage Thresholds

**Minimum Code Coverage**: 70%
- Located in: `coverage-report` job
- Variable: `threshold=70.0`

**Minimum Test Completeness**: 80%
- Located in: `test-completeness` job
- Checks handlers and validators

### Secrets Required

**None** - This pipeline works out of the box!

Optional secrets for advanced scenarios:
- `CODECOV_TOKEN` - For Codecov integration
- `SONAR_TOKEN` - For SonarCloud integration

## Test Completeness Validation

The pipeline includes intelligent test completeness checks:

### What It Checks

1. **Handler Coverage**
   - Finds all `*Handler.cs` files in Application layer
   - Checks for corresponding `*HandlerTests.cs` files
   - Counts number of test methods per handler

2. **Validator Coverage**
   - Finds all `*Validator.cs` files in Application layer
   - Checks for corresponding `*ValidatorTests.cs` files
   - Counts number of test methods per validator

3. **Untested Public Methods**
   - Scans for public methods in Application layer
   - Identifies classes without test files
   - Warns about potential missing coverage

### Expected Test File Locations

```
CleanApiTemplate.Application/
  Features/Products/
    Commands/
      CreateProductCommandHandler.cs
    Queries/
      GetProductsQueryHandler.cs

CleanApiTemplate.Test/
  Application/
    Handlers/
      CreateProductCommandHandlerTests.cs  ? Must exist
      GetProductsQueryHandlerTests.cs      ? Must exist
    Validators/
      CreateProductCommandValidatorTests.cs ? Must exist
```

### How It Works

1. **Discovery Phase**: Scans Application layer for handlers/validators
2. **Validation Phase**: Checks Test layer for corresponding test files
3. **Analysis Phase**: Counts test methods and calculates completeness
4. **Reporting Phase**: Adds detailed report to GitHub Actions summary
5. **Enforcement Phase**: Fails pipeline if completeness < 80%

### Bypassing Test Completeness Check

If you need to temporarily bypass (not recommended):

```yaml
# Comment out or remove the test-completeness job dependency
needs: [code-quality, unit-tests, integration-tests, coverage-report]
# Remove: test-completeness
```

## Running Locally

### Run All Checks

```bash
# Restore dependencies
dotnet restore

# Build
dotnet build --configuration Release

# Format check
dotnet format --verify-no-changes

# Unit tests with coverage
dotnet test --filter "Category!=Integration" --collect:"XPlat Code Coverage"

# Integration tests (requires SQL Server)
dotnet test --filter "Category=Integration"

# Security scan
dotnet list package --vulnerable --include-transitive
```

### Check Test Completeness Manually

```bash
# Find handlers without tests
for handler in $(find CleanApiTemplate.Application -name "*Handler.cs"); do
  handler_name=$(basename "$handler" .cs)
  if [ ! -f "CleanApiTemplate.Test/Application/Handlers/${handler_name}Tests.cs" ]; then
    echo "Missing tests for: $handler_name"
  fi
done
```

## Artifacts

The pipeline generates several artifacts available for download:

| Artifact | Retention | Description |
|----------|-----------|-------------|
| `build-artifacts` | 7 days | Compiled binaries |
| `unit-test-coverage` | 30 days | Unit test coverage data |
| `integration-test-coverage` | 30 days | Integration test coverage data |
| `coverage-report` | 30 days | HTML coverage reports |
| `security-scan-report` | 30 days | Vulnerability scan results |

## Status Badges

Add these to your README.md:

```markdown
[![CI/CD Pipeline](https://github.com/YOUR_ORG/CleanApiTemplate/actions/workflows/ci-cd.yml/badge.svg)](https://github.com/YOUR_ORG/CleanApiTemplate/actions/workflows/ci-cd.yml)

[![PR Validation](https://github.com/YOUR_ORG/CleanApiTemplate/actions/workflows/pr-validation.yml/badge.svg)](https://github.com/YOUR_ORG/CleanApiTemplate/actions/workflows/pr-validation.yml)
```

## Viewing Results

### GitHub Actions UI

1. Go to **Actions** tab in your repository
2. Click on a workflow run
3. View individual job results
4. Check job summaries for detailed reports

### Pull Request Comments

The pipeline automatically adds comments to PRs with:
- Test results summary
- Code coverage metrics
- Test completeness analysis
- Links to detailed reports

### Job Summary

Each job adds information to the GitHub Actions summary:

**Coverage Report Example:**
```
## Code Coverage Summary
Line Coverage: 85.3%
Branch Coverage: 78.9%
Method Coverage: 92.1%

? Coverage meets minimum threshold (70%)
```

**Test Completeness Example:**
```
## Test Completeness Analysis
Handlers: 8/8 (100%)
Validators: 5/5 (100%)
Overall: 95%

? All critical classes have tests
```

## Integration with Branch Protection

Recommended branch protection rules for `main` and `develop`:

1. **Require status checks to pass before merging**
   - ? Code Quality & Build
   - ? Unit Tests
   - ? Integration Tests
   - ? Code Coverage Analysis
   - ? Test Completeness Check
   - ? Security Scan

2. **Require pull request reviews**: 1 approval

3. **Require branches to be up to date before merging**

4. **Include administrators** (optional)

## Troubleshooting

### Pipeline Fails on Coverage Threshold

**Issue**: Coverage is below 70%

**Solutions:**
1. Add more unit tests
2. Remove dead code
3. Adjust threshold in `ci-cd.yml` (line with `threshold=70.0`)

### Pipeline Fails on Test Completeness

**Issue**: Missing test files detected

**Solutions:**
1. Create missing test files following naming convention
2. Add tests for new handlers/validators
3. Temporarily adjust threshold (not recommended)

### SQL Server Container Fails

**Issue**: Integration tests can't connect to database

**Solutions:**
1. Check SQL Server health check configuration
2. Increase wait time in "Wait for SQL Server" step
3. Verify connection string environment variable

### Format Check Fails

**Issue**: Code formatting doesn't match rules

**Solutions:**
```bash
# Auto-format locally
dotnet format

# Commit formatted code
git add .
git commit -m "Apply code formatting"
```

### Security Scan Fails

**Issue**: Vulnerable packages detected

**Solutions:**
1. Update vulnerable packages: `dotnet list package --outdated`
2. Run: `dotnet add package [PackageName] --version [NewVersion]`
3. Test thoroughly after updates

## Maintenance

### Update .NET Version

```yaml
# Change in ci-cd.yml and pr-validation.yml
env:
  DOTNET_VERSION: '9.0.x'  # Update as needed
```

### Update SQL Server Version

```yaml
# Change in ci-cd.yml, integration-tests job
services:
  sqlserver:
    image: mcr.microsoft.com/mssql/server:2025-latest  # Update as needed
```

### Add New Test Categories

If you add new test categories (e.g., `Category=E2E`):

```yaml
- name: ?? Run E2E Tests
  run: |
    dotnet test ${{ env.TEST_PROJECT }} \
      --filter "Category=E2E" \
      --logger "trx" \
      --collect:"XPlat Code Coverage"
```

## Best Practices

### For Developers

1. **Run tests locally before pushing**
   ```bash
   dotnet test
   ```

2. **Check code coverage locally**
   ```bash
   dotnet test --collect:"XPlat Code Coverage"
   ```

3. **Create tests alongside features**
   - Write handler test immediately after handler
   - Write validator test immediately after validator

4. **Follow naming conventions**
   - Handler: `{Name}Handler.cs` ? Test: `{Name}HandlerTests.cs`
   - Validator: `{Name}Validator.cs` ? Test: `{Name}ValidatorTests.cs`

### For Reviewers

1. **Check pipeline status before approving PR**
2. **Review coverage reports**
3. **Ensure test completeness is 100%**
4. **Verify no security vulnerabilities**

## Advanced Scenarios

### Adding SonarCloud Integration

```yaml
- name: SonarCloud Scan
  uses: SonarSource/sonarcloud-github-action@master
  env:
    GITHUB_TOKEN: ${{ secrets.GITHUB_TOKEN }}
    SONAR_TOKEN: ${{ secrets.SONAR_TOKEN }}
```

### Adding Codecov Integration

```yaml
- name: Upload to Codecov
  uses: codecov/codecov-action@v3
  with:
    files: ./coverage/**/coverage.cobertura.xml
    flags: unittests
    name: codecov-umbrella
```

### Adding Slack Notifications

```yaml
- name: Notify Slack
  uses: 8398a7/action-slack@v3
  if: always()
  with:
    status: ${{ job.status }}
    webhook_url: ${{ secrets.SLACK_WEBHOOK }}
```

## Summary

This CI/CD pipeline provides:

? **Automated Testing** - Unit and integration tests  
? **Code Coverage** - Minimum 70% threshold  
? **Test Completeness** - Ensures tests aren't forgotten  
? **Security Scanning** - Detects vulnerabilities  
? **Code Quality** - Format checking  
? **Fast Feedback** - PR validation in seconds  
? **Detailed Reporting** - Rich summaries and artifacts  
? **Production Ready** - No secrets required  

The pipeline is designed to catch issues early, maintain high code quality, and ensure comprehensive test coverage.
