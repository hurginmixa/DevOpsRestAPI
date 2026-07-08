using System;
using System.Collections.Generic;
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
        // Палитра «рельсов» слева у групп 1-го уровня (по кругу).
        private static readonly string[] Rails = { "#4f46e5", "#0d9488", "#db2777", "#ca8a04", "#7c3aed" };

        private static readonly string[] AvatarColors = { "#4f46e5", "#0d9488", "#db2777", "#ca8a04", "#2563eb", "#7c3aed", "#dc2626" };

        public static string PrintToString(IDocumentWorkItemList workItemList, Config config)
        {
            StringBuilder sb = new StringBuilder();
            using TextWriter textWriter = new StringWriter(sb);

            textWriter.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
            textWriter.WriteLine(@"<!DOCTYPE html PUBLIC ""-//W3C//DTD XHTML 1.0 Strict//EN"" ""http://www.w3.org/TR/xhtml1/DTD/xhtml1-strict.dtd"">");
            textWriter.WriteLine("<html>");
            textWriter.WriteLine("<head>");
            textWriter.WriteLine("<meta charset='utf-8' />");
            textWriter.WriteLine($"<title>{config.HTMLTitle}</title>");
            textWriter.WriteLine(GetStyles());
            textWriter.WriteLine($"<script src='ItemReportScript.js?v={DateTime.Now.Ticks}'></script>");
            textWriter.WriteLine("</head>");
            textWriter.WriteLine("<body ondblclick='onDocumentClick(event)'>");

            textWriter.WriteLine("<div class='toolbar'>");
            textWriter.WriteLine($"  <div class='brand'><h1>{config.HTMLTitle}</h1><span class='sub'>AzCamtek / Falcon</span></div>");
            textWriter.WriteLine("  <label class='search'><svg viewBox='0 0 24 24' fill='none' stroke='currentColor' stroke-width='2'><circle cx='11' cy='11' r='7'/><path d='m21 21-4.3-4.3'/></svg><input type='text' placeholder='Filter items…' aria-label='Filter items' oninput='onFilter(this.value)' /></label>");
            textWriter.WriteLine("  <button class='btn' onclick='OnCollapseAll()'><svg viewBox='0 0 24 24' fill='none' stroke='currentColor' stroke-width='2'><path d='M4 9h16M4 15h16'/></svg>Collapse all</button>");
            textWriter.WriteLine("  <button class='btn icon' onclick='toggleTheme()' title='Light / dark' aria-label='Toggle theme'><svg viewBox='0 0 24 24' fill='none' stroke='currentColor' stroke-width='2'><path d='M21 12.8A9 9 0 1 1 11.2 3a7 7 0 0 0 9.8 9.8z'/></svg></button>");
            textWriter.WriteLine("</div>");

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

            textWriter.WriteLine("<div class='board'>");
            textWriter.WriteLine("<div class='scroller'>");
            textWriter.WriteLine("<table>");
            textWriter.WriteLine("<thead>");
            textWriter.WriteLine("<tr>");
            textWriter.WriteLine("<th>Id</th>");
            textWriter.WriteLine("<th>Work item</th>");
            textWriter.WriteLine("<th>State</th>");
            textWriter.WriteLine("<th>Assignee</th>");

            foreach (string path in reportedPaths)
            {
                textWriter.WriteLine($"<th class='br'>{HttpUtility.HtmlAttributeEncode(path)}</th>");
            }

            textWriter.WriteLine("</tr>");
            textWriter.WriteLine("</thead>");

            int colorIndex = -1;

            // Каждый item 1-го уровня получает следующий цвет-рельс по кругу.
            // colorIndex общий на обе секции. Сами строки рисует RenderRows.
            void RenderSection(IEnumerable<IDocumentWorkItem> items)
            {
                foreach (IDocumentWorkItem workItem in items)
                {
                    colorIndex = (colorIndex + 1) % Rails.Length;
                    textWriter.Write(RenderRows(new[] { workItem }, reportedPaths, Rails[colorIndex], 0, 0));
                }
            }

            textWriter.WriteLine($"<tr class='section'><td colspan='{reportedPaths.Length + 4}'><span class='section-label'><span class='dot' style='background:#b45309'></span><h2>Not completed</h2></span></td></tr>");

            RenderSection(workItemList.Where(r => r.HasActiveSubItems));

            textWriter.WriteLine($"<tr class='section'><td colspan='{reportedPaths.Length + 4}'><span class='section-label'><span class='dot' style='background:#15803d'></span><h2>Completed</h2></span></td></tr>");

            RenderSection(workItemList.Where(r => !r.HasActiveSubItems));

            textWriter.WriteLine("</table>");
            textWriter.WriteLine("</div>");
            textWriter.WriteLine("</div>");
            textWriter.WriteLine("<div class='toasts' id='toasts'></div>");
            textWriter.WriteLine("</body>");
            textWriter.WriteLine("</html>");

            return sb.ToString();
        }

        // По одной <td> на колонку. Все PR item'а идут по возрастанию id и
        // раскладываются «лесенкой»: каждый PR занимает свою строку во всех
        // колонках (в своей ветке — чип, в остальных пусто), строки выровнены.
        private static string RenderPullRequestCells(string[] reportedPaths, IEnumerable<(DocumentPullRequest Request, bool IsOwner)> pullRequestList)
        {
            if (reportedPaths.Length <= 0)
            {
                return string.Empty;
            }

            (DocumentPullRequest Request, bool IsOwner)[] all = pullRequestList.OrderBy(r => r.Request.Id).ToArray();

            StringBuilder sb = new StringBuilder();

            foreach (string path in reportedPaths)
            {
                sb.Append("<td class='pr'>");

                if (all.Length == 0)
                {
                    sb.Append("<span class='cell-empty'>·</span>");
                }
                else
                {
                    sb.Append("<span class='prladder'>");
                    foreach ((DocumentPullRequest Request, bool IsOwner) tuple in all)
                    {
                        sb.Append(tuple.Request.TargetRefName == path
                            ? $"<span class='pr-line'>{GetLinkText(tuple)}</span>"
                            : "<span class='pr-line'></span>");
                    }
                    sb.Append("</span>");
                }

                sb.Append("</td>");
            }

            return sb.ToString();
        }

        // Рисует переданные item'ы вместе с их поддеревьями. Цвет-рельс задаётся
        // снаружи и наследуется вниз (ротацию 1-го уровня делает вызывающий код).
        // Используется и полным отчётом, и перерисовкой поддерева.
        public static string RenderRows(IEnumerable<IDocumentWorkItem> items, string[] reportedPaths, string colorHtml, int levelNumber, int parentItemId)
        {
            StringBuilder sb = new StringBuilder();
            using TextWriter textWriter = new StringWriter(sb);

            foreach (IDocumentWorkItem workItem in items)
            {
                (DocumentPullRequest Request, bool IsOwner)[] pullRequestList = workItem.GetFullPullRequestList().Where(re => reportedPaths.Contains(re.Request.TargetRefName)).ToArray();

                bool hasKids = workItem.SubItems.Any();
                bool done = workItem.IsClosed || workItem.IsResolved;
                (string fg, string bg) = GetTypeColors(workItem.WorkItemType);

                textWriter.WriteLine($"<tr class='item lvl{levelNumber} childOf_{parentItemId}{(done ? " done" : "")}' id='{workItem.Id}' style='--rail:{colorHtml};'>");

                // ------------ Id
                string mark = hasKids
                    ? $"<span id='mark' class='caret' onclick='OnMarkClick(this, {workItem.Id})'>◢</span>"
                    : "<span class='caret leaf'></span>";
                string indent = levelNumber > 0 ? $"<span class='indent' style='width:{levelNumber * 18}px'></span>" : "";
                string subItemsCount = hasKids ? $"<span class='subcount'>({workItem.SubItems.Count()})</span>" : string.Empty;
                string refreshIcon = levelNumber == 0
                    ? $"<span class='refresh' onclick='OnRefreshClick({workItem.Id})' title='Reload from Azure'><svg viewBox='0 0 24 24' fill='none' stroke='currentColor' stroke-width='2'><path d='M21 12a9 9 0 1 1-2.6-6.4M21 3v5h-5'/></svg></span>"
                    : string.Empty;

                textWriter.Write($"<td class='idcell'><span class='idwrap'>{indent}{mark}<a class='idlink' href='{workItem.Html}' target='_blank'>{workItem.Id}</a>{subItemsCount}{refreshIcon}</span></td>");

                // ------------ Work item (glyph + type pill + title)
                string strike = (workItem.IsClosed && pullRequestList.Length == 0) ? " strike" : string.Empty;
                textWriter.Write($"<td><span class='title'><span class='glyph' style='color:{fg}'>&#128447;</span><span class='type' style='color:{fg};background:{bg}'>{workItem.WorkItemType}</span><span class='name{strike}'>{workItem.Title}</span></span></td>");

                // ------------ State
                string has = workItem.HasActiveSubItems ? "<span class='has'>HAS</span>" : string.Empty;
                textWriter.Write($"<td><span class='chip {GetStateClass(workItem)}'><span class='tick'></span>{workItem.State}</span>{has}</td>");

                // ------------ Assignee
                textWriter.Write($"<td><span class='who'><span class='avatar' style='background:{AvatarColor(workItem.AssignedTo)}'>{Initials(workItem.AssignedTo)}</span>{workItem.AssignedTo}</span></td>");

                // ------------ PR ladder
                textWriter.Write(RenderPullRequestCells(reportedPaths, pullRequestList));

                textWriter.WriteLine("</tr>");

                textWriter.Write(RenderRows(workItem.SubItems, reportedPaths, colorHtml, levelNumber + 1, workItem.Id));
            }

            return sb.ToString();
        }

        // Цвет текста и мягкий фон плашки типа work item'а.
        private static (string fg, string bg) GetTypeColors(string workItemType)
        {
            switch (workItemType)
            {
                case "Bug": return ("#dc2626", "#fdecec");
                case "Task": return ("#b45309", "#fdf1e3");
                case "Task-Validation": return ("#15803d", "#e7f5ec");
                case "Feature": return ("#7c3aed", "#f1e9fe");
                case "Requirement": return ("#1d4ed8", "#e8effc");
                case "Issue": return ("#9f1239", "#fde7ec");
                case "ClarityFeature": return ("#4f46e5", "#eef0fe");
                case "ClarityTask": return ("#0d9488", "#e2f5f2");
                default: return ("#475569", "#eef2f7");
            }
        }

        private static string GetStateClass(IDocumentWorkItem workItem)
        {
            if (workItem.IsClosed) return "s-closed";
            if (workItem.IsResolved) return "s-resolved";
            if (workItem.IsActive) return "s-active";
            if (workItem.IsInProgress) return "s-progress";
            return "s-proposed";
        }

        private static string AvatarColor(string name)
        {
            int sum = 0;
            foreach (char c in name)
            {
                sum += c;
            }
            return AvatarColors[Math.Abs(sum) % AvatarColors.Length];
        }

        private static string Initials(string name)
        {
            string[] parts = name.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            StringBuilder sb = new StringBuilder();
            foreach (string p in parts)
            {
                sb.Append(char.ToUpper(p[0]));
                if (sb.Length == 2)
                {
                    break;
                }
            }

            return sb.Length == 0 ? "?" : sb.ToString();
        }

        private static string GetLinkText((DocumentPullRequest request, bool owner) pullRequest)
        {
            string date = $"{pullRequest.request.CloseDate:yyyy/MM/dd HH:mm}";

            string title = $"{pullRequest.request.TargetRefName} · {date} · {pullRequest.request.CreateBy} · {pullRequest.request.Status}";

            string cls = pullRequest.request.Status == "active" ? "active"
                : pullRequest.request.Status == "abandoned" ? "abandoned"
                : "merged";

            string owner = pullRequest.owner ? "<span class='owner'> ★</span>" : string.Empty;

            return $"<a class='prtag {cls}' href='https://dev.azure.com/AzCamtek/GIT/_git/CamtekGit/pullrequest/{pullRequest.request.Id}' target='_blank' title='{title}'>{pullRequest.request.Id}{owner}</a>";
        }

        private static string GetStyles()
        {
            return @"
<style type='text/css'>
  :root {
    --bg: #f5f7fa;
    --surface: #ffffff;
    --surface-2: #f8fafc;
    --ink: #1e293b;
    --ink-soft: #475569;
    --muted: #94a3b8;
    --line: #e6ebf1;
    --line-strong: #d3dbe6;
    --accent: #4f46e5;
    --accent-soft: #eef0fe;
    --good: #15803d;
    --good-soft: #e7f5ec;
    --warn: #b45309;
    --warn-soft: #fdf1e3;
    --info: #1d4ed8;
    --info-soft: #e8effc;
    --shadow: 0 1px 2px rgba(15,23,42,.06), 0 8px 24px -12px rgba(15,23,42,.18);
    --sans: ui-sans-serif, system-ui, ""Segoe UI"", Roboto, Helvetica, Arial, sans-serif;
    --mono: ui-monospace, ""Cascadia Code"", ""SF Mono"", Consolas, ""Liberation Mono"", monospace;
  }
  :root[data-theme='dark'] {
    --bg: #0b1220;
    --surface: #111a2b;
    --surface-2: #0e1626;
    --ink: #e2e8f0;
    --ink-soft: #a9b6c9;
    --muted: #64748b;
    --line: #223049;
    --line-strong: #2c3d59;
    --accent: #818cf8;
    --accent-soft: #1e2544;
    --good: #4ade80;
    --good-soft: #10241a;
    --warn: #fbbf24;
    --warn-soft: #2a1f08;
    --info: #93b4fb;
    --info-soft: #14213f;
    --shadow: 0 1px 2px rgba(0,0,0,.4), 0 12px 30px -16px rgba(0,0,0,.7);
  }

  * { box-sizing: border-box; }
  body {
    margin: 0;
    background: var(--bg);
    color: var(--ink);
    font-family: var(--sans);
    font-size: 14px;
    line-height: 1.45;
    -webkit-font-smoothing: antialiased;
  }

  .toolbar {
    position: sticky; top: 0; z-index: 30;
    display: flex; align-items: center; gap: 14px;
    min-height: 53px; padding: 10px 20px;
    background: color-mix(in srgb, var(--surface) 88%, transparent);
    backdrop-filter: blur(10px);
    border-bottom: 1px solid var(--line);
  }
  .brand { display: flex; align-items: baseline; gap: 10px; margin-right: auto; }
  .brand h1 { font-size: 15px; font-weight: 650; letter-spacing: -.01em; margin: 0; }
  .brand .sub { font-family: var(--mono); font-size: 11px; color: var(--muted); }
  .search {
    display: flex; align-items: center; gap: 8px;
    background: var(--surface-2); border: 1px solid var(--line-strong);
    border-radius: 9px; padding: 6px 11px; min-width: 220px;
    transition: border-color .15s, box-shadow .15s;
  }
  .search:focus-within { border-color: var(--accent); box-shadow: 0 0 0 3px var(--accent-soft); }
  .search input { border: 0; outline: 0; background: transparent; color: var(--ink); font: inherit; width: 100%; }
  .search svg { width: 15px; height: 15px; color: var(--muted); flex: none; }

  .btn {
    display: inline-flex; align-items: center; gap: 7px;
    font: inherit; font-weight: 550; font-size: 13px; color: var(--ink-soft);
    background: var(--surface-2); border: 1px solid var(--line-strong);
    padding: 7px 13px; border-radius: 9px; cursor: pointer;
    transition: background .15s, border-color .15s, color .15s, transform .05s;
  }
  .btn:hover { border-color: var(--accent); color: var(--accent); }
  .btn:active { transform: translateY(1px); }
  .btn.icon { padding: 7px; }
  .btn svg { width: 15px; height: 15px; }

  .board { padding: 18px 20px 60px; }
  .scroller {
    overflow: auto; max-height: calc(100vh - 96px); background: var(--surface);
    border: 1px solid var(--line); border-radius: 14px; box-shadow: var(--shadow);
  }
  table { border-collapse: collapse; width: 100%; min-width: 900px; }

  thead th {
    position: sticky; top: 0; z-index: 20;
    background: var(--surface-2); text-align: left;
    font-size: 11px; font-weight: 600; letter-spacing: .06em; text-transform: uppercase;
    color: var(--muted); padding: 11px 14px;
    border-bottom: 1px solid var(--line-strong); white-space: nowrap;
  }
  thead th.br { font-family: var(--mono); text-transform: none; letter-spacing: 0; color: var(--ink-soft); }

  tbody td { padding: 9px 14px; border-bottom: 1px solid var(--line); vertical-align: top; }
  tbody tr.item { transition: background .12s; }
  tbody tr.item:hover { background: color-mix(in srgb, var(--accent) 5%, transparent); }

  tr.section td {
    background: var(--surface-2);
    border-top: 1px solid var(--line-strong); border-bottom: 1px solid var(--line-strong);
  }
  .section-label { display: flex; align-items: center; gap: 9px; }
  .section-label .dot { width: 8px; height: 8px; border-radius: 50%; }
  .section-label h2 { margin: 0; font-size: 12px; font-weight: 650; letter-spacing: .04em; text-transform: uppercase; }

  td.idcell { white-space: nowrap; border-left: 6px solid var(--rail, transparent); }
  .idwrap { display: flex; align-items: center; gap: 6px; }
  .indent { display: inline-block; flex: none; }
  .caret {
    width: 18px; height: 20px; flex: none; display: inline-grid; place-items: center;
    color: var(--muted); cursor: pointer; border-radius: 5px; user-select: none;
    transition: background .12s, color .12s;
  }
  .caret:hover { background: var(--accent-soft); color: var(--accent); }
  .caret.leaf { cursor: default; color: var(--line-strong); }
  .idlink { font-family: var(--mono); font-size: 12.5px; color: var(--accent); text-decoration: none; font-variant-numeric: tabular-nums; }
  .idlink:hover { text-decoration: underline; }
  .subcount { font-family: var(--mono); font-size: 11px; color: var(--muted); }
  .refresh {
    width: 22px; height: 22px; border-radius: 6px; flex: none;
    display: inline-grid; place-items: center; cursor: pointer;
    color: var(--muted); border: 1px solid transparent;
    transition: color .12s, background .12s, border-color .12s;
  }
  .refresh:hover { color: var(--accent); background: var(--accent-soft); border-color: color-mix(in srgb, var(--accent) 30%, transparent); }
  .refresh svg { width: 14px; height: 14px; }
  .refresh.spinning svg { animation: spin .7s linear infinite; }
  @keyframes spin { to { transform: rotate(360deg); } }

  .title { display: flex; align-items: baseline; gap: 8px; }
  .title .glyph { flex: none; }
  .type { font-weight: 600; font-size: 11px; padding: 1px 7px; border-radius: 999px; white-space: nowrap; }
  .name { color: var(--ink); }
  tr.done .name { color: var(--ink-soft); }
  .name.strike { text-decoration: line-through; text-decoration-color: var(--muted); }

  .chip { display: inline-flex; align-items: center; gap: 5px; font-size: 12px; font-weight: 550; padding: 2px 9px; border-radius: 999px; white-space: nowrap; }
  .chip .tick { width: 6px; height: 6px; border-radius: 50%; background: currentColor; opacity: .85; }
  .chip.s-closed { color: var(--good); background: var(--good-soft); }
  .chip.s-resolved { color: var(--info); background: var(--info-soft); }
  .chip.s-active { color: var(--warn); background: var(--warn-soft); }
  .chip.s-progress { color: var(--info); background: var(--info-soft); }
  .chip.s-proposed { color: var(--ink-soft); background: var(--surface-2); }
  .has { margin-left: 6px; font-family: var(--mono); font-size: 10px; color: var(--warn); border: 1px solid color-mix(in srgb, var(--warn) 35%, transparent); border-radius: 5px; padding: 0 4px; }

  .who { display: inline-flex; align-items: center; gap: 7px; color: var(--ink-soft); white-space: nowrap; }
  .avatar { width: 20px; height: 20px; border-radius: 50%; display: grid; place-items: center; font-size: 10px; font-weight: 650; color: #fff; flex: none; }

  td.pr { white-space: nowrap; vertical-align: top; }
  .prladder { display: flex; flex-direction: column; gap: 5px; }
  .pr-line { min-height: 22px; display: flex; align-items: center; }
  .prtag {
    display: inline-flex; align-items: center; gap: 5px; align-self: flex-start;
    font-family: var(--mono); font-size: 11.5px; text-decoration: none;
    padding: 1px 8px; border-radius: 7px; border: 1px solid var(--line-strong);
    color: var(--ink-soft); background: var(--surface-2);
    transition: border-color .12s, color .12s;
  }
  .prtag:hover { border-color: var(--accent); color: var(--accent); }
  .prtag.active { border-color: color-mix(in srgb, var(--warn) 45%, transparent); color: var(--warn); background: var(--warn-soft); font-weight: 600; }
  .prtag.merged { border-color: color-mix(in srgb, var(--good) 40%, transparent); color: var(--good); background: var(--good-soft); }
  .prtag.abandoned { color: var(--muted); text-decoration: line-through; }
  .prtag .owner { color: var(--accent); font-weight: 700; }
  .cell-empty { color: var(--line-strong); }

  @keyframes flash {
    0% { background: color-mix(in srgb, var(--accent) 22%, transparent); }
    100% { background: transparent; }
  }
  tr.flash { animation: flash 1.1s ease-out; }

  .toasts { position: fixed; right: 18px; bottom: 18px; display: flex; flex-direction: column; gap: 8px; z-index: 60; }
  .toast {
    display: flex; align-items: center; gap: 9px;
    background: var(--surface); color: var(--ink);
    border: 1px solid var(--line-strong); border-left: 3px solid var(--good);
    border-radius: 10px; padding: 10px 14px; box-shadow: var(--shadow); font-size: 13px;
  }
  .toast.err { border-left-color: #dc2626; }
  .toast .tdot { width: 7px; height: 7px; border-radius: 50%; background: var(--good); flex: none; }

  @media (prefers-reduced-motion: reduce) {
    *, *::before { animation: none !important; transition: none !important; }
  }
</style>
";
        }
    }
}
