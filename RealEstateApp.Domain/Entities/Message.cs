using RealEstateApp.Domain.Common;
using RealEstateApp.Domain.Enums;

namespace RealEstateApp.Domain.Entities;

/// <summary>
/// Representa un mensaje enviado entre un cliente y un agente inmobiliario relacionado con una propiedad.
/// </summary>
public class Message : AggregateRoot
{
    public int PropertyId { get; private set; }
    public string ClientId { get; private set; } = null!;
    public string AgentId { get; private set; } = null!;
    public SenderType SenderType { get; private set; }
    public string Content { get; private set; } = null!;

    public Property Property { get; private set; } = null!;

    private Message() { }

    public static Result<Message> Create(
        int propertyId,
        string clientId,
        string agentId,
        SenderType senderType,
        string content
    )
    {
        if (propertyId <= 0)
            return Result.Failure<Message>(
                Error.Validation("Message.PropertyId", "La propiedad es requerida.")
            );
        if (string.IsNullOrWhiteSpace(clientId))
            return Result.Failure<Message>(
                Error.Validation("Message.ClientId", "El cliente es requerido.")
            );
        if (string.IsNullOrWhiteSpace(agentId))
            return Result.Failure<Message>(
                Error.Validation("Message.AgentId", "El agente es requerido.")
            );
        if (string.IsNullOrWhiteSpace(content?.Trim()))
            return Result.Failure<Message>(
                Error.Validation("Message.Content", "El mensaje no puede estar vacío.")
            );
        if (!Enum.IsDefined(senderType))
            return Result.Failure<Message>(
                Error.Validation("Message.SenderType", "El tipo de remitente no es válido.")
            );

        var msg = new Message
        {
            PropertyId = propertyId,
            ClientId = clientId,
            AgentId = agentId,
            SenderType = senderType,
            Content = content.Trim(),
        };

        return Result.Success(msg);
    }
}
