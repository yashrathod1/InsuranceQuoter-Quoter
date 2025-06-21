namespace InsuranceQuoter_Service.Exceptions;

public class CompanyNotFoundException : Exception
{
    public CompanyNotFoundException(string companyName) 
        : base($"Company {companyName} is not supported") {}
}
