namespace InsuranceQuoter_Service.ViewModels;

public class SheetDefinitionRow
{
    public int SheetNo { get; set; }
    public int Term { get; set; }
    public string Gender { get; set; } = "";
    public decimal MinimumFaceAmount { get; set; }
    public decimal MaximumFaceAmount { get; set; }
    public string HealthClass { get; set; } = "";
    public int MinimumAge { get; set; }
    public int MaximumAge { get; set; }
    public int StartingRow { get; set; }
    public int Column { get; set; }
    public decimal PolicyFee { get; set; }
}