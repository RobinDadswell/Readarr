namespace NzbDrone.Core.IndexerSearch.Definitions
{
    public class BookSearchCriteria : SearchCriteriaBase
    {
        public string BookTitle { get; set; }
        public int BookYear { get; set; }
        public string BookIsbn { get; set; }
        public string Disambiguation { get; set; }

        public string BookQuery => GetQueryTitle($"{BookTitle}");

        public override string ToString()
        {
            return $"[{Author.Name} - {BookTitle} ({BookYear})]";
        }
    }
}
