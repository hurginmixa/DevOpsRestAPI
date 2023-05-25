using System.Collections.Generic;
using System.Text;
using CommonCode.DocumentClasses;

namespace CommonCode
{
    public static class Tools
    {
        public static string JoinToString<T>(this IEnumerable<T> enumerable, string delimiter)
        {
            using IEnumerator<T> enumerator = enumerable.GetEnumerator();

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

        public static IEnumerable<string> GetUniqueCommittedPaths(this IDocumentWorkItemList workItemList)
        {
            HashSet<string> hashSet = new HashSet<string>();

            void SearchLevel(IDocumentWorkItemList levelList)
            {
                foreach (IDocumentWorkItem workItem in levelList)
                {
                    foreach (DocumentPullRequest request in workItem.PullRequestList)
                    {
                        hashSet.Add(request.TargetRefName);
                    }

                    SearchLevel(workItem.SubItems);
                }
            }

            SearchLevel(workItemList);

            return hashSet;
        }
    }
}