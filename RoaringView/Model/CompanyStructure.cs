using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RoaringView.Model
{
    public class CompanyStructure
    {
        [Key]
        public int CompanyStructureId { get; set; }

        // Foreign key for Company (assuming this is the subsidiary or the actual company)
        [ForeignKey("Company")]
        public int CompanyId { get; set; }

        

        [ForeignKey("MotherCompany")]
        public int? MotherCompanyId { get; set; }


        public int CompanyLevel { get; set; }
        public double OwnedPercentage { get; set; }

    }
}
