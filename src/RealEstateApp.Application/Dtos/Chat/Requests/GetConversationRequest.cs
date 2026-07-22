namespace RealEstateApp.Application.Dtos.Chat.Requests;

public sealed record GetConversationRequest(
    int PropertyId,
    string? ClientId = null,
    int Page = 1,
    int PageSize = 20
);
