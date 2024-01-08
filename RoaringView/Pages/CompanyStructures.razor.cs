using Microsoft.AspNetCore.Mvc;
using RoaringView.Data;
using RoaringView.Model;


//kan tas bort, men kan vara bra att spara tills man fixar Company Hierarchy chart
// Data tabell för company structure
[ApiController]
[Route("api/[controller]")]
public class CompanyStructureController : ControllerBase
{
    private readonly CompanyStructureService _companystructureservice;

    public CompanyStructureController(CompanyStructureService service)
    {
        _companystructureservice = service;
    }

    [HttpGet]
    public async Task<ActionResult<List<CompanyStructure>>> Get()
    {
        try
        {
            var companyStructures = await _companystructureservice.GetCompanyStructuresAsync();
            return Ok(companyStructures);
        }
        catch (System.Exception ex)
        {
            // Log the exception details here
            return StatusCode(500, "Internal Server Error: " + ex.Message);
        }
    }
}
