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

namespace ItemsReport.Services
{
    public class ReportService
    {
        private readonly ICacheHandler _cacheHandler;
        private readonly Config _config;

        public ReportService(ICacheHandler cacheHandler, Config config)
        {
            _cacheHandler = cacheHandler;
            _config = config;
        }

        public async Task<string> GenerateReportAsync()
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            DocumentWorkItemData[] cachedData = _cacheHandler.ReadFromCache();

            Tools.CombineIds(ids: _config.Ids, oldIds: _config.OldIds, cachedIds: cachedData.Ids(), idsToReading: out int[] idsToReading, idsStillInCache: out int[] idsStillInCache);

            DocumentWorkItemList workItemList = new DocumentWorkItemList();

            workItemList.AddFromCache(cachedData, idsStillInCache);

            await PerformWorkItems(workItemNumbers: idsToReading, workItemList: workItemList, levelNumber: 1);

            RemoveDuplicateItems(workItemList: workItemList);

            Console.WriteLine($"Printing, {stopwatch.Elapsed.TotalMilliseconds}");

            IEnumerable<IDocumentWorkItem> filteredList = !_config.Filter.IsFiltered ? workItemList : workItemList.Where(predicate: it => it.GetFullPullRequestList().Any(predicate: pr => GetPredicate(pr: pr.Request, config: _config)));

            string htmlOutput = PrinterHtml.PrintToString(workItemList: new DocumentWorkItemList(list: filteredList), config: _config);

            Console.WriteLine($"Complete, {stopwatch.Elapsed.TotalMilliseconds}");

            _cacheHandler.SaveToCache(workItemList: workItemList);

            stopwatch.Stop();

            return htmlOutput;
        }

        private bool GetPredicate(DocumentPullRequest pr, Config config)
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

        private void RemoveDuplicateItems(IDocumentWorkItemList workItemList)
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
            Console.WriteLine(duplicateNumbers);

            foreach (IDocumentWorkItem workItem in itemsToDelete)
            {
                workItemList.RemoveItem(workItem);
            }
        }

        private async Task PerformWorkItems(IEnumerable<int> workItemNumbers, IDocumentWorkItemList workItemList, int levelNumber)
        {
            string jsonResponseBody = CustJsonSerializer.FormatJson(await HttpTools.GetWorkItemListByIds(workItemNumbers.Distinct().OrderBy(rr => rr), _config.Token));
            GitWorkItemList gitWorkItemList = JsonSerializer.Deserialize<GitWorkItemList>(jsonResponseBody);

            Task[] tasks = gitWorkItemList.Value.Select(gitWorkItem => PerformWorkItem(gitWorkItem, workItemList, levelNumber)).ToArray();

            await Task.WhenAll(tasks);
        }

        private async Task PerformWorkItem(GitWorkItem gitWorkItem, IDocumentWorkItemList workItemList, int levelNumber)
        {
            Console.WriteLine($"{Thread.CurrentThread.ManagedThreadId,2} {levelNumber,2} {gitWorkItem.Id,5} {gitWorkItem.Fields.WorkItemType} {gitWorkItem.Fields.Title}");

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
                    .Select(pullRequestId => HttpTools.GetPullRequestById(pullRequestId, _config.Token))
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
                await PerformWorkItems(childList.ToArray(), workItem.SubItems, levelNumber + 1);
            }
        }
    }
}
