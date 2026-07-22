namespace RealEstateApp.Application.Dtos.Chat.Requests;

public sealed record SendMessageRequest(int PropertyId, string Content);
