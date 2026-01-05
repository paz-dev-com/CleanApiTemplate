using MediatR;

namespace CleanApiTemplate.Application.Common;

/// <summary>
/// Base class for all command requests in CQRS pattern
/// Commands modify state and may or may not return data
/// </summary>
/// <typeparam name="TResponse">Type of the command response</typeparam>
public abstract class CommandBase<TResponse> : IRequest<TResponse>
{
}

/// <summary>
/// Base class for commands that don't return a value
/// </summary>
public abstract class CommandBase : IRequest<Unit>
{
}
