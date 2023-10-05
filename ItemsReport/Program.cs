using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using CommonCode;
using CommonCode.DocumentClasses;
using CommonCode.DocumentClasses.SerializeClasses;
using CommonCode.GitClasses;
using static System.Console;

namespace ItemsReport
{
    public static class Program
    {
        static async Task Main()
        {
            try
            {
                Config config = Config.GetConfig();

                ICacheHandler cacheHandler = new CacheHandler(config: config);

                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();

                DocumentWorkItemData[] cachedData = cacheHandler.ReadFromCache();

                Tools.CombineIds(ids: config.Ids, oldIds: config.OldIds, cachedIds: cachedData.Ids(), idsToReading: out int[] idsToReading, idsStillInCache: out int[] idsStillInCache);

                DocumentWorkItemList workItemList = new DocumentWorkItemList();

                workItemList.AddFromCache(cachedData, idsStillInCache);

                await PerformWorkItems(workItemNumbers: idsToReading, workItemList: workItemList, levelNumber: 1, config: config);

                RemoveDuplicateItems(workItemList: workItemList);

                WriteLine(value: $"Printing, {stopwatch.Elapsed.TotalMilliseconds}");

                IEnumerable<IDocumentWorkItem> filteredList = !config.Filter.IsFiltered ? workItemList : workItemList.Where(predicate: it => it.GetFullPullRequestList().Any(predicate: pr => GetPredicate(pr: pr.Request, config: config)));

                PrinterHtml.Print(workItemList: new DocumentWorkItemList(list: filteredList), config: config);

                WriteLine(value: $"Complete, {stopwatch.Elapsed.TotalMilliseconds}");

                cacheHandler.SaveToCache(workItemList: workItemList);

                stopwatch.Stop();
            }
            catch (Exception e)
            {
                WriteLine(value: e);
                Console.ReadKey();
            }
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

                Task.WaitAll(pullRequestTasks.Cast<Task>().ToArray());

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
