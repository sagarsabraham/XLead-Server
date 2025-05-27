// Interfaces/IUserRepository.cs
using XLead_Server.DTOs;
using XLead_Server.Models;

namespace XLead_Server.Interfaces
{
    public interface IUserRepository
    {
        Task<IEnumerable<UserReadDto>> GetAllUsersAsync();
        Task<UserReadDto> GetUserByIdAsync(long id);
        Task<User> CreateUserAsync(UserCreateDto userCreateDto);
        Task<User> UpdateUserAsync(long id, UserCreateDto userUpdateDto);
        Task DeleteUserAsync(long id);
        Task<string> GetUserNameByIdAsync(long id);
    }
}