﻿//using Microsoft.AspNetCore.Mvc;
//using RoaringView.Data;
//using RoaringView.Model;
//using System.Collections.Generic;
//using System.Threading.Tasks;

//[ApiController]
//[Route("api/[controller]")]
//public class CompanyStructureController : ControllerBase
//{
//    private readonly CompanyStructureService _companystructureservice;

//    public CompanyStructureController(CompanyStructureService service)
//    {
//        _companystructureservice = service;
//    }

//    [HttpGet]
//    public async Task<ActionResult<List<CompanyStructure>>> Get()
//    {
//        try
//        {
//            var companyStructures = await _companystructureservice.GetCompanyStructuresAsync();
//            return Ok(companyStructures);
//        }
//        catch (System.Exception ex)
//        {
//            // Log the exception details here
//            return StatusCode(500, "Internal Server Error: " + ex.Message);
//        }
//    }
//}
