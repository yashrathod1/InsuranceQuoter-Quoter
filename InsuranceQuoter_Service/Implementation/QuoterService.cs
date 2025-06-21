using System.Globalization;
using System.Text;
using System.Text.Json;
using CsvHelper;
using InsuranceQuoter_Service.CompanyProduct;
using InsuranceQuoter_Service.Exceptions;
using InsuranceQuoter_Service.Interface;
using InsuranceQuoter_Service.ViewModels;
using Microsoft.AspNetCore.Http;
using OfficeOpenXml;

namespace InsuranceQuoter_Service.Implementation;

public class QuoterService : IQuoterService
{
    #region FindRate

    public static async Task<RateRowViewModel?> LoadAndFindRateAsync(RateSearchViewModel input)
    {
        try
        {
            string serviceProjectPath = Path.Combine(AppContext.BaseDirectory, @"..\..\..\..\InsuranceQuoter_Service");
            string filePath = Path.Combine(serviceProjectPath, "company", input.CompanyName.ToLower(), $"{input.CompanyName.ToLower()}_rates.csv");

            using StreamReader reader = new(filePath);
            using CsvReader csv = new(reader, CultureInfo.InvariantCulture);
            IEnumerable<RateRowViewModel> records = csv.GetRecords<RateRowViewModel>();

            return await Task.FromResult(
                records.FirstOrDefault(r =>
                    r.Gender.Equals(input.Gender, StringComparison.OrdinalIgnoreCase) &&
                    r.HealthClass.Equals(input.HealthClass, StringComparison.OrdinalIgnoreCase) &&
                    r.Term == input.Term &&
                    r.Age == input.Age &&
                    input.FaceAmount >= r.MinimumFaceAmount &&
                    input.FaceAmount <= r.MaximumFaceAmount));
        }
        catch (Exception ex)
        {
            throw new Exception("Error in load and find rate", ex);
        }
    }
    public static async Task<IciciWopRiderRateRowVIewModel?> LoadAndFindRiderRateAsync(IciciWopRiderRateSearchViewModel input)
    {
        try
        {
            string serviceProjectPath = Path.Combine(AppContext.BaseDirectory, @"..\..\..\..\InsuranceQuoter_Service");
            string filePath = Path.Combine(serviceProjectPath, "company", input.CompanyName.ToLower(), $"{input.CompanyName.ToLower()}_wop_rates.csv");

            if (!File.Exists(filePath))
                return null;

            using StreamReader reader = new(filePath);
            using CsvReader csv = new(reader, CultureInfo.InvariantCulture);

            IEnumerable<IciciWopRiderRateRowVIewModel> records = csv.GetRecords<IciciWopRiderRateRowVIewModel>();

            IciciWopRiderRateRowVIewModel? match = records.FirstOrDefault(r =>
                r.Term == input.Term &&
                r.Age == input.Age &&
                r.TobaccoUse == input.TobaccoUse);

            return await Task.FromResult(match);
        }
        catch (Exception ex)
        {
            throw new Exception("Error in load and find rate of rider", ex);
        }
    }

    public static async Task<decimal?> LoadCRRiderRateAsync(string company)
    {
        try
        {
            string serviceProjectPath = Path.Combine(AppContext.BaseDirectory, @"..\..\..\..\InsuranceQuoter_Service");
            string filePath = Path.Combine(serviceProjectPath, "company", company.ToLower(), $"{company.ToLower()}_cr_rates.csv");

            if (!File.Exists(filePath))
                return null;

            using StreamReader reader = new(filePath);
            using CsvReader csv = new(reader, CultureInfo.InvariantCulture);

            IEnumerable<CRRiderRateRowViewModel> records = csv.GetRecords<CRRiderRateRowViewModel>();

            CRRiderRateRowViewModel? crRow = records.FirstOrDefault();
            return await Task.FromResult(crRow?.RatePerThousand);
        }
        catch (Exception ex)
        {
            throw new Exception("Error in load and find rate of child rider", ex);
        }
    }

