# Code Quality & Formatting Configuration

This document describes the code quality, formatting, and analysis configurations applied to the CleanApiTemplate solution.

## Configuration Files

### 1. `.editorconfig` (Solution Root)
Defines code style rules, formatting preferences, naming conventions, and analyzer settings.

**Key Features:**
- ✅ **Unused Imports**: `IDE0005` - Warns about unnecessary using directives
- ✅ **Code Style**: Enforces consistent C# coding patterns
- ✅ **Naming Conventions**: 
  - Interfaces must start with `I` (e.g., `IRepository`)
  - Private fields must start with `_` (e.g., `_dbContext`)
  - Async methods must end with `Async` (e.g., `GetProductAsync`)
- ✅ **Formatting**: Consistent indentation, spacing, and line breaks
- ✅ **Code Quality**: Detects unused code, unnecessary casts, unreachable code

### 2. `Directory.Build.props` (Solution Root)
Global MSBuild properties applied to all projects in the solution.

**Key Features:**
- ✅ **Code Analysis**: Enabled for all projects with latest analyzers
- ✅ **Style Enforcement**: Code style violations cause build warnings
- ✅ **Analysis Mode**: Full analysis mode enabled (`All`)
- ✅ **Documentation**: XML documentation generation enabled

## Code Quality Rules

### Critical Rules (Warnings)

| Rule | Description | Severity |
|------|-------------|----------|
| `IDE0005` | Remove unnecessary using directives | Warning |
| `IDE0051` | Remove unused private member | Warning |
| `IDE0052` | Remove unread private member | Warning |
| `IDE0059` | Unnecessary assignment of a value | Warning |
| `IDE0060` | Remove unused parameter | Warning |
| `CA1001` | Types that own disposable fields should be disposable | Warning |
| `CA1806` | Do not ignore method results | Warning |
| `csharp_prefer_braces` | Always use braces for control statements | Warning |
| `csharp_style_namespace_declarations` | Use file-scoped namespaces | Warning |

### Naming Conventions

```csharp
// ✅ Correct
public interface IProductService { }
public class ProductService { }
private readonly IRepository<Product> _repository;
public async Task<Product> GetProductAsync(Guid id) { }
private const int MaxItems = 100;

// ❌ Incorrect
public interface ProductService { }  // Missing 'I' prefix
public class productService { }      // Should be PascalCase
private readonly IRepository<Product> repository; // Missing '_' prefix
public async Task<Product> GetProduct(Guid id) { } // Missing 'Async' suffix
private const int maxItems = 100;    // Should be PascalCase
```

### Code Style Preferences

```csharp
// ✅ Preferred - File-scoped namespace (C# 10+)
namespace CleanApiTemplate.Application.Features.Products;

public class ProductService { }

// ❌ Not preferred - Traditional namespace
namespace CleanApiTemplate.Application.Features.Products
{
    public class ProductService { }
}
```

```csharp
// ✅ Preferred - Always use braces
if (condition)
{
    DoSomething();
}

// ❌ Not preferred - No braces
if (condition)
    DoSomething();
```

```csharp
// ✅ Preferred - Use 'var' when type is apparent
var product = new Product();
var results = await _repository.GetAllAsync();

// ❌ Not preferred - Explicit type when apparent
Product product = new Product();
```

```csharp
// ✅ Preferred - No 'this' qualification
public class ProductService
{
    private readonly IRepository<Product> _repository;
    
    public void DoSomething()
    {
        _repository.GetAll();
    }
}

// ❌ Not preferred - Unnecessary 'this'
public class ProductService
{
    private readonly IRepository<Product> _repository;
    
    public void DoSomething()
    {
        this._repository.GetAll();
    }
}
```

## Using Your IDE

### Visual Studio 2022

#### Automatic Formatting
- **Format Document**: `Ctrl+K, Ctrl+D`
- **Format Selection**: `Ctrl+K, Ctrl+F`
- **Remove Unused Usings**: `Ctrl+R, Ctrl+G`
- **Sort Usings**: `Ctrl+R, Ctrl+S`

