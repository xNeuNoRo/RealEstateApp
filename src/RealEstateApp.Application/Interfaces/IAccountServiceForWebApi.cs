using RealEstateApp.Application.Dtos.Auth;

namespace RealEstateApp.Application.Interfaces;

public interface IAccountServiceForWebApi
{
    Task<LoginResponseDto> AuthenticateAsync(LoginDto login);
    Task<RegisterResponseDto> RegisterUserAsync(RegisterUserDto register);
    Task<List<UserDto>> GetAllUsersAsync();
    Task<UserDto?> GetUserByIdAsync(string id);
    Task<UserDto?> GetUserByEmailAsync(string email);
    Task<UserDto?> GetUserByUserNameAsync(string userName);
    Task<bool> DeleteUserAsync(string id);
    Task<UserDto?> EditUserAsync(EditUserDto dto);
    Task<bool> ActivateUserAsync(string id);
    Task<bool> DeactivateUserAsync(string id);
}
