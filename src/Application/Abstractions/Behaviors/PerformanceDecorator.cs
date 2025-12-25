using System.Diagnostics;
using Application.Abstractions.Messaging;
using Microsoft.Extensions.Logging;
using SharedKernel.Model.Responses;

namespace Application.Abstractions.Behaviors;

internal static class PerformanceDecorator
{
    private const int ThresholdMilliseconds = 500;

    internal sealed class CommandHandler<TCommand, TResponse>(
        ICommandHandler<TCommand, TResponse> innerHandler,
        ILogger<CommandHandler<TCommand, TResponse>> logger)
        : ICommandHandler<TCommand, TResponse>
        where TCommand : ICommand<TResponse>
    {
        public async Task<ResponseModel<TResponse>> Handle(TCommand command, CancellationToken cancellationToken)
        {
            var stopwatch = Stopwatch.StartNew();

            ResponseModel<TResponse> result = await innerHandler.Handle(command, cancellationToken);

            stopwatch.Stop();

            long elapsedMilliseconds = stopwatch.ElapsedMilliseconds;

            if (elapsedMilliseconds >= ThresholdMilliseconds)
            {
                string commandName = typeof(TCommand).Name;

                logger.LogWarning(
                    "Long-running command {Command} took {ElapsedMilliseconds} ms",
                    commandName,
                    elapsedMilliseconds);
            }

            return result;
        }
    }

    internal sealed class CommandBaseHandler<TCommand>(
        ICommandHandler<TCommand> innerHandler,
        ILogger<CommandBaseHandler<TCommand>> logger)
        : ICommandHandler<TCommand>
        where TCommand : ICommand
    {
        public async Task<ResponseModel> Handle(TCommand command, CancellationToken cancellationToken)
        {
            var stopwatch = Stopwatch.StartNew();

            ResponseModel result = await innerHandler.Handle(command, cancellationToken);

            stopwatch.Stop();

            long elapsedMilliseconds = stopwatch.ElapsedMilliseconds;

            if (elapsedMilliseconds >= ThresholdMilliseconds)
            {
                string commandName = typeof(TCommand).Name;

                logger.LogWarning(
                    "Long-running command {Command} took {ElapsedMilliseconds} ms",
                    commandName,
                    elapsedMilliseconds);
            }

            return result;
        }
    }

    internal sealed class QueryHandler<TQuery, TResponse>(
        IQueryHandler<TQuery, TResponse> innerHandler,
        ILogger<QueryHandler<TQuery, TResponse>> logger)
        : IQueryHandler<TQuery, TResponse>
        where TQuery : IQuery<TResponse>
    {
        public async Task<ResponseModel<TResponse>> Handle(TQuery query, CancellationToken cancellationToken)
        {
            var stopwatch = Stopwatch.StartNew();

            ResponseModel<TResponse> result = await innerHandler.Handle(query, cancellationToken);

            stopwatch.Stop();

            long elapsedMilliseconds = stopwatch.ElapsedMilliseconds;

            if (elapsedMilliseconds >= ThresholdMilliseconds)
            {
                string queryName = typeof(TQuery).Name;

                logger.LogWarning(
                    "Long-running query {Query} took {ElapsedMilliseconds} ms",
                    queryName,
                    elapsedMilliseconds);
            }

            return result;
        }
    }
}
