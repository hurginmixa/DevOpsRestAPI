﻿using System.Collections.Generic;
using System.Text;
using Test01.DocumentClasses;

namespace Test01
{
    public static class Tools
    {
        public static string JoinToString<T>(IEnumerable<T> enumerable, string delimiter)
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

        public static IEnumerable<string> GetUniquePath(IDocumentWorkItemList workItemList)
        {
            HashSet<string> hashSet = new HashSet<string>();

            void SearchLevel(IDocumentWorkItemList levelList)
            {
                foreach (DocumentWorkItem workItem in levelList.GetWorkItems())
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