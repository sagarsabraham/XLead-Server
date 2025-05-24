using Microsoft.AspNetCore.Mvc;
using XLead_Server.Models;
using XLead_Server.DTOs;
using XLead_Server.Interfaces;

[ApiController]
[Route("api/[controller]")]
public class DealsController : ControllerBase
{
    private readonly IDealRepository _dealRepository;

    public DealsController(IDealRepository dealRepository)
    {
        _dealRepository = dealRepository;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<DealReadDto>> GetDealById(long id)
    {
        var dealDto = await _dealRepository.GetDealByIdAsync(id);
        if (dealDto == null)
            return NotFound();
        return Ok(dealDto);
    }

    [HttpPost]
    public async Task<ActionResult<DealReadDto>> CreateDeal([FromBody] DealCreateDto dto)
    {
        var deal = new Deal
        {
            DealName = dto.DealName,
            DealAmount = dto.DealAmount,
            AccountId = dto.AccountId,
            RegionId = dto.RegionId,
            DomainId = dto.DomainId,
            RevenueTypeId = dto.RevenueTypeId,
            DuId = dto.DuId,
            CountryId = dto.CountryId,
            Description = dto.Description,
            Probability = dto.Probability,
            ContactId = dto.ContactId,
            StartingDate = dto.StartingDate,
            ClosingDate = dto.ClosingDate,
            CreatedBy = dto.CreatedBy,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
            // DealStages can be added here if needed
        };

        await _dealRepository.AddDealAsync(deal);

        // Fetch the full DTO with related data
        var result = await _dealRepository.GetDealByIdAsync(deal.Id);

        return CreatedAtAction(nameof(GetDealById), new { id = deal.Id }, result);
    }
}
