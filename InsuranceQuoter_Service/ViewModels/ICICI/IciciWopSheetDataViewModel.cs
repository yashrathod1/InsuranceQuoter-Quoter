namespace InsuranceQuoter_Service.ViewModels;

public class IciciWopSheetDataViewModel
{
    public int SheetNo { get; set; }
    public int Term { get; set; }
    public bool TobaccoUse { get; set; } 
    public int MinimumAge { get; set; }
    public int MaximumAge { get; set; }
    public int StartingRow { get; set; }
    public int Column { get; set; }
}
