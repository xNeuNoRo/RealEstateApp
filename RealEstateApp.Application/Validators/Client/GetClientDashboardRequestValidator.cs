using FluentValidation;
using RealEstateApp.Application.Dtos.Client.Requests;

namespace RealEstateApp.Application.Validators.Client;

public sealed class GetClientDashboardRequestValidator
    : AbstractValidator<GetClientDashboardRequest> { }
