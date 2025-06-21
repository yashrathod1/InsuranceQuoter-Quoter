namespace InsuranceQuoter_Service.CompanyProduct;

public interface IProductInfo
{
    string CompanyName {get; }

    string ProductName { get; }

    string TermRating { get; }

    int MinIssueAge { get; }

    decimal MonthlyModalFactor { get; }

    string AgeDetermination { get; }

    List<string> ExcludedStates { get; }

    int CalculateAge(DateOnly dateOnly);

    bool IsStateAllowed(string stateCode);

}
