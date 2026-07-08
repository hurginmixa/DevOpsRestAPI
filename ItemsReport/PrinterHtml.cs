using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Web;
using CommonCode;
using CommonCode.DocumentClasses;

namespace ItemsReport
{
    public static class PrinterHtml
    {
        public static string PrintToString(IDocumentWorkItemList workItemList, Config config)
        {
            StringBuilder sb = new StringBuilder();
            using TextWriter textWriter = new StringWriter(sb);

            textWriter.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
            textWriter.WriteLine(@"<!DOCTYPE html PUBLIC ""-//W3C//DTD XHTML 1.0 Strict//EN"" ""http://www.w3.org/TR/xhtml1/DTD/xhtml1-strict.dtd"">");
            textWriter.WriteLine("<html>");
            textWriter.WriteLine("<head>");
            textWriter.WriteLine($"<title>{config.HTMLTitle}</title>");
            textWriter.WriteLine(GetStyles());
            textWriter.WriteLine($"<script src='ItemReportScript.js?v={System.DateTime.Now.Ticks}'></script>");
            textWriter.WriteLine("</head>");
            textWriter.WriteLine("<body ondblclick='onDocumentClick(event)'>");
            textWriter.WriteLine("&nbsp;&nbsp;&nbsp;<button onclick='OnCollapseAll()' class='favorite styled'>Collapse All</button><br /><br />");
            textWriter.WriteLine("<table>");

            string[] reportedPaths = workItemList.GetUniqueCommittedPaths()
                .Select(s =>
                {
                    int prior;
                    switch (s)
                    {
                        case "ver/10.0/2024/04/rel":
                            prior = 0;
                            break;
                        case "ver/11.0/2025/06/rel":
                            prior = 1;
                            break;
                        default:
                        {
                            prior = s.StartsWith("ver") ? 2 : 3;
                            break;
                        }
                    }
                    return new {S = s, Prior = prior};
                })
                .OrderBy(s => s.Prior).ThenBy(s => s.S)
                .Select(s => s.S)
                .ToArray();

            // Отдаём клиенту текущий набор колонок — TS шлёт его обратно при
            // перерисовке (POST /refresh/{id}/rows), чтобы выровнять <td>.
            textWriter.WriteLine($"<script>var reportedPaths = {JsonSerializer.Serialize(reportedPaths)};</script>");

            textWriter.WriteLine("<thead>");
            textWriter.WriteLine("<tr>");
            textWriter.WriteLine("<th class='sticky-column, sticky-row'>Id</th>");
            textWriter.WriteLine("<th class='sticky-column, sticky-row'>Title</th>");
            textWriter.WriteLine("<th class='sticky-column, sticky-row'>State</th>");
            textWriter.WriteLine("<th class='sticky-column, sticky-row'>Assign To</th>");

            foreach (string path in reportedPaths)
            {
                textWriter.WriteLine($"<th class='sticky-column, sticky-row'>{HttpUtility.HtmlAttributeEncode(path)}</th>");
            }

            textWriter.WriteLine("</tr>");
            textWriter.WriteLine("</thead>");

            Color[] colors = {Color.Aquamarine, Color.MistyRose, Color.LightSkyBlue, Color.DarkSeaGreen, Color.LightPink };
            int colorIndex = -1;

            // Каждый item 1-го уровня получает следующий цвет палитры по кругу.
            // colorIndex общий на обе секции — чтобы вывод не менялся. Сами строки
            // рисует переиспользуемый RenderRows.
            void RenderSection(IEnumerable<IDocumentWorkItem> items)
            {
                foreach (IDocumentWorkItem workItem in items)
                {
                    colorIndex = (colorIndex + 1) % colors.Length;
                    textWriter.Write(RenderRows(new[] { workItem }, reportedPaths, ColorTranslator.ToHtml(colors[colorIndex]), 0, 0));
                }
            }

            textWriter.WriteLine("<tr>");
            textWriter.WriteLine($"<td colspan='{reportedPaths.Length + 4}'><h1>Not completed items</h1></td>");
            textWriter.WriteLine("</tr>");

            RenderSection(workItemList.Where(r => r.HasActiveSubItems));

            textWriter.WriteLine("<tr>");
            textWriter.WriteLine($"<td colspan='{reportedPaths.Length + 4}'><h1>Completed items</h1></td>");
            textWriter.WriteLine("</tr>");

            RenderSection(workItemList.Where(r => !r.HasActiveSubItems));

            textWriter.WriteLine("</table>");
            textWriter.WriteLine("</body>");
            textWriter.WriteLine("</html>");

            return sb.ToString();
        }

