using System.IO;
using System.Text.Json;
using CommonCode;
using CommonCode.DocumentClasses;
using CommonCode.DocumentClasses.SerializeClasses;

namespace ItemsReport
{
    public interface ICacheHandler
    {
        DocumentWorkItemData[] ReadFromCache();

        void SaveToCache(DocumentWorkItemList workItemList);
    }

    public class CacheHandler : ICacheHandler
    {
        private readonly Config _config;

        public CacheHandler(Config config)
        {
            _config = config;
        }

        public DocumentWorkItemData[] ReadFromCache()
        {
            string cacheDataFilePath = PPath.GetExeDirectory() / _config.CacheDataFile;
            string json = File.Exists(cacheDataFilePath) ? File.ReadAllText(cacheDataFilePath) : "[]";
            DocumentWorkItemData[] itemDatas = JsonSerializer.Deserialize<DocumentWorkItemData[]>(json);
            return itemDatas;
        }

        public void SaveToCache(DocumentWorkItemList workItemList)
        {
            DocumentWorkItemData[] data = workItemList.GetData();

            string json = CustJsonSerializer.FormatJson(JsonSerializer.Serialize(data));

            File.WriteAllText(PPath.GetExeDirectory() / _config.CacheDataFile, json);
        }
    }
}