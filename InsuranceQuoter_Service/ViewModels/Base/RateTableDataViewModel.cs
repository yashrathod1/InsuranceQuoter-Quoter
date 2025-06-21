namespace InsuranceQuoter_Service.ViewModels;

public class RateTableDataViewModel
{
    public int SheetNo { get; set; }
    public int Term { get; set; }
    public string? Gender { get; set; }
    public int MinimumFaceAmount { get; set; }
    public int MaximumFaceAmount { get; set; }
    public int PolicyFee { get; set; }
    public string? HealthClass { get; set; }
    public int MinimumAge { get; set; }
    public int MaximumAge { get; set; }
    public int StartingRow { get; set; }
    public int Column { get; set; }
}
