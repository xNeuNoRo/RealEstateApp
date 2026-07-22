using RealEstateApp.Application.Dtos.Agent.Requests;
using RealEstateApp.Application.Dtos.Agent.Responses;
using RealEstateApp.Application.Dtos.Chat.Requests;
using RealEstateApp.Application.Dtos.Chat.Responses;
using RealEstateApp.Application.Dtos.Offers.Requests;
using RealEstateApp.Application.Dtos.Offers.Responses;
using RealEstateApp.Domain.Common;

namespace RealEstateApp.Application.Interfaces.Services;

public interface IAgentService
{
    Task<Result<AgentProfileResponse>> GetProfileAsync(CancellationToken ct = default);
    Task<Result<AgentProfileResponse>> UpdateProfileAsync(
        UpdateAgentProfileRequest request,
        CancellationToken ct = default
    );
    Task<Result<PagedResult<ConversationSummaryResponse>>> GetChatListAsync(
        GetMyConversationsRequest request,
        CancellationToken ct = default
    );
    Task<Result<PagedResult<MessageResponse>>> GetConversationAsync(
        GetConversationRequest request,
        CancellationToken ct = default
    );
    Task<Result<MessageResponse>> ReplyMessageAsync(
        ReplyMessageRequest request,
        CancellationToken ct = default
    );
    Task<Result<PagedResult<OfferResponse>>> GetPropertyOffersAsync(
        GetPropertyOffersRequest request,
        CancellationToken ct = default
    );
    Task<Result<PagedResult<OfferClientSummaryResponse>>> GetPropertyOfferClientsAsync(
        GetPropertyOfferClientsRequest request,
        CancellationToken ct = default
    );
    Task<Result<OfferResponse>> AcceptOfferAsync(int offerId, CancellationToken ct = default);
    Task<Result> RejectOfferAsync(int offerId, CancellationToken ct = default);
}