        private static string RenderPullRequestCells(string[] reportedPaths, IEnumerable<(DocumentPullRequest Request, bool IsOwner)> pullRequestList)
        {
            if (reportedPaths.Length <= 0)
            {
                return string.Empty;
            }

            (DocumentPullRequest Request, bool IsOwner)[] l = pullRequestList.OrderBy(r => r.Request.Id).ToArray();

            Dictionary<string, StringBuilder> builders = reportedPaths.ToDictionary(i => i, i => new StringBuilder());

            foreach ((DocumentPullRequest Request, bool IsOwner) tuple in l)
            {
                foreach (string path in reportedPaths)
                {
                    string linkText = tuple.Request.TargetRefName == path ? GetLinkText(tuple) : "&nbsp;";

                    builders[path].Append($"{linkText}<br>");
                }
            }

            StringBuilder sb2 = new StringBuilder();

            foreach (string path in reportedPaths)
            {
                sb2.Append("<td>");

                sb2.Append(builders[path].ToString());

                sb2.Append("</td>");
            }

            return sb2.ToString();
        }

        // Рисует переданные item'ы вместе с их поддеревьями. Цвет фона задаётся
        // снаружи и наследуется вниз по уровням (ротацию цвета 1-го уровня делает
        // вызывающий код). Используется и полным отчётом, и перерисовкой поддерева.
        public static string RenderRows(IEnumerable<IDocumentWorkItem> items, string[] reportedPaths, string colorHtml, int levelNumber, int parentItemId)
        {
            StringBuilder sb = new StringBuilder();
            using TextWriter textWriter = new StringWriter(sb);

            foreach (IDocumentWorkItem workItem in items)
            {
                (DocumentPullRequest Request, bool IsOwner)[] pullRequestList = workItem.GetFullPullRequestList().Where(re => reportedPaths.Contains(re.Request.TargetRefName)).ToArray();

                string style = $"background-color:{colorHtml};";
                if (pullRequestList.Length == 0 && !(workItem.IsClosed || workItem.IsResolved))
                {
                    style += " font-weight: bold;";
                }

                textWriter.WriteLine($"<tr style='{style}' id='{workItem.Id}' class='childOf_{parentItemId}'>");

                string markSpan = "\u25A2";
                if (workItem.SubItems.Any())
                {
                    markSpan = $"<span id='mark' onclick='OnMarkClick(this, {workItem.Id})' style='cursor: pointer' >\u25e2</span>";
                }

                string lineShift = "&nbsp;";
                if (levelNumber != 0)
                {
                    lineShift = string.Concat(Enumerable.Repeat("&nbsp;", levelNumber * 5));

                    lineShift += "&nbsp;&nbsp;";
                }

                string subItemsCount = workItem.SubItems.Any() ? $"&nbsp;(&nbsp;{workItem.SubItems.Count()}&nbsp;)" : string.Empty;

                // Иконка перерисовки — только для item'ов 1-го уровня.
                string refreshIcon = levelNumber == 0
                    ? $"&nbsp;<span onclick='OnRefreshClick({workItem.Id})' style='cursor: pointer' title='Reload from Azure'>↻</span>"
                    : string.Empty;

                textWriter.Write($"<td style='white-space: nowrap'><code>{lineShift}{markSpan}</code>&nbsp;<a href='{workItem.Html}' target='_blank'>{workItem.Id}</a>{subItemsCount}{refreshIcon}</td>");

                // ------------ workItemTitle

                Color folderColor = GetFolderColor(workItem);
                string folder = $"<span style='color: {ColorTranslator.ToHtml(folderColor)};'>&#128447;</span>&nbsp;";

                string workItemTitle = workItem.Title;
                if (workItem.IsClosed && pullRequestList.Length == 0)
                {
                    workItemTitle = $"<S>{workItemTitle}</S>";
                }
                string workItemText = $"{folder}<b>{workItem.WorkItemType}</b>&nbsp;:&nbsp;{workItemTitle}";

                textWriter.Write($"<td>{workItemText}</td>");

                // ------------ state
                string workItemState = workItem.State;
                if (workItem.HasActiveSubItems)
                {
                    workItemState += "&nbsp;(HAS)";
                }
                textWriter.Write($"<td>{workItemState}</td>");

                textWriter.Write($"<td>{workItem.AssignedTo}</td>");

                textWriter.Write(RenderPullRequestCells(reportedPaths, pullRequestList));

                textWriter.WriteLine("</tr>");

                textWriter.Write(RenderRows(workItem.SubItems, reportedPaths, colorHtml, levelNumber + 1, workItem.Id));
            }

            return sb.ToString();
        }

