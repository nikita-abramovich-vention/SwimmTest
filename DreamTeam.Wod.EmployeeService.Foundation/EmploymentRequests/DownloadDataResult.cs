namespace DreamTeam.Wod.EmployeeService.Foundation.EmploymentRequests
{
    public sealed class DownloadDataResult
    {
        public bool IsSuccessful { get; }

        public int AffectedEntitiesCount { get; }


        private DownloadDataResult(bool isSuccessful, int affectedEntitiesCount)
        {
            IsSuccessful = isSuccessful;
            AffectedEntitiesCount = affectedEntitiesCount;
        }


        public static DownloadDataResult CreateSuccessful(int affectedEntitiesCount)
        {
            return new DownloadDataResult(true, affectedEntitiesCount);
        }

        public static DownloadDataResult CreateUnsuccessful()
        {
            return new DownloadDataResult(false, 0);
        }
    }
}