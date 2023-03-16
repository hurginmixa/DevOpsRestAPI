using System;
using Test01.GitClasses;

namespace Test01.DocumentClasses
{
    public class DocumentPullRequest
    {
        private readonly int _id;
        private readonly string _status;
        private readonly string _targetRefName;
        private readonly DateTime _closeDate;

        public DocumentPullRequest(GitPullRequest pullRequest)
        {
            _id = pullRequest.Id;
            _status = pullRequest.Status;
            _targetRefName = pullRequest.TargetRefName.Replace("refs/heads/", "");
            _closeDate = pullRequest.ClosedDate.ToLocalTime();
        }

        public int Id => _id;

        public string Status => _status;

        public DateTime CloseDate => _closeDate;

        public string TargetRefName => _targetRefName;

        private bool Equals(DocumentPullRequest other)
        {
            return _id == other._id && _status == other._status && _targetRefName == other._targetRefName;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((DocumentPullRequest) obj);
        }

        public override int GetHashCode()
        {
            return _id;
        }
    }
}