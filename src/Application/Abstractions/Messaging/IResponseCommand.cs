using SharedKernel.Model.Responses;

namespace Application.Abstractions.Messaging;

public interface IResponseCommand<TResponse>;

public interface IResponseCommandHandler<in TCommand, TResponse>
    where TCommand : IResponseCommand<TResponse>
{
    Task<ResponseModel<TResponse>> Handle(TCommand command, CancellationToken cancellationToken);
}
