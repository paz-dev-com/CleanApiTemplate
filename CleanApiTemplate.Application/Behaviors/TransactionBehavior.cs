using CleanApiTemplate.Core.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CleanApiTemplate.Application.Behaviors;

/// <summary>
/// Pipeline behavior for automatically managing database transactions
/// Demonstrates Unit of Work pattern with automatic transaction handling
/// </summary>
/// <typeparam name="TRequest">Request type</typeparam>
/// <typeparam name="TResponse">Response type</typeparam>
public class TransactionBehavior<TRequest, TResponse>(
    IUnitOfWork unitOfWork,
    ILogger<TransactionBehavior<TRequest, TResponse>> logger) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly ILogger<TransactionBehavior<TRequest, TResponse>> _logger = logger;

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        Type requestType = request.GetType();
        bool isCommand = requestType.Name.EndsWith("Command", StringComparison.Ordinal);

        if (!isCommand)
        {
            return await next(cancellationToken);
        }

        string requestName = typeof(TRequest).Name;

        try
        {
            _logger.LogInformation("Beginning transaction for {RequestName}", requestName);
            
            await _unitOfWork.BeginTransactionAsync(cancellationToken);

            TResponse? response = await next(cancellationToken);

            await _unitOfWork.CommitTransactionAsync(cancellationToken);
            
            _logger.LogInformation("Transaction committed successfully for {RequestName}", requestName);

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Transaction failed for {RequestName}. Rolling back changes", requestName);
            
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            
            throw;
        }
    }
}
