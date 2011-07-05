
namespace HatCMS.Placeholders
{
    public class FileLibraryAggregatorData
    {
        private int numFilesForOverview = 5;
        public int NumFilesForOverview
        {
            get { return numFilesForOverview; }
            set { numFilesForOverview = value; }
        }

        private int numFilesPerPage = -1;
        public int NumFilesPerPage
        {
            get { return numFilesPerPage; }
            set { numFilesPerPage = value; }
        }
    }
}
