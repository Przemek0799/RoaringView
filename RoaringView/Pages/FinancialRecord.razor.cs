using Microsoft.AspNetCore.Mvc;
using RoaringView.Data;
using RoaringView.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

[ApiController]
[Route("api/[controller]")]
public class FinancialRecordController : ControllerBase
{
    private readonly FinancialRecordService _financialrecordservice;

    public FinancialRecordController(FinancialRecordService service)
    {
        _financialrecordservice = service;
    }

    [HttpGet]
    public async Task<ActionResult<List<FinancialRecord>>> Get()
    {
        try
        {
            var financialRecords = await _financialrecordservice.GetFinancialRecordsAsync();
            return Ok(financialRecords);
        }
        catch (System.Exception ex)
        {
            return StatusCode(500, "Internal Server Error: " + ex.Message);
        }
    }
}
