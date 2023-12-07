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
        private readonly string _cacheDataFilePath;

        public CacheHandler(Config config)
        {
            _cacheDataFilePath = Path.GetFullPath(config.CacheDataFile);
        }

        public DocumentWorkItemData[] ReadFromCache()
        {
            string json = File.Exists(_cacheDataFilePath) ? File.ReadAllText(_cacheDataFilePath) : "[]";
            DocumentWorkItemData[] itemDatas = JsonSerializer.Deserialize<DocumentWorkItemData[]>(json);
            return itemDatas;
        }

        public void SaveToCache(DocumentWorkItemList workItemList)
        {
            DocumentWorkItemData[] data = workItemList.GetData();

            string json = CustJsonSerializer.FormatJson(JsonSerializer.Serialize(data));

            File.WriteAllText(_cacheDataFilePath, json);
        }
    }
}