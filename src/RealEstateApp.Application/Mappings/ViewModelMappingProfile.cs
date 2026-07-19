using AutoMapper;
using RealEstateApp.Application.Dtos.Admin.Requests;
using RealEstateApp.Application.Dtos.Admin.Responses;
using RealEstateApp.Application.Dtos.Agent.Requests;
using RealEstateApp.Application.Dtos.Agent.Responses;
using RealEstateApp.Application.Dtos.Auth.Requests;
using RealEstateApp.Application.Dtos.Catalog.Requests;
using RealEstateApp.Application.Dtos.Catalog.Responses;
using RealEstateApp.Application.Dtos.Chat.Requests;
using RealEstateApp.Application.Dtos.Chat.Responses;
using RealEstateApp.Application.Dtos.Client.Requests;
using RealEstateApp.Application.Dtos.Client.Responses;
using RealEstateApp.Application.Dtos.Favorites.Responses;
using RealEstateApp.Application.Dtos.Offers.Requests;
using RealEstateApp.Application.Dtos.Offers.Responses;
using RealEstateApp.Application.Dtos.Property.Requests;
using RealEstateApp.Application.Dtos.Property.Responses;
using RealEstateApp.Application.ViewModels.Admin;
using RealEstateApp.Application.ViewModels.Agent;
using RealEstateApp.Application.ViewModels.Auth;
using RealEstateApp.Application.ViewModels.Chat;
using RealEstateApp.Application.ViewModels.Client;
using RealEstateApp.Application.ViewModels.Offers;
using RealEstateApp.Application.ViewModels.Property;
using RealEstateApp.Application.ViewModels.Home;
using RealEstateApp.Application.ViewModels.Shared;

namespace RealEstateApp.Application.Mappings;

public sealed class ViewModelMappingProfile : Profile
{
    public ViewModelMappingProfile()
    {
        MapAuth();
        MapProperty();
        MapClient();
        MapAgent();
        MapAdmin();
        MapCatalog();
        MapChat();
        MapOffers();
        MapShared();
    }

    private void MapAuth()
    {
        CreateMap<LoginViewModel, LoginRequest>();
        CreateMap<ForgotPasswordViewModel, ForgotPasswordRequest>()
            .ForMember(d => d.Origin, opt => opt.Ignore());
        CreateMap<ResetPasswordViewModel, ResetPasswordRequest>();
        CreateMap<ChangePasswordViewModel, ChangePasswordRequest>();
    }

    private void MapProperty()
    {
        CreateMap<PropertyListItemResponse, PropertyListItemViewModel>();
        CreateMap<PropertyDetailResponse, PropertyDetailPublicViewModel>();
        CreateMap<PropertyFilterViewModel, GetPropertyListRequest>();
        CreateMap<PropertySearchByCodeViewModel, SearchPropertyByCodeRequest>();
    }

    private void MapClient()
    {
        CreateMap<ClientDashboardResponse, ClientDashboardViewModel>();
        CreateMap<ClientProfileResponse, ClientProfileViewModel>();
        CreateMap<UpdateClientProfileViewModel, UpdateClientProfileRequest>();
        CreateMap<FavoriteResponse, FavoriteListItemViewModel>();

        CreateMap<PropertyListItemResponse, PropertyListItemViewModel>();
    }

    private void MapAgent()
    {
        CreateMap<PublicAgentResponse, PublicAgentListItemViewModel>();
        CreateMap<PropertyListItemResponse, AgentPropertyListItemViewModel>();
        CreateMap<PropertySummaryResponse, AgentPropertyListItemViewModel>();
        CreateMap<AgentProfileResponse, AgentProfileViewModel>();
        CreateMap<UpdateAgentProfileViewModel, UpdateAgentProfileRequest>();

        CreateMap<CreatePropertyViewModel, CreatePropertyRequest>()
            .ForMember(d => d.ImageFiles, opt => opt.Ignore())
            .ForMember(d => d.ImprovementIds, opt => opt.MapFrom(s => s.ImprovementIds));

        CreateMap<EditPropertyViewModel, UpdatePropertyRequest>()
            .ForMember(d => d.PropertyId, opt => opt.MapFrom(s => s.Id))
            .ForMember(d => d.NewImageFiles, opt => opt.Ignore())
            .ForMember(d => d.ImageIdsToRemove, opt => opt.Ignore())
            .ForMember(d => d.ImprovementIdsToAdd, opt => opt.MapFrom(s => s.ImprovementIds))
            .ForMember(d => d.ImprovementIdsToRemove, opt => opt.Ignore());

        CreateMap<PropertyDetailResponse, AgentPropertyDetailViewModel>();

        CreateMap<SendMessageViewModel, SendMessageRequest>()
            .ForMember(d => d.PropertyId, opt => opt.Ignore())
            .ForMember(d => d.Content, opt => opt.MapFrom(s => s.Message));

        CreateMap<ReplyMessageViewModel, ReplyMessageRequest>()
            .ForMember(d => d.MessageId, opt => opt.Ignore())
            .ForMember(d => d.Content, opt => opt.MapFrom(s => s.Message));
    }

