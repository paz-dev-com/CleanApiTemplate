using CleanApiTemplate.Application.Common;
using CleanApiTemplate.Application.Features.Products.Commands;
using CleanApiTemplate.Application.Features.Products.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CleanApiTemplate.API.Controllers;

/// <summary>
/// Products API controller
/// Demonstrates Clean Architecture controller with CQRS pattern
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ProductsController(IMediator mediator, ILogger<ProductsController> logger) : ControllerBase
{
    private readonly IMediator _mediator = mediator;
    private readonly ILogger<ProductsController> _logger = logger;

    /// <summary>
    /// Get paginated list of products
    /// </summary>
    /// <param name="pageNumber">Page number (1-based)</param>
    /// <param name="pageSize">Number of items per page</param>
    /// <param name="searchTerm">Optional search term</param>
    /// <param name="categoryId">Optional category filter</param>
    /// <param name="includeInactive">Include inactive products</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated list of products</returns>
    [HttpGet]
    [ProducesResponseType(typeof(Result<PaginatedResult<ProductDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Result<PaginatedResult<ProductDto>>>> GetProducts(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? searchTerm = null,
        [FromQuery] Guid? categoryId = null,
        [FromQuery] bool includeInactive = false,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting products: Page {PageNumber}, Size {PageSize}, Search: {SearchTerm}",
            pageNumber, pageSize, searchTerm);

        var query = new GetProductsQuery
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            SearchTerm = searchTerm,
            CategoryId = categoryId,
            IncludeInactive = includeInactive
        };

        var result = await _mediator.Send(query, cancellationToken);

        if (result.IsFailure)
        {
            _logger.LogWarning("Failed to get products: {Error}", result.Error);
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Get a specific product by ID
    /// </summary>
    /// <param name="id">Product ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Product details</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(Result<ProductDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Result<ProductDto>>> GetProduct(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting product {ProductId}", id);

        var query = new GetProductByIdQuery { Id = id };
        var result = await _mediator.Send(query, cancellationToken);

        if (result.IsFailure)
        {
            _logger.LogWarning("Failed to get product {ProductId}: {Error}", id, result.Error);
            return NotFound(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Create a new product
    /// </summary>
    /// <param name="command">Product creation data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created product ID</returns>
    [HttpPost]
    [Authorize] // Requires authentication
    [ProducesResponseType(typeof(Result<Guid>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<Result<Guid>>> CreateProduct(
        [FromBody] CreateProductCommand command,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Creating product: {ProductName}", command.Name);

        var result = await _mediator.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            _logger.LogWarning("Failed to create product: {Error}", result.Error);
            return BadRequest(result);
        }

        _logger.LogInformation("Product created successfully: {ProductId}", result.Data);
        return CreatedAtAction(nameof(GetProduct), new { id = result.Data }, result);
    }

    /// <summary>
    /// Update an existing product
    /// </summary>
    /// <param name="id">Product ID</param>
    /// <param name="command">Product update data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Success result</returns>
    [HttpPut("{id}")]
    [Authorize]
    [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<Result>> UpdateProduct(
        Guid id,
        [FromBody] UpdateProductCommand command,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Updating product {ProductId}", id);

        // Ensure the ID in the route matches the ID in the command
        if (id != command.Id)
        {
            command.Id = id;
        }

        var result = await _mediator.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            _logger.LogWarning("Failed to update product {ProductId}: {Error}", id, result.Error);
            
            // Return appropriate status code based on error message
            if (result.Error?.Contains("not found") == true)
            {
                return NotFound(result);
            }
            
            return BadRequest(result);
        }

        _logger.LogInformation("Product updated successfully: {ProductId}", id);
        return Ok(result);
    }

    /// <summary>
    /// Delete a product (soft delete)
    /// </summary>
    /// <param name="id">Product ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Success result</returns>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")] // Requires Admin role
    [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<Result>> DeleteProduct(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Deleting product {ProductId}", id);

        var command = new DeleteProductCommand { Id = id };
        var result = await _mediator.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            _logger.LogWarning("Failed to delete product {ProductId}: {Error}", id, result.Error);
            return NotFound(result);
        }

        _logger.LogInformation("Product deleted successfully: {ProductId}", id);
        return Ok(result);
    }
}