using System;

namespace CommonCode.DocumentClasses.SerializeClasses
{
    public class DocumentPullRequestData
    {
        public int Id { get; set; }

        public string Status { get; set; }

        public string TargetRefName { get; set; }

        public DateTime CloseDate { get; set; }

        public string CreateBy { get; set; }
    }
}