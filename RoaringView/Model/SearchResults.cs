namespace RoaringView.Model
{
    public class SearchResults
    {
        public List<Address> Addresses { get; set; }
        public List<Company> Companies { get; set; }
        public List<CompanyEmployee> CompanyEmployees { get; set; }
        public List<CompanyRating> CompanyRatings { get; set; }
        public List<CompanyStructure> CompanyStructures { get; set; }
        public List<FinancialRecord> FinancialRecords { get; set; }

        public Dictionary<int, string> CompanyNameMap { get; set; }

        public bool HasResults()
        {
            return (Companies?.Any() == true)
                || (CompanyEmployees?.Any() == true)
                || (CompanyRatings?.Any() == true)
                || (Addresses?.Any() == true)
                || (CompanyStructures?.Any() == true)
                || (FinancialRecords?.Any() == true)
                // ... check other entities ...
                ;
        }
    }


}
