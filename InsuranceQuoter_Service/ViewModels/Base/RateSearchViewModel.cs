namespace InsuranceQuoter_Service.ViewModels;

public class RateSearchViewModel
{
    public string CompanyName { get; set; } = string.Empty;
    public string Gender { get; set; } = string.Empty;
    public string HealthClass { get; set; } = string.Empty;
    public int Term { get; set; }
    public int Age { get; set; }
    public decimal FaceAmount { get; set; }


}
