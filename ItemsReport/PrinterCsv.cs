using System.IO;
using System.Linq;
using System.Text;
using CommonCode;
using CommonCode.DocumentClasses;

namespace ItemsReport
{
    public static class PrinterCsv
    {
        public static void Print(IDocumentWorkItemList workItemList)
        {
            using TextWriter textWriter = new StreamWriter(@"c:\temp\mixa.csv");

            string[] paths = workItemList.GetUniqueCommittedPaths().OrderBy(t => t).ToArray();

            textWriter.Write("Id,Type,State,Title");
            foreach (string path in paths)
            {
                textWriter.Write($",{path}");
            }

            textWriter.WriteLine();

            #region void PrintPullRequestList(IEnumerable<DocumentPullRequest> pullRequestList)

            void PrintPullRequestList((DocumentPullRequest Request, bool IsOwner)[] pullRequestList)
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

                        (DocumentPullRequest request, bool owner)[] pullRequests = pullRequestList.Where(pp => pp.Request.TargetRefName == path).OrderBy(p => p.Request.Id).ToArray();
                        sb.Append(pullRequests.Length != 0
                            ? pullRequests.Select(p => $"{p.request.Id}({p.request.CloseDate:yyyy/MM/dd HH:mm})").JoinToString(" ")
                            : "\"\"");
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
                foreach (IDocumentWorkItem workItem in levelList)
                {
                    textWriter.Write($"{workItem.Id},");
                    textWriter.Write($"{workItem.WorkItemType},");
                    textWriter.Write($"{workItem.State},");
                    textWriter.Write($"\"{new string(' ', levelNumber * 4)}{workItem.Title}\"");

                    PrintPullRequestList(workItem.GetFullPullRequestList().ToArray());

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
    }
}
