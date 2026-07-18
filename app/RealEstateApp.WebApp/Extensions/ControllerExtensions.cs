using Microsoft.AspNetCore.Mvc;
using RealEstateApp.Application.ViewModels.Shared;

namespace RealEstateApp.WebApp.Extensions;

public static class ControllerExtensions
{
    public static async Task PopulateBaseViewModelAsync<T>(
        this Controller controller,
        T vm,
        IViewModelBuilder<BaseViewModel> builder,
        CancellationToken ct = default
    )
        where T : BaseViewModel
    {
        await builder.PopulateBaseAsync(vm, ct);

        if (vm.IsAuthenticated)
        {
            controller.ViewBag.CurrentUserId = vm.CurrentUserId;
            controller.ViewBag.CurrentUserFullName = vm.CurrentUserFullName;
            controller.ViewBag.UnreadNotificationsCount = vm.UnreadNotificationsCount;
        }
    }
}
