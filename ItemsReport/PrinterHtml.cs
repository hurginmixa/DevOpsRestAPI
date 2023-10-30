using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using CommonCode;
using CommonCode.DocumentClasses;

namespace ItemsReport
{
    public static class PrinterHtml
    {
        public static void Print(IDocumentWorkItemList workItemList, Config config)
        {
            using Stream textStream = new FileStream(PPath.GetExeDirectory() / config.OutputFile, FileMode.Create);
            using TextWriter textWriter = new StreamWriter(textStream, Encoding.UTF8);

            textWriter.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
            textWriter.WriteLine(@"<!DOCTYPE html PUBLIC ""-//W3C//DTD XHTML 1.0 Strict//EN"" ""http://www.w3.org/TR/xhtml1/DTD/xhtml1-strict.dtd"">");
            textWriter.WriteLine("<html>");
            textWriter.WriteLine("<head>");
            textWriter.WriteLine(GetStyles());
            textWriter.WriteLine(GetScripts());
            textWriter.WriteLine("</head>");
            textWriter.WriteLine("<body ondblclick='onDocumentClick(event)'>");
            textWriter.WriteLine("&nbsp;&nbsp;&nbsp;<button onclick='OnCollapseAll()' class='favorite styled'>Collapse All</button><br /><br />");
            textWriter.WriteLine("<table>");

            string[] reportedPaths = workItemList.GetUniqueCommittedPaths().OrderBy(s => s).ToArray();

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

            #region void PrintPullRequestList(IEnumerable<DocumentPullRequest> pullRequestList)

            void PrintPullRequestList(IEnumerable<(DocumentPullRequest Request, bool IsOwner)> pullRequestList)
            {
                if (reportedPaths.Length <= 0)
                {
                    return;
                }

                (DocumentPullRequest Request, bool IsOwner)[] l = pullRequestList.OrderBy(r => r.Request.Id).ToArray();

                string[] r = pullRequestList.GroupBy(r => r.Request.TargetRefName).Select(r => r.Key).ToArray();


                Dictionary<string, StringBuilder> builders = reportedPaths.ToDictionary(i => i, i => new StringBuilder());

                foreach ((DocumentPullRequest Request, bool IsOwner) tuple in l)
                {
                    foreach (string path in reportedPaths)
                    {
                        string linkText = tuple.Request.TargetRefName == path ? GetLinkText(tuple) : "&nbsp;";

                        builders[path].Append($"{linkText}<br>");
                    }                    
                }

                StringBuilder sb = new StringBuilder();

                foreach (string path in reportedPaths)
                {
                    sb.Append("<td>");

                    sb.Append(builders[path].ToString());

                    sb.Append("</td>");
                }

                textWriter.Write(sb);
            }

            #endregion

            Color[] colors = {Color.Aquamarine, Color.MistyRose, Color.LightSkyBlue, Color.Cornsilk, Color.DarkGray };
            int colorIndex = -1;

            #region void StartLevelsReporting(IDocumentWorkItemList oneLevelItemList)

            void StartLevelsReporting(IDocumentWorkItemList oneLevelItemList)
            {
                PrintLevel(oneLevelItemList, 0, Color.White, 0);
            }

            #endregion

            #region void PrintLevel(IDocumentWorkItemList oneLevelItemList, int levelNumber, Color color)

            void PrintLevel(IDocumentWorkItemList oneLevelItemList, int levelNumber, Color color, int parentItemId)
            {
                foreach (IDocumentWorkItem workItem in oneLevelItemList)
                {
                    (DocumentPullRequest Request, bool IsOwner)[] pullRequestList = workItem.GetFullPullRequestList().Where(re => reportedPaths.Contains(re.Request.TargetRefName)).ToArray();

                    if (levelNumber == 0)
                    {
                        colorIndex = (colorIndex + 1) % colors.Length;
                        color = colors[colorIndex];
                    }

                    string style = $"background-color:{ColorTranslator.ToHtml(color)};";
                    if (pullRequestList.Length == 0 && !workItem.IsClosed)
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

                        //lineShift += string.Concat(Enumerable.Repeat(">&nbsp;", levelNumber));

                        lineShift += "&nbsp;&nbsp;";
                    }

                    string subItemsCount = workItem.SubItems.Any() ? $"&nbsp;(&nbsp;{workItem.SubItems.Count()}&nbsp;)" : string.Empty;

                    textWriter.Write($"<td style='white-space: nowrap'><code>{lineShift}{markSpan}</code>&nbsp;<a href='{workItem.Html}' target='_blank'>{workItem.Id}</a>{subItemsCount}</td>");

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
                    textWriter.Write($"<td>{workItem.State}</td>");

                    textWriter.Write($"<td>{workItem.AssignedTo}</td>");

                    PrintPullRequestList(pullRequestList);

                    textWriter.WriteLine("</tr>");

                    PrintLevel(workItem.SubItems, levelNumber + 1, color, workItem.Id);
                }
            }

            #endregion

            textWriter.WriteLine("<tr>");
            textWriter.WriteLine($"<td colspan='{reportedPaths.Length + 4}'><h1>Not completed items</h1></td>");
            textWriter.WriteLine("</tr>");

            StartLevelsReporting(oneLevelItemList: new DocumentWorkItemList(workItemList.Where(r => r.HasActiveSubItems)));

            textWriter.WriteLine("<tr>");
            textWriter.WriteLine($"<td colspan='{reportedPaths.Length + 4}'><h1>Completed items</h1></td>");
            textWriter.WriteLine("</tr>");

            StartLevelsReporting(oneLevelItemList: new DocumentWorkItemList(workItemList.Where(r => !r.HasActiveSubItems)));

            textWriter.WriteLine("</table>");
            textWriter.WriteLine("</body>");
            textWriter.WriteLine("</html>");
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

            throw new System.NotImplementedException();
        }

        private static string GetScripts()
        {
            return @"
<script src='Scripts/FirstScript.js'></script>
" ;
        }

        private static string GetLinkText((DocumentPullRequest request, bool owner) pullRequest)
        {
            string date = $"{pullRequest.request.CloseDate:yyyy/MM/dd HH:mm}";

            string title =
                $"&ldquo;{pullRequest.request.TargetRefName}&rdquo;&nbsp;{date}&nbsp;{pullRequest.request.CreateBy}&nbsp;{pullRequest.request.Status}";

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
