namespace RealEstateApp.Application.ViewModels.Shared;

public interface IViewModelBuilder<T>
    where T : BaseViewModel
{
    Task PopulateBaseAsync(T vm, CancellationToken ct = default);
}
