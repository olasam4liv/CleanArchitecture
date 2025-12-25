using SharedKernel.Model.Responses;

namespace Application.Abstractions.Messaging;

public interface ICommandHandler<in TCommand>
    where TCommand : ICommand
{
    Task<ResponseModel> Handle(TCommand command, CancellationToken cancellationToken);
}

public interface ICommandHandler<in TCommand, TResponse>
    where TCommand : ICommand<TResponse>
{
    Task<ResponseModel<TResponse>> Handle(TCommand command, CancellationToken cancellationToken);
}
