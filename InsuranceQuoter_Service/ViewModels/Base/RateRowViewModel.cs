namespace InsuranceQuoter_Service.ViewModels;

public class RateRowViewModel
{
    public string Gender { get; set; } = null!;
    public int Term { get; set; }
    public int Age { get; set; }
    public string HealthClass { get; set; } = null!;
    public decimal RatePerThousand { get; set; }
    public decimal PolicyFee { get; set; }
    public decimal MinimumFaceAmount { get; set; }
    public decimal MaximumFaceAmount { get; set; }
}
