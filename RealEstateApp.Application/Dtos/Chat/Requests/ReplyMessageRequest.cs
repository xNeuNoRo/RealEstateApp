namespace RealEstateApp.Application.Dtos.Chat.Requests;

public sealed record ReplyMessageRequest(int MessageId, string Content);
