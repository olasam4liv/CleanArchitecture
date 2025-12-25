using SharedKernel.Model.Responses;

namespace Application.Abstractions.Messaging;

public interface IQueryHandler<in TQuery, TResponse>
    where TQuery : IQuery<TResponse>
{
    Task<ResponseModel<TResponse>> Handle(TQuery query, CancellationToken cancellationToken);
}
