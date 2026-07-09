using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using CommonCode.DocumentClasses;
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

        // POST /refresh/{id}/rows
        // Возвращает готовые <tr> поддерева (через общий RenderRows), выровненные
        // под колонки, которые прислал клиент. Цвет строки тоже задаёт клиент.
        [HttpPost("{id:int}/rows")]
        public async Task<IActionResult> RefreshRows(int id, [FromBody] RefreshRowsRequest request)
        {
            IDocumentWorkItem item = await _reportService.ReadItemAsync(id);

            if (item == null)
            {
                return NotFound();
            }

            string[] columns = request?.Columns ?? Array.Empty<string>();
            string color = string.IsNullOrEmpty(request?.Color) ? "white" : request.Color;

            string html = PrinterHtml.RenderRows(new[] { item }, columns, color, 0, 0);

            // Триггеры полной перезагрузки на клиенте: если в поддереве появился
            // PR в ветку вне текущих колонок — нужна новая колонка (её знает только
            // полный рендер); если сменилась секция Completed <-> Not completed —
            // строку надо перенести. Оба случая частичный свап отразить не может,
            // поэтому отдаём клиенту факты, а он решает про location.reload().
            string[] branches = item.GetFullPullRequestList()
                .Select(pr => pr.Request.TargetRefName)
                .Distinct()
                .ToArray();

            Response.Headers["X-Branches"] = JsonSerializer.Serialize(branches);
            Response.Headers["X-Active"] = item.HasActiveSubItems ? "true" : "false";

            return Content(html, "text/html");
        }
    }

    public class RefreshRowsRequest
    {
        public string[] Columns { get; set; }

        public string Color { get; set; }
    }
}
