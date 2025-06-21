using InsuranceQuoter_Service.CompanyProduct;


namespace InsuranceQuoter_Service.Company.SBI;

public class SBIProductInfo : ProductInfoBase
{
    public override string CompanyName => "SBI";
    public override string ProductName => "SBI Term";
    public override string TermRating => "A";
    public override int MinIssueAge => 18;
    public override decimal MonthlyModalFactor => 0.0875m;
    public override string AgeDetermination => "Exact";
    public override List<string> ExcludedStates => new() { "MH", "MP", "UP" };
    public override List<int> AllowedTerms => new() { 10, 15, 20, 30 };
    public override decimal MinimumFaceAmount => 100000;
    public override decimal MaximumFaceAmount => 9999999;

    
}