    public static async Task<int?> GetMaxAgeFromSheetAsync(string company, int term, string healthClass, string gender)
    {
        string path = Path.Combine(AppContext.BaseDirectory, @"..\..\..\..\InsuranceQuoter_Service",
            "company", company.ToLower(), $"{company.ToLower()}_sheets.csv");

        if (!File.Exists(path))
            return null;

        using StreamReader reader = new(path);
        using CsvReader csv = new(reader, CultureInfo.InvariantCulture);

        IEnumerable<SheetDefinitionRow> records = csv.GetRecords<SheetDefinitionRow>();

        SheetDefinitionRow? match = records.FirstOrDefault(r =>
            r.Term == term &&
            r.Gender.Equals(gender, StringComparison.OrdinalIgnoreCase) &&
            r.HealthClass.Equals(healthClass, StringComparison.OrdinalIgnoreCase));

        return await Task.FromResult(match?.MaximumAge);
    }

    #endregion

    #region Validate
    private static async Task<List<string>> ValidateInput(UserInputViewModel input, ProductInfoBase product, int age)
    {
        try
        {
            List<string> errors = new();

            //1 Minmium age validation
            if (age < product.MinIssueAge)
                errors.Add($"Minimum age for {product.CompanyName} is {product.MinIssueAge}, but given age is {age}");

            //2 Term Validation
            if (!product.IsTermAllowed(input.Term))
                errors.Add($"Term '{input.Term}' is invalid for {product.CompanyName}. Allowed: {string.Join(",", product.AllowedTerms)}");

            //3 Health class and MaxAge as per healthclass and the termm
            string originalHealthClass = input.HealthClass;
            if (!product.TryNormalizeHealthClass(originalHealthClass, input.TobaccoUse, out string? normalizedHealthClass))
            {
                errors.Add($"Invalid HealthClass '{originalHealthClass}' Allowed: PP, P, RP, R");
            }
            else
            {
                input.HealthClass = normalizedHealthClass!;
                int? maxAge = await GetMaxAgeFromSheetAsync(input.CompanyName, input.Term, input.HealthClass, input.Gender);
                if (maxAge.HasValue && age > maxAge.Value)
                {
                    errors.Add($"Maximum issue age for term {input.Term}, health class '{originalHealthClass}', and gender '{input.Gender}' is {maxAge.Value}, but provided age is {age}.");
                }
            }

            //4 State validation
            string originalStateName = input.State?.Trim() ?? "";
            if (!IndianStateMapper.TryGetStateCode(originalStateName, out string? stateCode) || string.IsNullOrWhiteSpace(stateCode))
            {
                errors.Add($"Invalid state name '{originalStateName}'");
            }
            else
            {
                input.State = stateCode;
                if (!product.IsStateAllowed(stateCode))
                {
                    List<string> excludedFullNames = product.ExcludedStates
                        .Select(code => IndianStateMapper.StateMap.FirstOrDefault(x => x.Value == code).Key)
                        .Where(name => !string.IsNullOrEmpty(name))
                        .ToList();

                    errors.Add($"State '{originalStateName}' is not allowed for {product.CompanyName}. NotAllowed: {string.Join(", ", excludedFullNames)}");
                }
            }

            //5 Face Amount validation
            if (!product.IsFaceAmountAllowed(input.FaceAmount))
                errors.Add($"Face amount must be between {product.MinimumFaceAmount} and {product.MaximumFaceAmount}");

            //6 Child Rider validation
            if (input.ChildRiderAmount.HasValue && input.ChildRiderAmount.Value > 0)
            {
                if (!product.SupportsChildRider)
                {
                    errors.Add($"{product.CompanyName} does not support Child Rider.");
                }
                else if (product.MinChildRiderAmount.HasValue && product.MaxChildRiderAmount.HasValue)
                {
                    if (input.ChildRiderAmount < product.MinChildRiderAmount || input.ChildRiderAmount > product.MaxChildRiderAmount)
                    {
                        errors.Add($"ChildRiderAmount must be between {product.MinChildRiderAmount} and {product.MaxChildRiderAmount} for {product.CompanyName}.");
                    }
                }

                decimal? crRate = await LoadCRRiderRateAsync(input.CompanyName);
                if (crRate == null)
                    errors.Add($"Child Rider rate not found for company {input.CompanyName}.");
            }

            //7 WOP Rider validation
            if (input.WOP)
            {
                string filePath = Path.Combine(AppContext.BaseDirectory, @"..\..\..\..\InsuranceQuoter_Service", "company", input.CompanyName.ToLower(), $"{input.CompanyName.ToLower()}_wop_rates.csv");
                if (!File.Exists(filePath))
                {
                    errors.Add($"WOP Rider not supported by {product.CompanyName}.");
                }
                else
                {
                    IciciWopRiderRateRowVIewModel? wopMatch = await LoadAndFindRiderRateAsync(new IciciWopRiderRateSearchViewModel
                    {
                        CompanyName = input.CompanyName,
                        Age = age,
                        Term = input.Term,
                        TobaccoUse = input.TobaccoUse
                    });

                    if (wopMatch == null)
                        errors.Add($"WOP Rider is not available for Age {age}, Term {input.Term}, TobaccoUse: {input.TobaccoUse}.");
                }
            }
            return errors;
        }
        catch (Exception ex)
        {
            throw new Exception("Error in validating the input", ex);
        }
    }

