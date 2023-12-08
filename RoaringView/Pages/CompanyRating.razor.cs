using Microsoft.AspNetCore.Mvc;
using RoaringView.Data;
using RoaringView.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

[ApiController]
[Route("api/[controller]")]
public class CompanyRatingsController : ControllerBase
{
    private readonly CompanyRatingService _companyratingservice;

    public CompanyRatingsController(CompanyRatingService service)
    {
        _companyratingservice = service;
    }

    [HttpGet]
    public async Task<ActionResult<List<CompanyRating>>> Get()
    {
        try
        {
            var companyRatings = await _companyratingservice.GetCompanyRatingsAsync();
            return Ok(companyRatings);
        }
        catch (System.Exception ex)
        {
            return StatusCode(500, "Internal Server Error: " + ex.Message);
        }
    }
}