    private void MapAdmin()
    {
        CreateMap<AdminDashboardResponse, AdminDashboardViewModel>();
        CreateMap<AgentListItemResponse, AgentListItemViewModel>();
        CreateMap<AdminListItemResponse, AdminListItemViewModel>();
        CreateMap<DeveloperListItemResponse, DeveloperListItemViewModel>();

        CreateMap<ToggleAgentStatusViewModel, ToggleAgentActiveRequest>();
        CreateMap<DeleteAgentViewModel, DeleteAgentRequest>();
        CreateMap<CreateAdminViewModel, CreateAdminRequest>();
        CreateMap<EditAdminViewModel, UpdateAdminRequest>()
            .ForMember(d => d.AdminId, opt => opt.MapFrom(s => s.Id))
            .ForMember(d => d.ConfirmNewPassword, opt => opt.MapFrom(s => s.ConfirmPassword));
        CreateMap<ToggleAdminStatusViewModel, ToggleAdminActiveRequest>();
        CreateMap<CreateDeveloperViewModel, CreateDeveloperRequest>();
        CreateMap<EditDeveloperViewModel, UpdateDeveloperRequest>()
            .ForMember(d => d.DeveloperId, opt => opt.MapFrom(s => s.Id))
            .ForMember(d => d.ConfirmNewPassword, opt => opt.MapFrom(s => s.ConfirmPassword));
        CreateMap<ToggleDeveloperStatusViewModel, ToggleDeveloperActiveRequest>();
    }

    private void MapCatalog()
    {
        CreateMap<PropertyTypeResponse, PropertyTypeListItemViewModel>();
        CreateMap<CreatePropertyTypeViewModel, CreatePropertyTypeRequest>();
        CreateMap<EditPropertyTypeViewModel, UpdatePropertyTypeRequest>();

        CreateMap<SaleTypeResponse, SaleTypeListItemViewModel>();
        CreateMap<CreateSaleTypeViewModel, CreateSaleTypeRequest>();
        CreateMap<EditSaleTypeViewModel, UpdateSaleTypeRequest>();

        CreateMap<ImprovementResponse, ImprovementListItemViewModel>();
        CreateMap<CreateImprovementViewModel, CreateImprovementRequest>();
        CreateMap<EditImprovementViewModel, UpdateImprovementRequest>();
    }

    private void MapChat()
    {
        CreateMap<ConversationSummaryResponse, ConversationSummaryViewModel>();
        CreateMap<MessageResponse, MessageViewModel>()
            .ForMember(d => d.IsFromCurrentUser, o => o.Ignore());
    }

    private void MapOffers()
    {
        CreateMap<OfferResponse, OfferListItemViewModel>();
        CreateMap<OfferClientSummaryResponse, OfferClientSummaryViewModel>();
        CreateMap<OfferResponse, ViewModels.Offers.OfferDetailViewModel>();
        CreateMap<CreateOfferViewModel, CreateOfferRequest>();
    }

    private void MapShared()
    {
        CreateMap<PropertyTypeResponse, SelectListItemViewModel>()
            .ForMember(d => d.Id, opt => opt.MapFrom(s => s.Id))
            .ForMember(d => d.Name, opt => opt.MapFrom(s => s.Name))
            .ForMember(d => d.Selected, opt => opt.MapFrom(_ => false));

        CreateMap<SaleTypeResponse, SelectListItemViewModel>()
            .ForMember(d => d.Id, opt => opt.MapFrom(s => s.Id))
            .ForMember(d => d.Name, opt => opt.MapFrom(s => s.Name))
            .ForMember(d => d.Selected, opt => opt.MapFrom(_ => false));

        CreateMap<ImprovementResponse, SelectListItemViewModel>()
            .ForMember(d => d.Id, opt => opt.MapFrom(s => s.Id))
            .ForMember(d => d.Name, opt => opt.MapFrom(s => s.Name))
            .ForMember(d => d.Selected, opt => opt.MapFrom(_ => false));
    }
}
