using AutoMapper;
using Microsoft.EntityFrameworkCore;
using XLead_Server.Data;
using XLead_Server.DTOs;
using XLead_Server.Interfaces;
using XLead_Server.Models;

namespace XLead_Server.Repositories
{
    public class CompanyRepository : ICompanyRepository
    {
        private readonly ApiDbContext _context;
        private readonly IMapper _mapper;

        public CompanyRepository(ApiDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }
        public async Task<IEnumerable<CompanyReadDto>> GetAllCompaniesAsync()
        {
            var companies = await _context.Companies.ToListAsync();
            return _mapper.Map<IEnumerable<CompanyReadDto>>(companies);
        }

        public async Task<Company> AddCompanyAsync(CompanyCreateDto dto)
        {
            var company = _mapper.Map<Company>(dto);
            company.IsActive = true;
            company.CreatedBy = 1;
            company.UpdatedBy = 1;
            company.UpdatedAt = DateTime.UtcNow;

            _context.Companies.Add(company);
            await _context.SaveChangesAsync();
            return company;
        }

        public async Task<Dictionary<string, List<string>>> GetCompanyContactMapAsync()
        {
            var companies = await _context.Companies
                .Include(c => c.Contacts)
                .ToListAsync();

            return companies.ToDictionary(
                c => c.CompanyName,
                c => c.Contacts.Select(ct => $"{ct.FirstName} {ct.LastName}".Trim()).ToList()
            );
        }


    }
}