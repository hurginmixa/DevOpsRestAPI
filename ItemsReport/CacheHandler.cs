using System.IO;
using System.Text.Json;
using CommonCode;
using CommonCode.DocumentClasses;
using CommonCode.DocumentClasses.SerializeClasses;

namespace ItemsReport
{
    public static class CacheHandler
    {
        public static DocumentWorkItemData[] ReadFromCache(Config config)
        {
            string cacheDataFilePath = PPath.GetExeDirectory() / config.CacheDataFile;
            string json = File.Exists(cacheDataFilePath) ? File.ReadAllText(cacheDataFilePath) : "[]";
            DocumentWorkItemData[] itemDatas = JsonSerializer.Deserialize<DocumentWorkItemData[]>(json);
            return itemDatas;
        }

        public static void SaveToCache(DocumentWorkItemList workItemList, Config config)
        {
            DocumentWorkItemData[] data = workItemList.GetData();

            string json = CustJsonSerializer.FormatJson(JsonSerializer.Serialize(data));

            File.WriteAllText(PPath.GetExeDirectory() / config.CacheDataFile, json);
        }
    }
}