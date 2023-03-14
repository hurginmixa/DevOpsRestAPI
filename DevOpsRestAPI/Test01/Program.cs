using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace DeviceProfileSample
{
    internal class Program
    {
        private static readonly string PersonalAccessToken = GetPat();
        private const string Organization = "AzCamtek";
        private const string Project = "Falcon";

        #region private class WorkItem

        private class WorkItem
        {
            private readonly int _id;
            private readonly string _workItemType;
            private readonly string _state;
            private readonly string _title;
           
            private readonly WorkItemList _workItemList = new WorkItemList();
            private readonly List<PullRequest> _pullRequestList = new List<PullRequest>();

            public WorkItem(int id, string workItemType, string state, string title)
            {
                _id = id;
                _workItemType = workItemType;
                _state = state;
                _title = title;
            }

            public IWorkItemList SubItems => _workItemList;

            public IEnumerable<PullRequest> PullRequestList => _pullRequestList;

            public IEnumerable<PullRequest> GetFullPullRequestList()
            {
                HashSet<PullRequest> pullRequests = new HashSet<PullRequest>();

                foreach (var pullRequest in PullRequestList)
                {
                    pullRequests.Add(pullRequest);
                }

                foreach (var subItem in SubItems.GetWorkItems())
                {
                    foreach (var pullRequest in subItem.PullRequestList)
                    {
                        pullRequests.Add(pullRequest);
                    }
                }

                return pullRequests;
            }


            public void AddPullRequest(PullRequest pullRequest)
            {
                _pullRequestList.Add(pullRequest);
            }

            public int Id => _id;

            public string WorkItemType => _workItemType;

            public string State => _state;

            public string Title => _title;
        }

        #endregion

        #region private class PullRequest

        private class PullRequest
        {
            private readonly int _id;
            private readonly string _status;
            private readonly string _targetRefName;

            public PullRequest(int id, string status, string targetRefName)
            {
                _id = id;
                _status = status;
                _targetRefName = targetRefName;
            }

            public int Id => _id;

            public string Status => _status;

            public string TargetRefName => _targetRefName;

            protected bool Equals(PullRequest other)
            {
                return _id == other._id && _status == other._status && _targetRefName == other._targetRefName;
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != this.GetType()) return false;
                return Equals((PullRequest) obj);
            }

            public override int GetHashCode()
            {
                return _id;
            }
        }

        #endregion

        #region private interface IWorkItemList

        private interface IWorkItemList
        {
            void AddWorkItem(WorkItem item);

            IEnumerable<WorkItem> GetWorkItems();
        }

        #endregion

        #region private class WorkItemList : IWorkItemList

        private class WorkItemList : IWorkItemList
        {
            private readonly List<WorkItem> _list = new List<WorkItem>();

            public void AddWorkItem(WorkItem item)
            {
                _list.Add(item);
            }

            public IEnumerable<WorkItem> GetWorkItems() => _list;
        }

        #endregion

        static void Main()
        {
            var workItems = new int[]
            {
                34598, 39309, 39559, 39378, 39347, 39523, 39319,
                39967, 39926, 39862, 35781, 39346, 38358,
                38280, 37192, 38836, 37787, 40044,
            };

            WorkItemList workItemList = new WorkItemList();

            PerformWorkItems(workItems, workItemList);

            Console.WriteLine("Printing");

            Print(workItemList);

            Console.WriteLine("Complete");
        }

        private static IEnumerable<string> GetUniquePath(IWorkItemList workItemList)
        {
            HashSet<string> hashSet = new HashSet<string>();

            void SearchLevel(IWorkItemList levelList)
            {
                foreach (WorkItem workItem in levelList.GetWorkItems())
                {
                    foreach (PullRequest request in workItem.PullRequestList)
                    {
                        hashSet.Add(request.TargetRefName);
                    }

                    SearchLevel(workItem.SubItems);
                }
            }

            SearchLevel(workItemList);

            return hashSet;
        }

        private static void Print(IWorkItemList workItemList)
        {
            string[] paths = GetUniquePath(workItemList).OrderBy(t => t).ToArray();

            using (TextWriter textWriter = new StreamWriter(@"c:\temp\mixa.csv"))
            {
                textWriter.Write("Id,Type,State,Title");
                foreach (string path in paths)
                {
                    textWriter.Write($",{path}");
                }
                textWriter.WriteLine();

                #region void PrintPullRequestList(IEnumerable<PullRequest> pullRequestList)

                void PrintPullRequestList(IEnumerable<PullRequest> pullRequestList)
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

                            PullRequest[] pullRequests = pullRequestList.Where(pp => pp.TargetRefName == path).ToArray();
                            if (pullRequests.Length != 0)
                            {
                                sb.Append(JoinToString(pullRequests.Select(p => $"{p.Id}-{p.Status}"), ":"));
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

                #region void PrintLevel(IWorkItemList levelList, int levelNumber)

                void PrintLevel(IWorkItemList levelList, int levelNumber)
                {
                    foreach (WorkItem workItem in levelList.GetWorkItems())
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
        }

        private static void PerformWorkItems(IEnumerable<int> workItems, IWorkItemList workItemList)
        {
            string responseBody = GetWorkItemListByIds(workItems.OrderBy(rr => rr)).Result;
            GitWorkItemList gitWorkItemList = JsonSerializer.Deserialize<GitWorkItemList>(responseBody);

            foreach (GitWorkItem gitWorkItem in gitWorkItemList.Value)
            {
                Console.WriteLine($"{gitWorkItem.Id} {gitWorkItem.Fields.Title}");

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

                WorkItem workItem = new WorkItem(id: gitWorkItem.Id, workItemType: gitWorkItem.Fields.WorkItemType, state: gitWorkItem.Fields.State, title: gitWorkItem.Fields.Title);
                workItemList.AddWorkItem(workItem);

                if (pullRequestList.Count > 0)
                {
                    foreach (int pullRequestId in pullRequestList.OrderBy(rr => rr))
                    {
                        string result = GetPullRequestById(pullRequestId).Result;
                        GitPullRequest pullRequest = JsonSerializer.Deserialize<GitPullRequest>(result);

                        workItem.AddPullRequest(new PullRequest(pullRequest.Id, pullRequest.Status, pullRequest.TargetRefName.Replace("refs/heads/", "")));
                    }
                }

                if (childList.Count > 0)
                {
                    PerformWorkItems(childList.ToArray(), workItem.SubItems);
                }
            }
        }

        private static async Task<string> GetPullRequestById(int id)
        {
            var uri = $"https://dev.azure.com/{Organization}/_apis/git/pullrequests/{id}?api-version=7.1-preview.0";

            return await GetStringByUri(uri);
        }

        private static async Task<string> GetWorkItemListByIds(IEnumerable<int> workItems)
        {
            var items = JoinToString(workItems, ",");

            var uri = $"https://dev.azure.com/{Organization}/{Project}/_apis/wit/workitems?ids={items}&$expand=all&api-version=7.0";

            return await GetStringByUri(uri);
        }

        private static async Task<string> GetStringByUri(string uri)
        {
            var pat = Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes($":{PersonalAccessToken}"));
            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", pat);

                HttpResponseMessage response = await httpClient.GetAsync(uri);
                response.EnsureSuccessStatusCode();

                return await response.Content.ReadAsStringAsync();
            }
        }

        private static string JoinToString<T>(IEnumerable<T> enumerable, string delimiter)
        {
            IEnumerator<T> enumerator = enumerable.GetEnumerator();

            if (!enumerator.MoveNext())
            {
                return string.Empty;
            }

            StringBuilder builder = new StringBuilder();
            builder.Append(enumerator.Current);

            while (enumerator.MoveNext())
            {
                builder.Append(delimiter);
                builder.Append(enumerator.Current);
            }

            return builder.ToString();
        }

        private static string GetPat()
        {
            Assembly assembly = Assembly.GetEntryAssembly();
            Debug.Assert(assembly != null);
            
            Uri uri = new Uri(assembly.CodeBase);

            string directoryName = Path.GetDirectoryName(uri.AbsolutePath);
            Debug.Assert(directoryName != null);

            return File.ReadAllText(Path.Combine(directoryName, "PAT.txt"));
        }
    }
}
