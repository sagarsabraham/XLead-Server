using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using XLead_Server.Interfaces;

namespace XLead_Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CountryController : ControllerBase
    {
        private ICountryRepository _countryRepository;
        public CountryController(ICountryRepository countryRepository)
        {
            _countryRepository = countryRepository;
        }

        [HttpGet]

        public async Task<IActionResult> Get()
        {
            var countries = await _countryRepository.GetCountries();
            if (countries == null)
            {
                return NotFound();
            }
            return Ok(countries);
        }

    }
}
