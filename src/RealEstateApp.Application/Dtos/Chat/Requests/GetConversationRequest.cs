namespace RealEstateApp.Application.Dtos.Chat.Requests;

public sealed record GetConversationRequest(int PropertyId, int Page = 1, int PageSize = 20);
