namespace InsuranceQuoter_Service.ViewModels;

public class MultipleQuoteResultViewModel
{
    public string CompanyName { get; set; } = string.Empty;

    public QuoteResultVIewModel? Quote { get; set; }

    public List<string> Errors { get; set; } = new();
}
