using MediatR;

namespace CleanApiTemplate.Application.Common;

/// <summary>
/// Base class for all query requests in CQRS pattern
/// Queries return data without modifying state
/// </summary>
/// <typeparam name="TResponse">Type of the query response</typeparam>
public abstract class QueryBase<TResponse> : IRequest<TResponse>
{
}
