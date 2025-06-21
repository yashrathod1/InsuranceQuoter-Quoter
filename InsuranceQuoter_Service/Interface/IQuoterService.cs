using InsuranceQuoter_Service.ViewModels;
using Microsoft.AspNetCore.Http;

namespace InsuranceQuoter_Service.Interface;

public interface IQuoterService
{
    Task<QuoteResultVIewModel?> GetQuoteAsync(UserInputViewModel input);

    Task<List<MultipleQuoteResultViewModel>> GetQuotesForMultipleCompaniesAsync(UserInputViewModel input);
}
