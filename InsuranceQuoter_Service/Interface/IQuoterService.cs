using InsuranceQuoter_Service.ViewModels;
using Microsoft.AspNetCore.Http;

namespace InsuranceQuoter_Service.Interface;

public interface IQuoterService
{
    Task<List<QuoteResultVIewModel>> GetQuoteAsync(UserInputViewModel input);
}
