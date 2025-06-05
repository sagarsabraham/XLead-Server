// Repositories/UserRepository.cs
//using AutoMapper;
//using Microsoft.EntityFrameworkCore;
//using XLead_Server.Data;
//using XLead_Server.DTOs;
//using XLead_Server.Interfaces;
//using XLead_Server.Models;

//namespace XLead_Server.Repositories
//{
//    public class UserRepository : IUserRepository
//    {
//        private readonly ApiDbContext _context;
//        private readonly IMapper _mapper;

//        public UserRepository(ApiDbContext context, IMapper mapper)
//        {
//            _context = context;
//            _mapper = mapper;
//        }

//        public async Task<IEnumerable<UserReadDto>> GetAllUsersAsync()
//        {
//            var users = await _context.Users
//                .Include(u => u.CreatedByUser)
//                .ToListAsync();

//            return _mapper.Map<IEnumerable<UserReadDto>>(users);
//        }

//        public async Task<UserReadDto> GetUserByIdAsync(long id)
//        {
//            var user = await _context.Users
//                .Include(u => u.CreatedByUser)
//                .FirstOrDefaultAsync(u => u.Id == id);

//            if (user == null)
//                return null;

//            return _mapper.Map<UserReadDto>(user);
//        }

//        public async Task<User> CreateUserAsync(UserCreateDto userCreateDto)
//        {
//            var user = _mapper.Map<User>(userCreateDto);
//            user.CreatedAt = DateTime.Now;

//            await _context.Users.AddAsync(user);
//            await _context.SaveChangesAsync();

//            return user;
//        }

//        public async Task<User> UpdateUserAsync(long id, UserCreateDto userUpdateDto)
//        {
//            var user = await _context.Users.FindAsync(id);
//            if (user == null)
//                return null;

//            _mapper.Map(userUpdateDto, user);

//            _context.Users.Update(user);
//            await _context.SaveChangesAsync();

//            return user;
//        }

//        public async Task DeleteUserAsync(long id)
//        {
//            var user = await _context.Users.FindAsync(id);
//            if (user != null)
//            {
//                _context.Users.Remove(user);
//                await _context.SaveChangesAsync();
//            }
//        }

//        public async Task<string> GetUserNameByIdAsync(long id)
//        {
//            var user = await _context.Users.FindAsync(id);
//            return user?.Name ?? "Unknown User";
//        }
//    }
//}