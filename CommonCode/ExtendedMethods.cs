using System.Linq;
using CommonCode.DocumentClasses.SerializeClasses;

namespace CommonCode
{
    public static class ExtendedMethods
    {
        public static int[] Ids(this DocumentWorkItemData[] cachedData) => cachedData.Select(selector: i => i.Id).ToArray();
    }
}