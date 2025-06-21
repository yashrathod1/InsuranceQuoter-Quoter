namespace InsuranceQuoter_Service.ViewModels;

public class HdfcWopSheetDataViewModel
{
    public int SheetNo { get; set; }
    public int StartingRow { get; set; }
    public int Column { get; set; }

    public int MinimumAge { get; set; }
    public int MaximumAge { get; set; }
}
