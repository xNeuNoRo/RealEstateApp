namespace RealEstateApp.Domain.Common;

public static class ErrorCodes
{
    public const string ValidationError = "VALIDATION_ERROR";
    public const string NotFound = "NOT_FOUND";
    public const string Unauthorized = "UNAUTHORIZED";
    public const string Forbidden = "FORBIDDEN";
    public const string Conflict = "CONFLICT";
    public const string InternalServerError = "INTERNAL_SERVER_ERROR";

    public const string PropertyNotFound = "PROPERTY_NOT_FOUND";
    public const string PropertyNotOwner = "PROPERTY_NOT_OWNER";
    public const string AgentNotFound = "AGENT_NOT_FOUND";
    public const string ClientNotFound = "CLIENT_NOT_FOUND";
    public const string ImprovementInUse = "IMPROVEMENT_IN_USE";
    public const string SaleTypeDuplicateCode = "SALE_TYPE_DUPLICATE_CODE";
    public const string PropertyTypeDuplicateName = "PROPERTY_TYPE_DUPLICATE_NAME";
    public const string UserNotFound = "USER_NOT_FOUND";
    public const string InvalidCredentials = "INVALID_CREDENTIALS";
    public const string EmailNotConfirmed = "EMAIL_NOT_CONFIRMED";
    public const string ResourceConflict = "RESOURCE_CONFLICT";
}
