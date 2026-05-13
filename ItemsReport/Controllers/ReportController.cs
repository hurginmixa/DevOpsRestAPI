using Microsoft.AspNetCore.Mvc;
using ItemsReport.Services;
using System;
using System.Threading.Tasks;

namespace ItemsReport.Controllers
{
    [ApiController]
    [Route("")]
    public class ReportController : ControllerBase
    {
        private readonly ReportService _reportService;

        public ReportController(ReportService reportService)
        {
            _reportService = reportService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            try
            {
                string html = await _reportService.GenerateReportAsync();
                return Content(html, "text/html");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error generating report: {ex}");
            }
        }
    }
}
