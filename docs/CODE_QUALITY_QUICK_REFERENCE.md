# Code Quality Quick Reference

## Daily Development Workflow

### Before You Commit

```bash
# 1. Format your code
dotnet format CleanApiTemplate.sln

# 2. Check for issues
dotnet format CleanApiTemplate.sln --verify-no-changes

# 3. Build to see warnings
dotnet build

# 4. Run tests
cd test-scripts
.\run-unit-tests.ps1  # Fast (~1s)
```

## Keyboard Shortcuts

### Visual Studio 2022

| Action | Shortcut |
|--------|----------|
| Format Document | `Ctrl+K, Ctrl+D` |
| Format Selection | `Ctrl+K, Ctrl+F` |
| Remove Unused Usings | `Ctrl+R, Ctrl+G` |
| Sort Usings | `Ctrl+R, Ctrl+S` |
| Quick Actions | `Ctrl+.` |

### Visual Studio Code

| Action | Shortcut |
|--------|----------|
| Format Document | `Shift+Alt+F` |
| Format Selection | `Ctrl+K, Ctrl+F` |
| Quick Fix | `Ctrl+.` |
| Organize Imports | Automatic on save |

### JetBrains Rider

| Action | Shortcut |
|--------|----------|
| Format Document | `Ctrl+Alt+Enter` |
| Optimize Imports | `Ctrl+Alt+O` |
| Quick Fixes | `Alt+Enter` |

## Common Code Style Rules

### ✅ Do This

```csharp
// File-scoped namespace
namespace CleanApiTemplate.Application.Features;

// Interfaces start with I
public interface IProductService { }

// Private fields start with underscore
private readonly IRepository<Product> _repository;

// Async methods end with Async
public async Task<Product> GetProductAsync(Guid id) { }

// Always use braces
if (condition)
{
    DoSomething();
}

// Use var when type is apparent
var product = new Product();
```

### ❌ Don't Do This

```csharp
// Traditional namespace
namespace CleanApiTemplate.Application.Features
{
    public class MyClass { }
}

// Interface without I prefix
public interface ProductService { }

// Private field without underscore
private readonly IRepository<Product> repository;

// Async method without Async suffix
public async Task<Product> GetProduct(Guid id) { }

// No braces
if (condition)
    DoSomething();

// Explicit type when apparent
Product product = new Product();
```

## Fixing Common Issues

### Unused Imports

**Visual Studio:**
```
Ctrl+R, Ctrl+G
```

**VS Code/Rider:**
- Save the file (format on save removes them automatically)

**Command Line:**
```bash
dotnet format CleanApiTemplate.sln
```

### Wrong Indentation

**Any IDE:**
1. Select all (`Ctrl+A`)
2. Format document (`Ctrl+K, Ctrl+D` in VS)

**Command Line:**
```bash
dotnet format CleanApiTemplate.sln
```

### Naming Violations

**Quick Fix:**
1. Place cursor on the violation
2. Press `Ctrl+.` (or click light bulb 💡)
3. Select "Rename to ..."

### Multiple Errors

**Fastest Way:**
```bash
# Fix all auto-fixable issues at once
dotnet format CleanApiTemplate.sln
```

## Suppressing Warnings

### In Code (When Justified)

```csharp
#pragma warning disable CA1062 // Validate arguments of public methods
public void MyMethod(string input)
{
    // Method body
}
#pragma warning restore CA1062
```

Or:

```csharp
[System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1062:Validate arguments of public methods")]
public void MyMethod(string input)
{
    // Method body
}
```

### In .editorconfig (Project-Wide)

```ini
# Make rule less strict
dotnet_diagnostic.CA1062.severity = suggestion

# Disable rule completely
dotnet_diagnostic.CA1062.severity = none
```

## CI/CD Commands

### Check Everything

```bash
# Format check (fail if not formatted)
dotnet format CleanApiTemplate.sln --verify-no-changes

# Build with warnings visible
dotnet build

# Build treating warnings as errors
dotnet build /p:TreatWarningsAsErrors=true
```

### Fix Everything

```bash
# Format all code
dotnet format CleanApiTemplate.sln

# Build
dotnet build
```

## Editor Configuration

### VS Code

The template includes `.vscode/settings.json` with:
- ✅ Format on save enabled
- ✅ Organize imports on save
- ✅ EditorConfig support

Just install recommended extensions when prompted!

### Visual Studio 2022

EditorConfig is automatically applied. Optional:
1. Tools → Options → Text Editor → C# → Code Style → Formatting
2. Enable "Automatically format on semicolon"
3. Enable "Automatically format on closing brace"

### Rider

EditorConfig is automatically applied. Optional:
1. Settings → Tools → Actions on Save
2. Enable "Reformat code"
3. Enable "Optimize imports"

## Recommended Extensions

### VS Code
- C# Dev Kit (Microsoft)
- C# (Microsoft)
- EditorConfig for VS Code

### Visual Studio
- Fine Code Coverage (for coverage visualization)

### Rider
- Built-in support, no extensions needed

## Troubleshooting

### "Too many warnings!"

**Temporary Fix:**
```bash
# Build without treating warnings as errors
dotnet build
```

**Permanent Fix:**
Update `.editorconfig` to make rules less strict:
```ini
dotnet_diagnostic.IDE0005.severity = suggestion  # Was: warning
```

### "My code keeps getting reformatted differently"

**Problem:** Multiple developers with different settings.

**Solution:** 
1. Ensure everyone has EditorConfig support in their IDE
2. Commit `.editorconfig` to source control
3. Everyone pulls latest `.editorconfig`
4. Format code: `dotnet format CleanApiTemplate.sln`

### "Format on save not working"

**VS Code:**
1. Check `.vscode/settings.json` exists
2. Install EditorConfig extension
3. Reload window (`Ctrl+Shift+P` → "Reload Window")

**Rider:**
1. Settings → Tools → Actions on Save
2. Check "Reformat code" is enabled
3. Check "Optimize imports" is enabled

## Resources

- **Full Documentation:** [docs/CODE_QUALITY.md](../docs/CODE_QUALITY.md)
- **EditorConfig Docs:** https://editorconfig.org/
- **.NET Code Style Rules:** https://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/style-rules/

## Quick Tips

💡 **Tip 1:** Run `dotnet format` before every commit
💡 **Tip 2:** Enable format-on-save in your IDE
💡 **Tip 3:** Use `Ctrl+.` to quick-fix issues
💡 **Tip 4:** Don't suppress warnings without understanding them
💡 **Tip 5:** Commit `.editorconfig` to source control for team consistency

---

**Remember:** Code quality tools are here to help, not to annoy! They catch bugs early and make code more maintainable. 🚀