    #endregion

    #region QuoteCalculation
    public async Task<QuoteResultVIewModel?> GetQuoteAsync(UserInputViewModel input)
    {
        ProductInfoBase product = ProductInfoFactory.GetProductInfo(input.CompanyName);

        int age = product.CalculateAge(input.DateOfBirth);
        var errors = await ValidateInput(input, product, age);
        if (errors.Any())
            throw new ValidationErrorsException(errors);

        RateRowViewModel? rateRow = await product.LoadBaseRateAsync(new RateSearchViewModel
        {
            CompanyName = input.CompanyName,
            Term = input.Term,
            Age = age,
            Gender = input.Gender,
            HealthClass = input.HealthClass,
            FaceAmount = input.FaceAmount
        });

        if (rateRow == null)
            return null;

        decimal riderPremium = 0;

        if (input.WOP)
        {
            var wopRate = await product.LoadWopRiderRateAsync(age, input.Term, input.TobaccoUse);
            if (wopRate.HasValue)
                riderPremium += product.CalculateWopPremium(input.FaceAmount, wopRate.Value);
        }

        if (input.ChildRiderAmount.HasValue && input.ChildRiderAmount.Value > 0)
        {
            var crRate = await product.LoadCrRiderRateAsync();
            if (crRate.HasValue)
                riderPremium += product.CalculateCrPremium(input.ChildRiderAmount.Value, crRate.Value);
        }

        decimal baseAnnual = product.CalculateBaseAnnualPremium(input.FaceAmount, rateRow.RatePerThousand);
        decimal baseMonthly = product.CalculateBaseMonthlyPremium(input.FaceAmount, rateRow.RatePerThousand);
        decimal annualPremium = product.CalculateAnnualPremium(input.FaceAmount, rateRow.RatePerThousand, rateRow.PolicyFee + riderPremium);
        decimal monthlyPremium = product.CalculateMonthlyPremium(input.FaceAmount, rateRow.RatePerThousand, rateRow.PolicyFee + riderPremium);

        return new QuoteResultVIewModel
        {
            RatePerThousand = rateRow.RatePerThousand,
            CompanyName = product.CompanyName,
            ProductName = product.ProductName,
            Rating = product.TermRating,
            BaseAnnualPremium = baseAnnual,
            BaseMonthlyPremium = baseMonthly,
            RiderPremium = riderPremium,
            AnnualPremium = annualPremium,
            MonthlyPremium = monthlyPremium
        };
    }

    public async Task<List<MultipleQuoteResultViewModel>> GetQuotesForMultipleCompaniesAsync(UserInputViewModel input)
    {

        List<MultipleQuoteResultViewModel> results = new();
        string[]? companyList = input.CompanyName.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        foreach (string company in companyList)
        {
            UserInputViewModel? clonedInput = JsonSerializer.Deserialize<UserInputViewModel>(
                JsonSerializer.Serialize(input)
            );

            clonedInput!.CompanyName = company;

            MultipleQuoteResultViewModel response = new() { CompanyName = company.ToUpper() };
            try
            {
                QuoteResultVIewModel? quote = await GetQuoteAsync(clonedInput);
                response.Quote = quote;
            }
            catch (CompanyNotFoundException ex)
            {
                response.Errors.Add(ex.Message);
            }
            catch (ValidationErrorsException ex)
            {
                response.Errors.AddRange(ex.Errors);
            }

            results.Add(response);
        }
        return results;
    }

    #endregion
}