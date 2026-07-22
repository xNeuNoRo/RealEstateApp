using RealEstateApp.Application.Interfaces.Services;

namespace RealEstateApp.Application.ViewModels.Shared;

public sealed class ViewModelBuilder : IViewModelBuilder<BaseViewModel>
{
    private readonly ICurrentUserService _currentUser;

    public ViewModelBuilder(ICurrentUserService currentUser)
    {
        _currentUser = currentUser;
    }

    public async Task PopulateBaseAsync(BaseViewModel vm, CancellationToken ct = default)
    {
        vm.IsAuthenticated = _currentUser.IsAuthenticated;
        vm.CurrentUserId = _currentUser.UserId;
        vm.CurrentUserFullName = _currentUser.FullName;
        vm.CurrentUserName = _currentUser.UserName;
        vm.Roles = _currentUser.Roles;

        await Task.CompletedTask;
    }
}
