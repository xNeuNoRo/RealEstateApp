namespace RealEstateApp.Application.Dtos.Client.Responses;

public sealed record ClientDashboardResponse(
    int FavoritesCount,
    int TotalOffers,
    int ActiveOffers,
    int ConversationsCount,
    int TotalMessages
);
