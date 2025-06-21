namespace InsuranceQuoter_Service.ViewModels;

public class QuoteResultVIewModel
{
    public string CompanyName { get; set; } = null!;
    public string ProductName { get; set; } = null!;
    public string Rating { get; set; } = null!;
    public decimal BaseAnnualPremium { get; set; }
    public decimal BaseMonthlyPremium { get; set; }
    public decimal AnnualPremium { get; set; }
    public decimal MonthlyPremium { get; set; }
    public decimal RiderPremium { get; set; }
    public decimal RatePerThousand { get; set;}
}
