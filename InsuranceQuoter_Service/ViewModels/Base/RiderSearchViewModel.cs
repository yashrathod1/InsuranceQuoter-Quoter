namespace InsuranceQuoter_Service.ViewModels.Base;

public class RiderSearchViewModel
{
    public int Age { get; set; }
    public int Term { get; set; }
    public string Gender { get; set; } = string.Empty;
    public bool TobaccoUse { get; set; }
    public decimal? RiderAmount { get; set; }
    public string State { get; set; } = string.Empty;
}
