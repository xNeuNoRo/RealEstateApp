using RealEstateApp.Application.Common.Interfaces;
using RealEstateApp.Application.Dtos.Chat.Requests;
using RealEstateApp.Application.Dtos.Chat.Responses;

namespace RealEstateApp.Application.Interfaces.UseCases.Chat;

public interface IReplyMessageUseCase : IUseCase<ReplyMessageRequest, MessageResponse>;
