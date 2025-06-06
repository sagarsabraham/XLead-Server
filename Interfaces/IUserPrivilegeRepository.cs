using XLead_Server.DTOs;

namespace XLead_Server.Interfaces
{
    public interface IUserPrivilegeRepository
    {
        Task<IEnumerable<PrivilegeReadDto>> GetPrivilegesByUserIdAsync(long userId);
    }
}
