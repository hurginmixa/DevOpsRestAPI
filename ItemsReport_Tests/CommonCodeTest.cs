using System.Linq;
using CommonCode;
using Xunit;

namespace ItemsReport_Tests
{
    public class CommonCodeTest
    {
        [Theory]

        [InlineData(new[] {3, 5, 6, 7}, new[] {1, 2, 3, 5, 6}, new[] {1, 2, 3, 4}, new[] {3, 5, 6, 7}, new[] {1, 2})]

        [InlineData(new[] { 3, 5, 6, 7 }, new[] { 1, 4 }, new[] { 2, 4, 5 }, new[] { 1, 3, 5, 6, 7 }, new[] { 4 })]

        [InlineData(new[] { 3, 5, 6, 7 }, new int[] { }, new int[] { }, new[] { 3, 5, 6, 7 }, new int[] { })]

        [InlineData(new[] { 3, 5, 6, 7 }, new int[] { }, new[] { 1, 2 }, new[] { 3, 5, 6, 7 }, new int[] { })]

        [InlineData(new int[] { }, new[] { 1, 3, 5, 6, 7 }, new[] { 1, 2 }, new[] { 3, 5, 6, 7 }, new[] { 1 })]

        [InlineData(new[] { 1 }, new[] { 3, 5, 6, 7 }, new[] { 1, 3, 5, 6, 8 }, new[] { 1, 7 }, new[] { 3, 5, 6 })]

        public void CombineIdsTest(int[] ids, int[] oldIds, int[] cachedIds, int[] expectIdsToReading, int[] expectStillInCacheIds)
        {
            Tools.CombineIds(ids, oldIds, cachedIds, out int[] idsToReading, out int[] stillInCache);

            Assert.Equal(actual: idsToReading.OrderBy(n => n), expected: expectIdsToReading.OrderBy(n => n));
            
            Assert.Equal(actual: stillInCache.OrderBy(n => n), expected: expectStillInCacheIds.OrderBy(n => n));
        }
    }
}