#### Enable Format on Save
1. Tools → Options → Text Editor → C# → Code Style → Formatting → General
2. Check "Automatically format on semicolon" and "Automatically format on closing brace"
3. Or install extension: [Format on Save](https://marketplace.visualstudio.com/items?itemName=mynkow.FormatdocumentonSave)

#### View Code Analysis
- **View → Error List**
- Filter by severity (Errors, Warnings, Messages)
- Double-click any issue to navigate to the code

#### Quick Fixes
- Press `Ctrl+.` or click the light bulb 💡 icon
- Select a fix or "Suppress" the warning

### Visual Studio Code

#### Extensions
Install these extensions for full support:
- **C# Dev Kit** (Microsoft)
- **C#** (Microsoft)
- **EditorConfig for VS Code**

#### Automatic Formatting
- **Format Document**: `Shift+Alt+F`
- **Format Selection**: `Ctrl+K, Ctrl+F`

#### Enable Format on Save
Add to `.vscode/settings.json`:
```json
{
  "editor.formatOnSave": true,
  "editor.codeActionsOnSave": {
    "source.fixAll": true,
    "source.organizeImports": true
  }
}
```

### JetBrains Rider

#### Automatic Formatting
- **Format Document**: `Ctrl+Alt+Enter`
- **Optimize Imports**: `Ctrl+Alt+O`

#### Enable Format on Save
1. Settings → Tools → Actions on Save
2. Check "Reformat code" and "Optimize imports"

## Command Line (dotnet CLI)

### Build with Code Analysis
```bash
# Build and show all warnings
dotnet build

# Build treating warnings as errors
dotnet build /p:TreatWarningsAsErrors=true

# Clean and rebuild
dotnet clean && dotnet build
```

### Format Code
Install `dotnet format` tool:
```bash
dotnet tool install -g dotnet-format
```

Format the entire solution:
```bash
# Check for formatting issues
dotnet format --verify-no-changes

# Apply formatting fixes
dotnet format

# Apply formatting and fix analyzers
dotnet format --fix-style warn --fix-analyzers warn
```

## CI/CD Integration

### GitHub Actions Example
```yaml
name: Code Quality Check

on: [push, pull_request]

jobs:
  code-quality:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      
      - name: Setup .NET
        uses: actions/setup-dotnet@v3
        with:
          dotnet-version: '10.0.x'
      
      - name: Restore dependencies
        run: dotnet restore
      
      - name: Check code formatting
        run: dotnet format --verify-no-changes
      
      - name: Build with warnings as errors
        run: dotnet build --no-restore /p:TreatWarningsAsErrors=true
      
      - name: Run tests
        run: dotnet test --no-build --verbosity normal
```

### Azure DevOps Pipeline Example
```yaml
trigger:
- main
- develop

pool:
  vmImage: 'ubuntu-latest'

steps:
- task: UseDotNet@2
  inputs:
    version: '10.0.x'

- script: dotnet restore
  displayName: 'Restore packages'

- script: dotnet format --verify-no-changes
  displayName: 'Verify code formatting'

- script: dotnet build --configuration Release /p:TreatWarningsAsErrors=true
  displayName: 'Build with strict warnings'

- script: dotnet test --configuration Release --logger trx
  displayName: 'Run tests'
```

## Common Issues & Solutions

### Issue: Too many warnings after enabling rules
**Solution 1**: Gradually increase strictness
1. Start with critical rules only
2. Fix existing violations
3. Enable additional rules incrementally

**Solution 2**: Suppress specific warnings in `.editorconfig`
```ini
# Make IDE0005 a suggestion instead of warning temporarily
dotnet_diagnostic.IDE0005.severity = suggestion
```

### Issue: Legacy code doesn't follow conventions
**Solution**: Use `.editorconfig` per-directory overrides
```ini
# In legacy code directory
[LegacyCode/**/*.cs]
dotnet_diagnostic.IDE1006.severity = none
```

### Issue: False positive warnings
**Solution**: Suppress with `#pragma` or `[SuppressMessage]`
```csharp
#pragma warning disable IDE0051 // Remove unused private member
private void UsedByReflection() { }
#pragma warning restore IDE0051

// OR
[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0051:Remove unused private members")]
private void UsedByReflection() { }
```

### Issue: Code formatting conflicts with team preferences
**Solution**: 
1. Review and update `.editorconfig` as a team
2. Commit the agreed-upon `.editorconfig` to source control
3. Everyone's IDE will automatically use the same settings

## Best Practices

### ✅ Do's
- ✅ Run `dotnet format` before committing code
- ✅ Enable format-on-save in your IDE
- ✅ Fix warnings promptly - don't let them accumulate
- ✅ Review code analysis suggestions - they often improve code quality
- ✅ Use the light bulb 💡 quick fixes
- ✅ Keep `.editorconfig` in source control

### ❌ Don'ts
- ❌ Don't suppress warnings without understanding them
- ❌ Don't disable all analyzers to "make it build"
- ❌ Don't ignore unused imports/code
- ❌ Don't commit code with unresolved warnings
- ❌ Don't use different formatting settings across the team

## Verifying Configuration

Run these commands to verify everything is working:

```bash
# 1. Check for formatting issues
dotnet format --verify-no-changes

# 2. Build with all warnings visible
dotnet build

# 3. Build treating warnings as errors
dotnet build /p:TreatWarningsAsErrors=true

# 4. Run tests
dotnet test
```

Expected output:
- ✅ No formatting issues found
- ✅ Build succeeded with 0 errors
- ✅ All tests passed

## Further Reading

- [EditorConfig Documentation](https://editorconfig.org/)
- [.NET Code Style Rules](https://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/style-rules/)
- [C# Coding Conventions](https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions)
- [Code Analysis Configuration](https://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/configuration-files)
- [dotnet format CLI](https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-format)

## Summary

This configuration provides:
- ✅ **Consistent Code Style**: Enforced across all developers and projects
- ✅ **Early Issue Detection**: Unused code, potential bugs caught at build time
- ✅ **Better Maintainability**: Clean, readable, standardized code
- ✅ **IDE Integration**: Works with Visual Studio, VS Code, and Rider
- ✅ **CI/CD Ready**: Can verify code quality in automated pipelines

All rules are documented, customizable, and aligned with .NET best practices and Clean Architecture principles.
