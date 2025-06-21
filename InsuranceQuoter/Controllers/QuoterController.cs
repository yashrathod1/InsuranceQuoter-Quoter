using InsuranceQuoter_Service.CompanyProduct;
using InsuranceQuoter_Service.Exceptions;
using InsuranceQuoter_Service.Interface;
using InsuranceQuoter_Service.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace InsuranceQuoter.Controllers;

[ApiController]
[Route("api/[controller]")]
public class QuoterController : Controller
{
    private readonly IQuoterService _quoterService;

    public QuoterController(IQuoterService quoterService)
    {
        _quoterService = quoterService;
    }

    [HttpPost("GenerateCSV")]
    public async Task<IActionResult> GenerateCsv(IFormFile file, string companyName)
    {
        ProductInfoBase product = ProductInfoFactory.GetProductInfo(companyName);
        string servicePath = Path.Combine(AppContext.BaseDirectory, @"..\..\..\..\InsuranceQuoter_Service");
        string companyFolder = Path.Combine(servicePath, "company", companyName.ToLower());

        Directory.CreateDirectory(companyFolder);
        await product.GenerateCsvFilesAsync(file, companyFolder);

        return Ok($"{companyName.ToUpper()} CSV files generated successfully.");
    }

    [HttpPost("GetQuote")]
    public async Task<IActionResult> GetQuoteAsync([FromBody] UserInputViewModel input)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            QuoteResultVIewModel? result = await _quoterService.GetQuoteAsync(input);

            if (result == null)
                return NotFound("No valid quote found for the given inputs.");
            return Ok(result);
        }
        catch (ValidationErrorsException vex)
        {
            return BadRequest(new { errors = vex.Errors });
        }
    }


}
