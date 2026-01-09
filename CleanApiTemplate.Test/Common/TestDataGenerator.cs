using Bogus;
using CleanApiTemplate.Core.Entities;

namespace CleanApiTemplate.Test.Common;

/// <summary>
/// Provides fake data generators for testing using Bogus library
/// </summary>
public static class TestDataGenerator
{
    public static Faker<Product> ProductFaker => new Faker<Product>()
        .RuleFor(p => p.Id, f => f.Random.Guid())
        .RuleFor(p => p.Name, f => f.Commerce.ProductName())
        .RuleFor(p => p.Description, f => f.Commerce.ProductDescription())
        .RuleFor(p => p.Sku, f => f.Commerce.Ean8().ToUpper())
        .RuleFor(p => p.Price, f => f.Finance.Amount(1, 10000))
        .RuleFor(p => p.StockQuantity, f => f.Random.Int(0, 1000))
        .RuleFor(p => p.IsActive, f => f.Random.Bool())
        .RuleFor(p => p.CategoryId, f => f.Random.Guid())
        .RuleFor(p => p.CreatedBy, f => f.Internet.Email())
        .RuleFor(p => p.CreatedAt, f => f.Date.Past())
        .RuleFor(p => p.UpdatedBy, f => f.Internet.Email())
        .RuleFor(p => p.UpdatedAt, f => f.Date.Recent());

    public static Faker<Category> CategoryFaker => new Faker<Category>()
        .RuleFor(c => c.Id, f => f.Random.Guid())
        .RuleFor(c => c.Name, f => f.Commerce.Categories(1)[0])
        .RuleFor(c => c.Description, f => f.Lorem.Sentence())
        .RuleFor(c => c.CreatedBy, f => f.Internet.Email())
        .RuleFor(c => c.CreatedAt, f => f.Date.Past())
        .RuleFor(c => c.UpdatedBy, f => f.Internet.Email())
        .RuleFor(c => c.UpdatedAt, f => f.Date.Recent());

    public static Faker<User> UserFaker => new Faker<User>()
        .RuleFor(u => u.Id, f => f.Random.Guid())
        .RuleFor(u => u.Username, f => f.Internet.UserName())
        .RuleFor(u => u.Email, f => f.Internet.Email())
        .RuleFor(u => u.PasswordHash, f => f.Random.Hash())
        .RuleFor(u => u.FirstName, f => f.Name.FirstName())
        .RuleFor(u => u.LastName, f => f.Name.LastName())
        .RuleFor(u => u.IsActive, f => f.Random.Bool())
        .RuleFor(u => u.CreatedBy, f => f.Internet.Email())
        .RuleFor(u => u.CreatedAt, f => f.Date.Past())
        .RuleFor(u => u.UpdatedBy, f => f.Internet.Email())
        .RuleFor(u => u.UpdatedAt, f => f.Date.Recent());

    public static Product GenerateProduct() => ProductFaker.Generate();
    
    public static List<Product> GenerateProducts(int count) => ProductFaker.Generate(count);
    
    public static Category GenerateCategory() => CategoryFaker.Generate();
    
    public static List<Category> GenerateCategories(int count) => CategoryFaker.Generate(count);
    
    public static User GenerateUser() => UserFaker.Generate();
    
    public static List<User> GenerateUsers(int count) => UserFaker.Generate(count);
}
