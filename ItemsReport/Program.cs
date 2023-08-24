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
using CommonCode.DocumentClasses.SerializeClasses;
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

            ReadIds(config);

            DocumentWorkItemList workItemList = new DocumentWorkItemList();

            await PerformWorkItems(workItemNumbers: config.Ids, workItemList: workItemList, levelNumber: 1, config: config);

            RemoveDuplicateItems(workItemList);

            WriteLine($"Printing, {stopwatch.Elapsed.TotalMilliseconds}");

            IEnumerable<IDocumentWorkItem> filteredList = !config.Filter.IsFiltered ? workItemList : workItemList.Where(it => it.GetFullPullRequestList().Any(pr => GetPredicate(pr.Request, config)));

            PrinterHtml.Print(new DocumentWorkItemList(filteredList), config);

            WriteLine($"Complete, {stopwatch.Elapsed.TotalMilliseconds}");

            DocumentWorkItemData[] data = workItemList.GetData();

            string json = CustJsonSerializer.FormatJson(JsonSerializer.Serialize(data));

            await File.WriteAllTextAsync(PPath.GetExeDirectory() / config.CacheDataFile, json);

            stopwatch.Stop();
        }

        private static void ReadIds(Config config)
        {
            string cacheDataFilePath = PPath.GetExeDirectory() / config.CacheDataFile;
            string json = File.Exists(cacheDataFilePath) ? File.ReadAllText(cacheDataFilePath) : "[]";
            DocumentWorkItemData[] itemDatas = JsonSerializer.Deserialize<DocumentWorkItemData[]>(json);

            int[] allIds = config.Ids.Union(config.OldIds).ToArray();

            HashSet<int> cachedIds = allIds.Intersect(itemDatas.Select(d => d.Id)).ToHashSet();

            itemDatas.Where(d => cachedIds.Contains(d.Id));
        }

        private static void CombineIds(int[] ids, int[] oldIds, int[] cacheIds)
        {

        }

        private static bool GetPredicate(DocumentPullRequest pr, Config config)
        {
            Config.FilterClass filter = config.Filter;

            if (!(pr.CloseDate >= filter.StartDate && pr.CloseDate <= filter.EndDate))
            {
                return false;
            }

            if (filter.SelectedBranchPaths.Length == 0)
            {
                return true;
            }

            return filter.SelectedBranchPaths.Contains(pr.TargetRefName);
        }

        private static void RemoveDuplicateItems(IDocumentWorkItemList workItemList)
        {
            HashSet<IDocumentWorkItem> items = new HashSet<IDocumentWorkItem>(workItemList);
            HashSet<IDocumentWorkItem> itemsToDelete = new HashSet<IDocumentWorkItem>();

            void Rr(IDocumentWorkItemList list)
            {
                foreach (IDocumentWorkItem workItem in list)
                {
                    if (items.Contains(workItem))
                    {
                        itemsToDelete.Add(workItem);
                    }

                    Rr(workItem.SubItems);
                }
            }

            foreach (IDocumentWorkItem workItem in workItemList)
            {
                Rr(workItem.SubItems);
            }


            string duplicateNumbers = itemsToDelete.Select(i => i.Id).JoinToString(", ");
            WriteLine(duplicateNumbers);

            foreach (IDocumentWorkItem workItem in itemsToDelete)
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
            WriteLine($"{Thread.CurrentThread.ManagedThreadId,2} {levelNumber,2} {gitWorkItem.Id,5} {gitWorkItem.Fields.WorkItemType} {gitWorkItem.Fields.Title}");

            List<int> childList = new List<int>();
            List<int> pullRequestList = new List<int>();

            foreach (GitWorkItemRelation relation in gitWorkItem.Relations)
            {
                if (relation.Attributes.Name is "Child" or "Pull Request" or "Fixed in Commit")
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

            if (gitWorkItem.Fields.IsTaskValidation && childList.Count == 0 && pullRequestList.Count == 0)
            {
                return;
            }

            DocumentWorkItem workItem = new DocumentWorkItem(gitWorkItem);
            workItemList.AddWorkItem(workItem);

            if (pullRequestList.Count > 0)
            {
                Task<string>[] pullRequestTasks = pullRequestList.OrderBy(rr => rr)
                    .Select(pullRequestId => HttpTools.GetPullRequestById(pullRequestId, config.Token))
                    .ToArray();

                foreach (Task<string> pullRequestTask in pullRequestTasks)
                {
                    string result = CustJsonSerializer.FormatJson(pullRequestTask.Result);
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
