using RealEstateApp.Application.Common.Interfaces;
using RealEstateApp.Application.Dtos.Chat.Requests;
using RealEstateApp.Application.Dtos.Chat.Responses;
using RealEstateApp.Domain.Common;

namespace RealEstateApp.Application.Interfaces.UseCases.Chat;

public interface IGetMyConversationsUseCase
    : IUseCase<GetMyConversationsRequest, PagedResult<ConversationSummaryResponse>>;
