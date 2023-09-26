﻿using System.Collections.Generic;
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
            textWriter.WriteLine(GetScripts());
            textWriter.WriteLine("</head>");
            textWriter.WriteLine("<body>");
            textWriter.WriteLine("<table>");

            string[] reportedPaths = workItemList.GetUniqueCommittedPaths().OrderBy(s => s).ToArray();

            textWriter.WriteLine("<thead>");
            textWriter.WriteLine("<tr>");
            textWriter.WriteLine("<th class='sticky-column, sticky-row'>Id</th>");
            textWriter.WriteLine("<th class='sticky-column, sticky-row'>Type</th>");
            textWriter.WriteLine("<th class='sticky-column, sticky-row'>State</th>");
            textWriter.WriteLine("<th class='sticky-column, sticky-row'>Title</th>");

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

                    if (levelNumber > 0)
                    {
                        style += " display: none;";
                    }

                    textWriter.WriteLine($"<tr style='{style}'>");

                    textWriter.Write($"<td><a href='{workItem.Html}' target='_blank'>{workItem.Id}</a></td>");
                    textWriter.Write($"<td>{workItem.WorkItemType}</td>");
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
</style>
";
        }
    }
}
