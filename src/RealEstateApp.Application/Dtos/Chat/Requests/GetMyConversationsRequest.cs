namespace RealEstateApp.Application.Dtos.Chat.Requests;

public sealed record GetMyConversationsRequest(int Page = 1, int PageSize = 20);
