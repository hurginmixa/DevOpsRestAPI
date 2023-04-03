using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Test01.GitClasses;
using Test01.DocumentClasses;
using Test01.GitClasses.GettingWorkItemsBatch;

namespace Test01
{
    public static class Program
    {
        private static readonly int[] WorkItemNumbers =
        {
            40178, 40180, 38967, 39059, 35861, 34516,

            34598, 39309, 39378, 39319,
            39967, 39346, 38358,
            38280, 37192, 38836, 37787, 40044, 33462,
            32807, 40071, 38966,

            40144, 40549,

            40044, 39309, 36757, 36514, 38280, 31765, 34112, 39244, 39926,

        };

        private static readonly string[] BranchPaths = {/*"ver/10.0/dev", "ver/10.0/2023/01/rel", "ver/10.0/2023/03/rel"/**/};

        static async Task Main()
        {
            DocumentWorkItemList workItemList = new DocumentWorkItemList();

            await PerformWorkItems(WorkItemNumbers, workItemList, 1);

            RemoveDuplicateItems(workItemList);

            Console.WriteLine("Printing");

            PrintHtml(workItemList);

            Console.WriteLine("Complete");
        }

        private static void RemoveDuplicateItems(IDocumentWorkItemList workItemList)
        {
            HashSet<DocumentWorkItem> items = new HashSet<DocumentWorkItem>(workItemList.GetWorkItems());
            HashSet<DocumentWorkItem> itemsToDelete = new HashSet<DocumentWorkItem>();

            void Rr(IDocumentWorkItemList list)
            {
                foreach (DocumentWorkItem workItem in list.GetWorkItems())
                {
                    if (items.Contains(workItem))
                    {
                        itemsToDelete.Add(workItem);
                    }

                    Rr(workItem.SubItems);
                }
            }

            foreach (DocumentWorkItem workItem in workItemList.GetWorkItems())
            {
                Rr(workItem.SubItems);
            }


            string duplicateNumbers = itemsToDelete.Select(i => i.Id).JoinToString(", ");
            Console.WriteLine(duplicateNumbers);

            foreach (DocumentWorkItem workItem in itemsToDelete)
            {
                workItemList.RemoveItem(workItem);
            }
        }

        private static void PrintCsv(IDocumentWorkItemList workItemList)
        {
            using TextWriter textWriter = new StreamWriter(@"c:\temp\mixa.csv");

            string[] paths = workItemList.GetUniquePath().OrderBy(t => t).ToArray();

            textWriter.Write("Id,Type,State,Title");
            foreach (string path in paths)
            {
                textWriter.Write($",{path}");
            }

            textWriter.WriteLine();

            #region void PrintPullRequestList(IEnumerable<DocumentPullRequest> pullRequestList)

            void PrintPullRequestList(IEnumerable<DocumentPullRequest> pullRequestList)
            {
                if (paths.Length > 0)
                {
                    StringBuilder sb = new StringBuilder();

                    foreach (string path in paths)
                    {
                        if (sb.Length > 0)
                        {
                            sb.Append(",");
                        }

                        DocumentPullRequest[] pullRequests = pullRequestList.Where(pp => pp.TargetRefName == path).OrderBy(p => p.Id).ToArray();
                        if (pullRequests.Length != 0)
                        {
                            sb.Append(pullRequests.Select(p => $"{p.Id}({p.CloseDate:yyyy/MM/dd HH:mm})").JoinToString(" "));
                        }
                        else
                        {
                            sb.Append("\"\"");
                        }
                    }

                    if (sb.Length > 0)
                    {
                        textWriter.Write($",{sb}");
                    }
                }
            }

            #endregion

            #region void PrintLevel(IDocumentWorkItemList levelList, int levelNumber)

            void PrintLevel(IDocumentWorkItemList levelList, int levelNumber)
            {
                foreach (DocumentWorkItem workItem in levelList.GetWorkItems())
                {
                    textWriter.Write($"{workItem.Id},");
                    textWriter.Write($"{workItem.WorkItemType},");
                    textWriter.Write($"{workItem.State},");
                    textWriter.Write($"\"{new string(' ', levelNumber * 4)}{workItem.Title}\"");

                    PrintPullRequestList(workItem.GetFullPullRequestList());

                    textWriter.WriteLine();

                    //if (workItem.WorkItemType != "Bug" && workItem.WorkItemType != "Feature")
                    {
                        PrintLevel(workItem.SubItems, levelNumber + 1);
                    }
                }
            }

            #endregion

            PrintLevel(workItemList, 0);
        }

        private static void PrintHtml(IDocumentWorkItemList workItemList)
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

        private static async Task PerformWorkItems(IEnumerable<int> workItemNumbers, IDocumentWorkItemList workItemList, int levelNumber)
        {
            string jsonResponseBody = CustJsonSerializer.FormatJson(await HttpTools.GetWorkItemListByIds(workItemNumbers.Distinct().OrderBy(rr => rr)));
            GitWorkItemList gitWorkItemList = JsonSerializer.Deserialize<GitWorkItemList>(jsonResponseBody);

            foreach (GitWorkItem gitWorkItem in gitWorkItemList.Value)
            {
                Console.WriteLine($"{Thread.CurrentThread.ManagedThreadId,2} {levelNumber,2} {gitWorkItem.Id,5} {gitWorkItem.Fields.Title}");

                if (gitWorkItem.Fields.WorkItemType == "Task-Validation")
                {
                    continue;
                }

                List<int> childList = new List<int>();
                List<int> pullRequestList = new List<int>();

                foreach (GitWorkItemRelation relation in gitWorkItem.Relations)
                {
                    if (relation.Attributes.Name == "Child" || relation.Attributes.Name == "Pull Request")
                    {
                        string decodedUrl = Uri.UnescapeDataString(relation.Url);
                        string txtId = decodedUrl.Substring(decodedUrl.LastIndexOf('/') + 1);

                        switch (relation.Attributes.Name)
                        {
                            case "Child":
                                childList.Add(int.Parse(txtId));
                                break;

                            case "Pull Request":
                                pullRequestList.Add(int.Parse(txtId));
                                break;
                        }
                    }
                }

                DocumentWorkItem workItem = new DocumentWorkItem(gitWorkItem);
                workItemList.AddWorkItem(workItem);

                if (pullRequestList.Count > 0)
                {
                    foreach (int pullRequestId in pullRequestList.OrderBy(rr => rr))
                    {
                        string result = CustJsonSerializer.FormatJson(await HttpTools.GetPullRequestById(pullRequestId));
                        GitPullRequest pullRequest = JsonSerializer.Deserialize<GitPullRequest>(result);

                        workItem.AddPullRequest(new DocumentPullRequest(pullRequest));
                    }
                }

                if (childList.Count > 0)
                {
                    await PerformWorkItems(childList.ToArray(), workItem.SubItems, levelNumber + 1);
                }
            }
        }
    }
}
