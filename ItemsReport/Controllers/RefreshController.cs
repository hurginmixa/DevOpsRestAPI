using System.Threading.Tasks;
using CommonCode.DocumentClasses.SerializeClasses;
using ItemsReport.Services;
using Microsoft.AspNetCore.Mvc;

namespace ItemsReport.Controllers
{
    [ApiController]
    [Route("refresh")]
    public class RefreshController : ControllerBase
    {
        private readonly ReportService _reportService;

        public RefreshController(ReportService reportService)
        {
            _reportService = reportService;
        }

        // POST /refresh/{id}
        // Возвращает поддерево одного item'а как JSON (DocumentWorkItemData).
        // Клиент сам решает, как встроить это в таблицу.
        [HttpPost("{id:int}")]
        public async Task<IActionResult> Refresh(int id)
        {
            DocumentWorkItemData data = await _reportService.RefreshItemAsync(id);

            if (data == null)
            {
                return NotFound();
            }

            return new JsonResult(data);
        }
    }
}
