using ABBELForgeService.Domain.Services.Class;
using Microsoft.AspNetCore.Mvc;

namespace ABBELForgeService.API.Controllers;

public class MeasureSquareController : Controller
{
    private readonly IMeasureSquareService _measureSquareService;
    
    public MeasureSquareController(IMeasureSquareService measureSquareService)
    {
        _measureSquareService = measureSquareService;
    }
    [HttpPost("upload")]
    public async Task<IActionResult> Post(IFormFile file, int pixelLength)
    {
        var bytes = await file.GetBytes();
        var result = await _measureSquareService.GetData(pixelLength, file.FileName, bytes);

        return Ok(result);
    }
}