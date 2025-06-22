using InsuranceQuoter_Service.CompanyProduct;

namespace InsuranceQuoter_Service.Company.KOTAK;

public class KOTAKProductInfo : ProductInfoBase
{
    public override string CompanyName => "KOTAK";
    public override string ProductName => "KOTAK Term";
    public override string TermRating => "A+";
    public override int MinIssueAge => 18;
    public override decimal MonthlyModalFactor => 0.0875m;
    public override string AgeDetermination => "Exact";
    public override List<string> ExcludedStates => new() { "MH", "MP", "UP" };
    public override List<int> AllowedTerms => new() { 10, 15, 20, 25, 30, 35, 40 };
    public override decimal MinimumFaceAmount => 100000;
    public override decimal MaximumFaceAmount => 10000000;

    public override bool TryNormalizeHealthClass(string healthClass, bool tobaccoUsage, out string? normalized)
    {
        normalized = (healthClass.ToUpper(), tobaccoUsage) switch
        {
            ("PP", false) => "Preferred Plus Non-Tob",
            ("P", false) => "Preferred Non-Tob",
            ("RP", false) => "Standard Plus Non-Tob",
            ("R", false) => "Standard Non-Tob",
            ("RP", true) or ("R", true) => "Standard Tob",
            _ => null
        };

        return normalized != null;
    }

}
