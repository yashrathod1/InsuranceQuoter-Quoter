namespace InsuranceQuoter_Service.ViewModels;

public class RiderRuleViewModel
{
    public string RiderName { get; set; } = string.Empty;
    public bool IsAvailable { get; set; }
    public bool IsSheetBasedMaxAge { get; set; } = false;
    public int? MaxIssueAge { get; set; }
    public decimal? MinAmount { get; set; }
    public decimal? MaxAmount { get; set; }

}
