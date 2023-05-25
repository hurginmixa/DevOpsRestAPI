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
            using TextWriter textWriter = new StreamWriter(PPath.GetExeDirectory() / config.OutputFile);

            textWriter.WriteLine(@"<!DOCTYPE html PUBLIC ""-//W3C//DTD XHTML 1.0 Strict//EN"" ""http://www.w3.org/TR/xhtml1/DTD/xhtml1-strict.dtd"">");
            textWriter.WriteLine("<html>");
            textWriter.WriteLine("<head>");
            textWriter.WriteLine(GetStyles());
            textWriter.WriteLine("</head>");
            textWriter.WriteLine("<body>");
            textWriter.WriteLine("<table border='1' class='freeze-table'>");

            string[] reportedPaths = workItemList.GetUniqueCommittedPaths().ToArray();

            textWriter.WriteLine("<tr>");
            textWriter.WriteLine("<th class='col-id-number fixed-header'>Id</th>");
            textWriter.WriteLine("<th class='col-type fixed-header'>Type</th>");
            textWriter.WriteLine("<th>State</th>");
            textWriter.WriteLine("<th>Title</th>");

            foreach (string path in reportedPaths)
            {
                textWriter.WriteLine($"<th>{HttpUtility.HtmlAttributeEncode(path)}</th>");
            }

            textWriter.WriteLine("</tr>");

            #region void PrintPullRequestList(IEnumerable<DocumentPullRequest> pullRequestList)

            void PrintPullRequestList(IEnumerable<(DocumentPullRequest Request, bool IsOwner)> pullRequestList)
            {
                if (reportedPaths.Length <= 0)
                {
                    return;
                }

                StringBuilder sb = new StringBuilder();

                foreach (string path in reportedPaths)
                {
                    sb.Append("<td>");

                    (DocumentPullRequest request, bool owner)[] pullRequests = pullRequestList.Where(pp => pp.Request.TargetRefName == path).OrderBy(p => p.Request.Id).ToArray();

                    if (pullRequests.Length > 0)
                    {
                        IEnumerable<string> enumerable = pullRequests.Select(pullRequest =>
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

                            linkText = $"<a href='https://dev.azure.com/AzCamtek/GIT/_git/CamtekGit/pullrequest/{pullRequest.request.Id}' target='_blank'>{linkText}</a>"; // 😪😪😪

                            if (pullRequest.owner)
                            {
                                linkText += "*";
                            }

                            return linkText;
                        });

                        sb.Append(enumerable.JoinToString("<br />"));
                    }
                    else
                    {
                        sb.Append("&nbsp;");
                    }

                    sb.Append("</td>");
                }

                textWriter.Write(sb);
            }

            #endregion

            Color[] colors = {Color.Aquamarine, Color.MistyRose, Color.LightSkyBlue, Color.Cornsilk};
            int colorIndex = -1;

            #region void StartLevelsReporting(IDocumentWorkItemList levelList)

            void StartLevelsReporting(IDocumentWorkItemList levelList)
            {
                PrintLevel(levelList, 0, Color.White);
            }

            #endregion

            #region void PrintLevel(IDocumentWorkItemList levelList, int levelNumber, Color color)

            void PrintLevel(IDocumentWorkItemList levelList, int levelNumber, Color color)
            {
                foreach (IDocumentWorkItem workItem in levelList)
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

                    textWriter.WriteLine($"<tr style='{style};'>");

                    textWriter.Write($"<td class='col-id-number' scope='row'><a href='{workItem.Html}' target='_blank'>{workItem.Id}</a></td>");
                    textWriter.Write($"<td class='col-type' scope='row'>{workItem.WorkItemType}</td>");
                    textWriter.Write($"<td>{workItem.State}</td>");

                    string workItemTitle = workItem.Title;
                    if (workItem.IsClosed && pullRequestList.Length == 0)
                    {
                        workItemTitle = $"<S>{workItemTitle}</S>";
                    }

                    textWriter.Write($"<td>{string.Concat(Enumerable.Repeat("*&nbsp;", levelNumber))}{workItemTitle}</td>");

                    PrintPullRequestList(pullRequestList);

                    textWriter.WriteLine("</tr>");

                    PrintLevel(workItem.SubItems, levelNumber + 1, color);
                }
            }

            #endregion

            textWriter.WriteLine("<tr>");
            textWriter.WriteLine($"<td colspan='{reportedPaths.Length + 4}'><h1>Not completed items</h1></td>");
            textWriter.WriteLine("</tr>");

            StartLevelsReporting(levelList: new DocumentWorkItemList(workItemList.Where(r => r.HasActiveSubItems)));

            textWriter.WriteLine("<tr>");
            textWriter.WriteLine($"<td colspan='{reportedPaths.Length + 4}'><h1>Completed items</h1></td>");
            textWriter.WriteLine("</tr>");

            StartLevelsReporting(levelList: new DocumentWorkItemList(workItemList.Where(r => !r.HasActiveSubItems)));

            textWriter.WriteLine("</table>");
            textWriter.WriteLine("</body>");
            textWriter.WriteLine("</html>");
        }

        private static string GetStyles()
        {
            return string.Empty;

            /*return @"
<style type='text/css'>
    .freeze-table {
        border-spacing: 0;
        font-family: 'Segoe UI', sans-serif, 'Helvetica Neue';
        font-size: 14px;
        padding: 0;
        border: 1px solid #ccc
    }

    thead th {
        top: 0;
        position: sticky;
        background-color: #666;
        color: #fff;
        z-index: 20;
        min-height: 30px;
        height: 30px;
        text-align: left;
    }

    tr:nth-child(even)
    {
        background-color: #f2f2f2;
    }

    th td {
        padding: 0;
        top: 0;
        outline: 1px solid #ccc;
        border: none;;
        outline-offset: -1px;
        padding-left: 5px;
    }

    tr {
        min-height: 25px;
        height: 25px;
    }

    .col-id-number
    {
        left: 0;
        position: sticky;
    }

    .col-type
    {
        left: 80;
        position: sticky;
    }

    .fixed-header
    {
        z-index: 50;
    }

    tr:nth-child(even) td[scope=row] {
        background-color: #f2f2f2;
    }

    tr:nth-child(odd) td[scope=row] {
        background-color: white;
    }

</style>
";*/
        }
    }
}
