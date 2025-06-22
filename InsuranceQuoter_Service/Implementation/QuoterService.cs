using System.Globalization;
using System.Text;
using System.Text.Json;
using CsvHelper;
using InsuranceQuoter_Service.CompanyProduct;
using InsuranceQuoter_Service.Exceptions;
using InsuranceQuoter_Service.Interface;
using InsuranceQuoter_Service.ViewModels;
using InsuranceQuoter_Service.ViewModels.Base;
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
                string tobaccoText = input.TobaccoUse ? "with Tobacco Use" : "with No Tobacco Use";
                errors.Add($"The combination of HealthClass '{originalHealthClass}' {tobaccoText} is not allowed for {product.CompanyName}.");
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

            var riderSearch = new RiderSearchViewModel
            {
                Age = age,
                Term = input.Term,
                Gender = input.Gender,
                TobaccoUse = input.TobaccoUse,
                RiderAmount = input.ChildRiderAmount,
            };

            // 6. Child Rider validation
            if (input.ChildRiderAmount is > 0)
            {
                var childRiderRule = product.GetRiderRule("Child Rider");
                if (childRiderRule == null || !childRiderRule.IsAvailable)
                {
                    errors.Add($"{product.CompanyName} does not support the Child Rider.");
                }
                else
                {
                    if (childRiderRule.MinAmount.HasValue && childRiderRule.MaxAmount.HasValue)
                    {
                        if (input.ChildRiderAmount < childRiderRule.MinAmount || input.ChildRiderAmount > childRiderRule.MaxAmount)
                        {
                            errors.Add($"Child Rider amount must be between {childRiderRule.MinAmount} and {childRiderRule.MaxAmount}.");
                        }
                    }

                    if (childRiderRule.MaxIssueAge.HasValue && age > childRiderRule.MaxIssueAge.Value)
                    {
                        errors.Add($"Child Rider is allowed up to age {childRiderRule.MaxIssueAge.Value}.");
                    }

                    var crRate = await product.LoadCrRiderRateAsync(riderSearch);
                    if (!crRate.HasValue)
                    {
                        errors.Add($"Child Rider rate not available for {product.CompanyName}.");
                    }
                }
            }
            // 7. Waiver of Premium (WOP) Rider validation
            if (input.WOP)
            {
                var wopRule = product.GetRiderRule("Waiver of Premium");
                if (wopRule == null || !wopRule.IsAvailable)
                {
                    errors.Add($"{product.CompanyName} does not support the Waiver of Premium Rider.");
                }
                else
                {
                    int? maxWopAge = wopRule.IsSheetBasedMaxAge
                        ? await product.GetWopMaxIssueAgeAsync(riderSearch)
                        : wopRule.MaxIssueAge;

                    if (maxWopAge.HasValue && age > maxWopAge.Value)
                    {
                        errors.Add($"Waiver of Premium Rider is allowed up to age {maxWopAge.Value} for term {input.Term}.");
                    }
                }
            }

            // 8. Accidental Death Benefit (ADB) Rider validation
            if (input.ADB)
            {
                var adbRule = product.GetRiderRule("ADB Rider");
                if (adbRule == null || !adbRule.IsAvailable)
                {
                    errors.Add($"{product.CompanyName} does not support the ADB Rider.");
                }
                else
                {
                    if (adbRule.MaxIssueAge.HasValue && age > adbRule.MaxIssueAge.Value)
                    {
                        errors.Add($"ADB Rider is allowed up to age {adbRule.MaxIssueAge.Value}.");
                    }

                    var adbRate = await product.LoadAdbRiderRateAsync(riderSearch);
                    if (!adbRate.HasValue)
                    {
                        errors.Add($"ADB Rider rate not available for {product.CompanyName}.");
                    }
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
    private async Task<QuoteResultVIewModel?> BuildQuoteAsync(UserInputViewModel input, ProductInfoBase product, int age)
    {
        var rateRow = await product.LoadBaseRateAsync(new RateSearchViewModel
        {
            CompanyName = product.CompanyName,
            Term = input.Term,
            Age = age,
            Gender = input.Gender,
            HealthClass = input.HealthClass,
            FaceAmount = input.FaceAmount
        });

        if (rateRow == null)
            return null;

        var riderInput = new RiderSearchViewModel
        {
            Age = age,
            Term = input.Term,
            Gender = input.Gender,
            TobaccoUse = input.TobaccoUse,
            RiderAmount = input.ChildRiderAmount,
            State = input.State
        };

        decimal riderPremium = 0;

        if (input.WOP)
        {
            var wopRate = await product.LoadWopRiderRateAsync(riderInput);
            if (wopRate.HasValue)
                riderPremium += product.CalculateWopPremium(input.FaceAmount, wopRate.Value);
        }

        if (input.ChildRiderAmount is > 0)
        {
            var crRate = await product.LoadCrRiderRateAsync(riderInput);
            if (crRate.HasValue)
                riderPremium += product.CalculateCrPremium(input.ChildRiderAmount.Value, crRate.Value);
        }

        if (input.ADB)
        {
            var adbRate = await product.LoadAdbRiderRateAsync(riderInput);
            if (adbRate.HasValue)
                riderPremium += product.CalculateCrPremium(input.FaceAmount, adbRate.Value);
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

    public async Task<List<QuoteResultVIewModel>> GetQuoteAsync(UserInputViewModel input)
    {
        var results = new List<QuoteResultVIewModel>();

        if (!string.IsNullOrWhiteSpace(input.CompanyName))
        {
            var product = ProductInfoFactory.GetProductInfo(input.CompanyName);
            int age = product.CalculateAge(input.DateOfBirth);

            var validationErrors = await ValidateInput(input, product, age);
            if (validationErrors.Any())
                throw new ValidationErrorsException(validationErrors);

            var quote = await BuildQuoteAsync(input, product, age);
            if (quote != null)
                results.Add(quote);
        }
        else
        {
            foreach (var product in ProductInfoFactory.GetAllProducts())
            {
                // Clone to avoid modifying original input
                var clonedInput = new UserInputViewModel
                {
                    CompanyName = product.CompanyName,
                    DateOfBirth = input.DateOfBirth,
                    Gender = input.Gender,
                    State = input.State,
                    Term = input.Term,
                    FaceAmount = input.FaceAmount,
                    HealthClass = input.HealthClass,
                    TobaccoUse = input.TobaccoUse,
                    WOP = input.WOP,
                    ADB = input.ADB,
                    ChildRiderAmount = input.ChildRiderAmount
                };

                int age = product.CalculateAge(clonedInput.DateOfBirth);
                var validationErrors = await ValidateInput(clonedInput, product, age);

                if (!validationErrors.Any())
                {
                    var quote = await BuildQuoteAsync(clonedInput, product, age);
                    if (quote != null)
                        results.Add(quote);
                }
                else
                {
                    Console.WriteLine($"Skipping {product.CompanyName} due to validation errors: {string.Join(" | ", validationErrors)}");
                }
            }

            if (!results.Any())
            {
                throw new ValidationErrorsException(new List<string>
            {
                "No products match the given criteria."
            });
            }
        }

        // else
        // {
        //     var allValidationErrors = new List<string>();

        //     foreach (var product in ProductInfoFactory.GetAllProducts())
        //     {
        //         // Clone to avoid modifying original input
        //         var clonedInput = new UserInputViewModel
        //         {
        //             CompanyName = product.CompanyName,
        //             DateOfBirth = input.DateOfBirth,
        //             Gender = input.Gender,
        //             State = input.State,
        //             Term = input.Term,
        //             FaceAmount = input.FaceAmount,
        //             HealthClass = input.HealthClass,
        //             TobaccoUse = input.TobaccoUse,
        //             WOP = input.WOP,
        //             ADB = input.ADB,
        //             ChildRiderAmount = input.ChildRiderAmount
        //         };

        //         int age = product.CalculateAge(clonedInput.DateOfBirth);
        //         var validationErrors = await ValidateInput(clonedInput, product, age);

        //         if (!validationErrors.Any())
        //         {
        //             var quote = await BuildQuoteAsync(clonedInput, product, age);
        //             if (quote != null)
        //                 results.Add(quote);
        //         }
        //         else
        //         {
        //             allValidationErrors.Add($"[{product.CompanyName}] - {string.Join(" | ", validationErrors)}");
        //         }
        //     }

        //     if (!results.Any())
        //     {
        //         throw new ValidationErrorsException(allValidationErrors.Any()
        //             ? allValidationErrors
        //             : new List<string> { "No products match the given criteria." });
        //     }
        // }


        return results;
    }

    #endregion
}