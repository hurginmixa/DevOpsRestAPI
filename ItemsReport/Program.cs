using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using CommonCode;
using CommonCode.DocumentClasses;
using CommonCode.GitClasses;
using ItemsReport;
using static System.Console;

namespace Test01
{
    public static class Program
    {
        static async Task Main()
        {
            Config config = Config.GetConfig();

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            DocumentWorkItemList workItemList = new DocumentWorkItemList();

            await PerformWorkItems(config.Ids, workItemList, 1, config);

            RemoveDuplicateItems(workItemList);

            WriteLine($"Printing, {stopwatch.Elapsed.TotalMilliseconds}");

            PrinterHtml.Print(workItemList, config);

            WriteLine($"Complete, {stopwatch.Elapsed.TotalMilliseconds}");

            stopwatch.Stop();
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
            WriteLine(duplicateNumbers);

            foreach (DocumentWorkItem workItem in itemsToDelete)
            {
                workItemList.RemoveItem(workItem);
            }
        }

        private static async Task PerformWorkItems(IEnumerable<int> workItemNumbers, IDocumentWorkItemList workItemList, int levelNumber, Config config)
        {
            string jsonResponseBody = CustJsonSerializer.FormatJson(await HttpTools.GetWorkItemListByIds(workItemNumbers.Distinct().OrderBy(rr => rr), config.Token));
            GitWorkItemList gitWorkItemList = JsonSerializer.Deserialize<GitWorkItemList>(jsonResponseBody);

            Task[] tasks = gitWorkItemList.Value.Select(gitWorkItem => PerformWorkItem(gitWorkItem, workItemList, levelNumber, config)).ToArray();

            foreach (Task task in tasks)
            {
                await task;
            }
        }

        private static async Task PerformWorkItem(GitWorkItem gitWorkItem, IDocumentWorkItemList workItemList, int levelNumber, Config config)
        {
            WriteLine($"{Thread.CurrentThread.ManagedThreadId,2} {levelNumber,2} {gitWorkItem.Id,5} {gitWorkItem.Fields.Title}");

            if (gitWorkItem.Fields.WorkItemType == "Task-Validation")
            {
                return;
            }

            List<int> childList = new List<int>();
            List<int> pullRequestList = new List<int>();

            foreach (GitWorkItemRelation relation in gitWorkItem.Relations)
            {
                if (relation.Attributes.Name is "Child" or "Pull Request")
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
                Task<string>[] tasks = pullRequestList.OrderBy(rr => rr)
                    .Select(pullRequestId => HttpTools.GetPullRequestById(pullRequestId, config.Token))
                    .ToArray();

                foreach (Task<string> task in tasks)
                {
                    string result = CustJsonSerializer.FormatJson(task.Result);
                    GitPullRequest pullRequest = JsonSerializer.Deserialize<GitPullRequest>(result);

                    workItem.AddPullRequest(new DocumentPullRequest(pullRequest));
                }
            }

            if (childList.Count > 0)
            {
                await PerformWorkItems(childList.ToArray(), workItem.SubItems, levelNumber + 1, config);
            }
        }
    }
}
