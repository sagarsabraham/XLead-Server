// XLead_Server/Repositories/ContactRepository.cs
using AutoMapper; // Keep if used by GetAllContactsAsync or other methods
using Microsoft.AspNetCore.Mvc; // Not typically needed in repository
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic; // For IEnumerable
using System.Linq; // For LINQ methods if any
using System.Threading.Tasks; // For Task
using XLead_Server.Data;
using XLead_Server.DTOs;
using XLead_Server.Interfaces;
using XLead_Server.Models;

namespace XLead_Server.Repositories
{
    public class ContactRepository : IContactRepository
    {
        private readonly ApiDbContext _context;
        private readonly IMapper _mapper; // IMapper is used for GetAllContactsAsync

        public ContactRepository(ApiDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper; // Store mapper if used elsewhere
        }

        // **** ADDED MISSING IMPLEMENTATION ****
        public async Task<Contact> AddContactAsync(ContactCreateDto dto)
        {
            // You might want to use AutoMapper here as well if you have a mapping configured
            // from ContactCreateDto to Contact. For now, manual mapping:
            var contact = new Contact
            {
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Email = dto.Email,
                PhoneNumber = dto.PhoneNumber,
                CompanyId = dto.CompanyId, // Ensure CompanyId is correctly set in the DTO
                CreatedBy = dto.CreatedBy,
                IsActive = true, // Default to active, or get from DTO if available
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Contacts.Add(contact);
            await _context.SaveChangesAsync();
            return contact; // Return the created Contact entity (with its generated ID)
        }

        public async Task<IEnumerable<Contact>> GetAllContacts() // This method was in your original interface
        {
            return await _context.Contacts.ToListAsync();
        }

        public async Task<Contact?> GetByFullNameAndCompanyIdAsync(string firstName, string? lastName, long companyId)
        {
            // Normalize lastName for query if it can be null or empty
            var normalizedLastName = string.IsNullOrWhiteSpace(lastName) ? null : lastName;
            var lowerFirstName = firstName.ToLower(); // Case-insensitive search for first name

            if (normalizedLastName == null)
            {
                // Search for contacts where LastName is null or empty
                return await _context.Contacts
                    .FirstOrDefaultAsync(c => c.FirstName.ToLower() == lowerFirstName &&
                                              (c.LastName == null || c.LastName == "") &&
                                              c.CompanyId == companyId);
            }
            else
            {
                // Search for contacts with a specific last name
                var lowerLastName = normalizedLastName.ToLower(); // Case-insensitive search for last name
                return await _context.Contacts
                    .FirstOrDefaultAsync(c => c.FirstName.ToLower() == lowerFirstName &&
                                              c.LastName != null && c.LastName.ToLower() == lowerLastName &&
                                              c.CompanyId == companyId);
            }
        }

        public async Task<IEnumerable<ContactReadDto>> GetAllContactsAsync()
        {
            var contacts = await _context.Contacts
                                         .Include(c => c.Company) // Include Company for CompanyName
                                         .ToListAsync();
            // This mapping relies on AutoMapper profiles being set up correctly.
            // Specifically, how Contact.Company.CompanyName maps to ContactReadDto.CompanyName
            // and how Contact.FirstName + Contact.LastName maps to ContactReadDto.FullName.
            return _mapper.Map<IEnumerable<ContactReadDto>>(contacts);
        }
    }
}