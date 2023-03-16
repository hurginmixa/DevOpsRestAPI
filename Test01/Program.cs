using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Test01.GitClasses;
using Microsoft.Office.Interop.Excel;
using Test01.DocumentClasses;

namespace Test01
{
    public static class Program
    {
        private static readonly int[] WorkItems =
        {
            34598, 39309, 39559, 39378, 39347, 39523, 39319,
            39967, 39926, 39862, 35781, 39346, 38358,
            38280, 37192, 38836, 37787, 40044, 33462,
            32807,
        };

        static async Task Main()
        {
            DocumentWorkItemList workItemList = new DocumentWorkItemList();

            await PerformWorkItems(WorkItems, workItemList);

            Console.WriteLine("Printing");

            PrintHtml(workItemList);

            Console.WriteLine("Complete");
        }

        private static void PrintCsv(IDocumentWorkItemList workItemList)
        {
            using TextWriter textWriter = new StreamWriter(@"c:\temp\mixa.csv");

            string[] paths = Tools.GetUniquePath(workItemList).OrderBy(t => t).ToArray();

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

                        DocumentPullRequest[] pullRequests = pullRequestList.Where(pp => pp.TargetRefName == path).ToArray();
                        if (pullRequests.Length != 0)
                        {
                            sb.Append(Tools.JoinToString(
                                pullRequests.Select(p => $"{p.Id}({p.CloseDate:yyyy/MM/dd HH:mm})"), " "));
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

            string[] paths = Tools.GetUniquePath(workItemList).OrderBy(t => t).ToArray();

            textWriter.WriteLine(@"<!DOCTYPE html PUBLIC ""-//W3C//DTD XHTML 1.0 Strict//EN"" ""http://www.w3.org/TR/xhtml1/DTD/xhtml1-strict.dtd"">");
            textWriter.WriteLine("<html>");
            textWriter.WriteLine("<head>");
            textWriter.WriteLine("</head>");
            textWriter.WriteLine("<body>");
            textWriter.WriteLine("<table border='1'>");

            textWriter.WriteLine("<tr>");
            textWriter.WriteLine("<td>Id</td><td>Type</td><td>State</td><td>Title</td>");

            foreach (string path in paths)
            {
                textWriter.WriteLine($"<td>{HttpUtility.HtmlAttributeEncode(path)}</td>");
            }

            textWriter.WriteLine("</tr>");

            void PrintPullRequestList(IEnumerable<DocumentPullRequest> pullRequestList)
            {
                if (paths.Length > 0)
                {
                    StringBuilder sb = new StringBuilder();

                    foreach (string path in paths)
                    {
                        sb.Append("<td>");

                        DocumentPullRequest[] pullRequests = pullRequestList.Where(pp => pp.TargetRefName == path).ToArray();
                        if (pullRequests.Length != 0)
                        {
                            sb.Append(Tools.JoinToString(
                                pullRequests.Select(p =>
                                {
                                    return $"<span title='{p.CloseDate:yyyy/MM/dd HH:mm}'>{p.Id}</span>";
                                }), "&nbsp;"));
                            //sb.Append(Tools.JoinToString(pullRequests.Select(p => $"{p.Id}({p.CloseDate:yyyy/MM/dd HH:mm})"), " "));
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

            void PrintLevel(IDocumentWorkItemList levelList, int levelNumber)
            {
                textWriter.WriteLine("<tr>");

                foreach (DocumentWorkItem workItem in levelList.GetWorkItems())
                {
                    textWriter.Write($"<td><a href='{workItem.Html}' target='_blank'>{workItem.Id}</a></td>");
                    textWriter.Write($"<td>{workItem.WorkItemType}</td>");
                    textWriter.Write($"<td>{workItem.State}</td>");
                    textWriter.Write($"<td>{string.Concat(Enumerable.Repeat("&nbsp;", levelNumber * 3))}{workItem.Title}</td>");

                    PrintPullRequestList(workItem.GetFullPullRequestList());

                    textWriter.WriteLine("</tr>");

                    PrintLevel(workItem.SubItems, levelNumber + 1);
                }
            }

            PrintLevel(workItemList, 0);

            textWriter.WriteLine("</table>");
            textWriter.WriteLine("</body>");
            textWriter.WriteLine("</html>");
        }

        private static async Task PerformWorkItems(IEnumerable<int> workItems, IDocumentWorkItemList workItemList)
        {
            string jsonResponseBody = CustJsonSerializer.FormatJson(await HttpTools.GetWorkItemListByIds(workItems.OrderBy(rr => rr)));
            GitWorkItemList gitWorkItemList = JsonSerializer.Deserialize<GitWorkItemList>(jsonResponseBody);

            foreach (GitWorkItem gitWorkItem in gitWorkItemList.Value)
            {
                Console.WriteLine($"{Thread.CurrentThread.ManagedThreadId} {gitWorkItem.Id} {gitWorkItem.Fields.Title}");

                List<int> childList = new List<int>();
                List<int> pullRequestList = new List<int>();

                foreach (GitRelation relation in gitWorkItem.Relations)
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
                    await PerformWorkItems(childList.ToArray(), workItem.SubItems);
                }
            }
        }
    }
}
