using FluentValidation;
using RealEstateApp.Application.Dtos.Admin.Requests;

namespace RealEstateApp.Application.Validators.Admin;

public sealed class GetAdminDashboardRequestValidator : AbstractValidator<GetAdminDashboardRequest>
{
    public GetAdminDashboardRequestValidator() { }
}
