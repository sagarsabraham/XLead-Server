using AutoMapper;
using Microsoft.EntityFrameworkCore;
using XLead_Server.Data;
using XLead_Server.DTOs;
using XLead_Server.Interfaces;

namespace XLead_Server.Repositories
{
    public class UserPrivilegeRepository :IUserPrivilegeRepository
    {
        private readonly ApiDbContext _context;
        private readonly IMapper _mapper;

        public UserPrivilegeRepository(ApiDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<IEnumerable<PrivilegeReadDto>> GetPrivilegesByUserIdAsync(long userId)
        {
            var privileges = await _context.UsersPrivileges
                .Where(up => up.UserId == userId)
                .Select(up => up.Privilege)
                .ToListAsync();

            return _mapper.Map<IEnumerable<PrivilegeReadDto>>(privileges);
        }
    }
}
