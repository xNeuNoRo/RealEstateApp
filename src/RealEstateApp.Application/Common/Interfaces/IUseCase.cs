using RealEstateApp.Domain.Common;

namespace RealEstateApp.Application.Common.Interfaces;

/// <summary>
/// Contrato base para Use Cases con DTO de entrada y respuesta tipada.
/// </summary>
public interface IUseCase<in TRequest, TResponse>
    where TRequest : notnull
{
    Task<Result<TResponse>> ExecuteAsync(
        TRequest request,
        CancellationToken cancellationToken = default
    );
}

/// <summary>
/// Contrato base para Use Cases con DTO de entrada que solo informan éxito/fallo
/// (p.ej. <c>ActivateAccount</c>, <c>ForgotPassword</c>, <c>DeleteProperty</c>).
/// </summary>
public interface IUseCase<in TRequest>
    where TRequest : notnull
{
    Task<Result> ExecuteAsync(TRequest request, CancellationToken cancellationToken = default);
}