        private static Color GetFolderColor(IDocumentWorkItem workItem)
        {
            switch (workItem.WorkItemType)
            {
                case "Bug" : return Color.Red;
                case "Task" : return Color.Yellow;
                case "Task-Validation" : return Color.GreenYellow;
                case "Feature" : return Color.BlueViolet;
                case "Requirement" : return Color.DodgerBlue;
                case "Issue" : return Color.DarkRed;

                default: return Color.Black;
            }
        }

        private static string GetLinkText((DocumentPullRequest request, bool owner) pullRequest)
        {
            string date = $"{pullRequest.request.CloseDate:yyyy/MM/dd HH:mm}";

            string title = $"&ldquo;{pullRequest.request.TargetRefName}&rdquo;&nbsp;{date}&nbsp;{pullRequest.request.CreateBy}&nbsp;{pullRequest.request.Status}";

            string linkText = $"<span title='{title}'>{pullRequest.request.Id}</span>";

            if (pullRequest.request.Status == "active")
            {
                linkText = $"<B>{linkText}</B>";
            }
            else if (pullRequest.request.Status == "abandoned")
            {
                linkText = $"<S>{linkText}</S>";
            }

            linkText =
                $"<a href='https://dev.azure.com/AzCamtek/GIT/_git/CamtekGit/pullrequest/{pullRequest.request.Id}' target='_blank'>{linkText}</a>"; // 😪😪😪

            if (pullRequest.owner)
            {
                linkText += "*";
            }

            return linkText;
        }

        private static string GetStyles()
        {
            return @"
<style type='text/css'>
  table {
    width: 100%;
    border-collapse: collapse;
  }
  
  th, td {
    padding: 8px;
    border: 1px solid #ddd;
  }

  .sticky-column {
    position: sticky;
    left: 0;
    z-index: 1;
    background-color: #f1f1f1;
  }
  
  .sticky-row {
    position: sticky;
    background-color: #f1f1f1;
    z-index: 2;
    top: 0;
  }
  
  .styled {
    border: 0;
    line-height: 2.5;
    padding: 0 20px;
    font-size: 1rem;
    text-align: center;
    color: #fff;
    text-shadow: 1px 1px 1px #000;
    border-radius: 10px;
    background-color: rgba(220, 0, 0, 1);
    background-image: linear-gradient(to top left, rgba(0, 0, 0, 0.2), rgba(0, 0, 0, 0.2) 30%, rgba(0, 0, 0, 0));
    box-shadow:
      inset 2px 2px 3px rgba(255, 255, 255, 0.6),
      inset -2px -2px 3px rgba(0, 0, 0, 0.6);
  }

  .styled:hover {
    background-color: rgba(255, 0, 0, 1);
  }

  .styled:active {
    box-shadow:
      inset -2px -2px 3px rgba(255, 255, 255, 0.6),
      inset 2px 2px 3px rgba(0, 0, 0, 0.6);
  }

</style>
";
        }
    }
}
