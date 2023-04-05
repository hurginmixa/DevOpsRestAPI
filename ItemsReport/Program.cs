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
        private static readonly int[] WorkItemNumbers =
        {
            40178, 40180, 38967, 39059, 35861, 34516,

            34598, 39309, 39378, 39319,
            39967, 39346, 38358,
            38280, 37192, 38836, 37787, 40044, 33462,
            32807, 40071, 38966,

            40144, 40549,

            40044, 39309, 36757, 36514, 38280, 31765, 34112, 39244, 39926, 37442, 31358, 39031

        };

        static async Task Main()
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            DocumentWorkItemList workItemList = new DocumentWorkItemList();

            await PerformWorkItems(WorkItemNumbers, workItemList, 1);

            RemoveDuplicateItems(workItemList);

            WriteLine($"Printing, {stopwatch.Elapsed.TotalMilliseconds}");

            PrinterHtml.Print(workItemList);

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

        private static async Task PerformWorkItems(IEnumerable<int> workItemNumbers, IDocumentWorkItemList workItemList, int levelNumber)
        {
            string jsonResponseBody = CustJsonSerializer.FormatJson(await HttpTools.GetWorkItemListByIds(workItemNumbers.Distinct().OrderBy(rr => rr)));
            GitWorkItemList gitWorkItemList = JsonSerializer.Deserialize<GitWorkItemList>(jsonResponseBody);

            Task[] tasks = gitWorkItemList.Value.Select(gitWorkItem => PerformWorkItem(gitWorkItem, workItemList, levelNumber)).ToArray();

            foreach (Task task in tasks)
            {
                await task;
            }
        }

        private static async Task PerformWorkItem(GitWorkItem gitWorkItem, IDocumentWorkItemList workItemList, int levelNumber)
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
                    .Select(pullRequestId => HttpTools.GetPullRequestById(pullRequestId))
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
                await PerformWorkItems(childList.ToArray(), workItem.SubItems, levelNumber + 1);
            }
        }
    }
}
