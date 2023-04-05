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
        private static readonly string[] BranchPaths = {/*"ver/10.0/dev", "ver/10.0/2023/01/rel", "ver/10.0/2023/03/rel"/**/};

        public static void Print(IDocumentWorkItemList workItemList)
        {
            using TextWriter textWriter = new StreamWriter(@"c:\temp\mixa.html");

            string[] paths = Tools.GetUniquePath(workItemList).Where(l => BranchPaths.Length == 0 || BranchPaths.Contains(l)).OrderBy(t => t).ToArray();

            #region string Styles()

            string Styles()
            {
                return @"
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
";
            }

            #endregion

            textWriter.WriteLine(@"<!DOCTYPE html PUBLIC ""-//W3C//DTD XHTML 1.0 Strict//EN"" ""http://www.w3.org/TR/xhtml1/DTD/xhtml1-strict.dtd"">");
            textWriter.WriteLine("<html>");
            textWriter.WriteLine("<head>");
            //textWriter.WriteLine(Styles());
            textWriter.WriteLine("</head>");
            textWriter.WriteLine("<body>");
            textWriter.WriteLine("<table border='1' class='freeze-table'>");

            textWriter.WriteLine("<tr>");
            textWriter.WriteLine("<th class='col-id-number fixed-header'>Id</th>");
            textWriter.WriteLine("<th class='col-type fixed-header'>Type</th>");
            textWriter.WriteLine("<th>State</th>");
            textWriter.WriteLine("<th>Title</th>");

            foreach (string path in paths)
            {
                textWriter.WriteLine($"<th>{HttpUtility.HtmlAttributeEncode(path)}</th>");
            }

            textWriter.WriteLine("</tr>");

            #region void PrintPullRequestList(IEnumerable<DocumentPullRequest> pullRequestList)

            void PrintPullRequestList(IEnumerable<DocumentPullRequest> pullRequestList)
            {
                if (paths.Length > 0)
                {
                    StringBuilder sb = new StringBuilder();

                    foreach (string path in paths)
                    {
                        sb.Append("<td>");

                        DocumentPullRequest[] pullRequests = pullRequestList.Where(pp => pp.TargetRefName == path).OrderBy(p => p.Id).ToArray();

                        if (pullRequests.Length != 0)
                        {
                            IEnumerable<string> enumerable = pullRequests.Select(p =>
                            {
                                string date = $"{p.CloseDate:yyyy/MM/dd HH:mm}";
                                return $"<span title='&ldquo;{p.TargetRefName}&rdquo;&nbsp;{date}&nbsp;{p.CreateBy}'>{p.Id}</span>";
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
            }

            #endregion

            Color[] colors = new[] {Color.Aquamarine, Color.MistyRose, Color.LightSkyBlue};
            int colorIndex = -1;

            #region void PrintLevel(IDocumentWorkItemList levelList, int levelNumber, Color color)

            void PrintLevel(IDocumentWorkItemList levelList, int levelNumber, Color color)
            {
                foreach (DocumentWorkItem workItem in levelList.GetWorkItems())
                {
                    if (levelNumber == 0)
                    {
                        colorIndex = (colorIndex + 1) % colors.Length;
                        color = colors[colorIndex];
                    }

                    DocumentPullRequest[] pullRequestList = workItem.GetFullPullRequestList().ToArray();

                    string style = $"background-color:{ColorTranslator.ToHtml(color)};";
                    if (pullRequestList.Length == 0)
                    {
                        style += " font-weight: bold;";
                    }

                    textWriter.WriteLine($"<tr style='{style};'>");

                    textWriter.Write($"<td class='col-id-number' scope='row'><a href='{workItem.Html}' target='_blank'>{workItem.Id}</a></td>");
                    textWriter.Write($"<td class='col-type' scope='row'>{workItem.WorkItemType}</td>");
                    textWriter.Write($"<td>{workItem.State}</td>");
                    textWriter.Write($"<td>{string.Concat(Enumerable.Repeat("&nbsp;", levelNumber * 3))}{workItem.Title}</td>");

                    PrintPullRequestList(pullRequestList);

                    textWriter.WriteLine("</tr>");

                    PrintLevel(workItem.SubItems, levelNumber + 1, color);
                }
            }

            #endregion

            PrintLevel(workItemList, 0, Color.White);

            textWriter.WriteLine("</table>");
            textWriter.WriteLine("</body>");
            textWriter.WriteLine("</html>");
        }
    }
}
