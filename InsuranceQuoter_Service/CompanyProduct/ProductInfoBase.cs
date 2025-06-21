using System.Globalization;
using System.Text;
using CsvHelper;
using InsuranceQuoter_Service.ViewModels;
using Microsoft.AspNetCore.Http;
using OfficeOpenXml;

namespace InsuranceQuoter_Service.CompanyProduct;

public abstract class ProductInfoBase : IProductInfo
{
    public abstract string CompanyName { get; }
    public abstract string ProductName { get; }
    public abstract string TermRating { get; }
    public abstract int MinIssueAge { get; }
    public abstract decimal MonthlyModalFactor { get; }
    public abstract string AgeDetermination { get; }
    public abstract List<int> AllowedTerms { get; }
    public abstract decimal MinimumFaceAmount { get; }
    public abstract decimal MaximumFaceAmount { get; }
    public abstract List<string> ExcludedStates { get; }
    public virtual bool SupportsChildRider => MinChildRiderAmount.HasValue && MaxChildRiderAmount.HasValue;
    public virtual decimal? MinChildRiderAmount => null;
    public virtual decimal? MaxChildRiderAmount => null;
    public bool IsTermAllowed(int Term) => AllowedTerms.Contains(Term);

    protected string BasePath => Path.Combine(AppContext.BaseDirectory, @"..\..\..\..\InsuranceQuoter_Service\company");

    public bool IsFaceAmountAllowed(decimal faceAmount)
        => faceAmount >= MinimumFaceAmount && faceAmount <= MaximumFaceAmount;

    public int CalculateAge(DateOnly dob)
    {
        DateOnly today = DateOnly.FromDateTime(DateTime.Today);
        int age = today.Year - dob.Year;
        if (dob > today.AddYears(-age)) age--;

        if (AgeDetermination.Equals("Nearest", StringComparison.OrdinalIgnoreCase))
        {
            DateOnly nextBirthday = dob.AddYears(age + 1);
            if ((nextBirthday.DayNumber - today.DayNumber) <= 182)
            {
                age++;
            }
        }
        return age;
    }

    public bool IsStateAllowed(string stateCode)
    {
        return !ExcludedStates.Contains(stateCode.ToUpper());
    }

    #region ReadCsv
    public virtual async Task<RateRowViewModel?> LoadBaseRateAsync(RateSearchViewModel input)
    {
        string path = Path.Combine(BasePath, CompanyName.ToLower(), $"{CompanyName.ToLower()}_rates.csv");

        if (!File.Exists(path))
            throw new FileNotFoundException($"Base rate CSV not found for {CompanyName}");

        using var reader = new StreamReader(path);
        using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
        await csv.ReadAsync();
        csv.ReadHeader();

        while (await csv.ReadAsync())
        {
            var record = csv.GetRecord<RateRowViewModel>();
            if (record.Term == input.Term &&
                record.Age == input.Age &&
                input.FaceAmount >= record.MinimumFaceAmount &&
                input.FaceAmount <= record.MaximumFaceAmount &&
                record.Gender.Equals(input.Gender, StringComparison.OrdinalIgnoreCase) &&
                record.HealthClass.Equals(input.HealthClass, StringComparison.OrdinalIgnoreCase))
            {
                return record;
            }
        }

        return null;
    }
    public virtual Task<decimal?> LoadWopRiderRateAsync(int age, int term, bool tobaccoUse) => Task.FromResult<decimal?>(null);
    public virtual Task<decimal?> LoadCrRiderRateAsync() => Task.FromResult<decimal?>(null);

    #endregion

    #region GenerateCsv
    public async Task GenerateCsvFilesAsync(IFormFile excelFile, string companyFolder)
    {
        await GenerateBaseRateCsvAsync(excelFile, companyFolder);
        await GenerateWopRateCsvAsync(excelFile, companyFolder);
        await GenerateCrRateCsvAsync(excelFile, companyFolder);
        await GenerateABDRateCsvAsync(excelFile, companyFolder);
    }

    protected virtual async Task GenerateBaseRateCsvAsync(IFormFile excelFile, string companyFolder)
    {
        try
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            string mappingCsvPath = Directory.GetFiles(companyFolder, "*_sheets.csv").FirstOrDefault() ?? "";
            if (string.IsNullOrEmpty(mappingCsvPath))
                throw new FileNotFoundException($"Mapping CSV not found for company: {CompanyName}");

            List<RateTableDataViewModel>? configRecords = new();
            using (StreamReader reader = new(mappingCsvPath))
            using (CsvReader csv = new(reader, CultureInfo.InvariantCulture))
            {
                configRecords = csv.GetRecords<RateTableDataViewModel>().ToList();
            }

            StringBuilder output = new();
            output.AppendLine("Term,Gender,MinimumFaceAmount,MaximumFaceAmount,PolicyFee,HealthClass,Age,RatePerThousand");

            using (ExcelPackage package = new(excelFile.OpenReadStream()))
            {
                foreach (RateTableDataViewModel record in configRecords)
                {
                    ExcelWorksheet worksheet = package.Workbook.Worksheets[record.SheetNo - 1];
                    int row = record.StartingRow;

                    for (int age = record.MinimumAge; age <= record.MaximumAge; age++, row++)
                    {
                        string rateStr = worksheet.Cells[row, record.Column].Value?.ToString() ?? "";
                        if (decimal.TryParse(rateStr, out decimal rate))
                        {
                            string formattedRate = rate.ToString("0.0000", CultureInfo.InvariantCulture);
                            output.AppendLine($"{record.Term},{record.Gender},{record.MinimumFaceAmount},{record.MaximumFaceAmount},{record.PolicyFee},{record.HealthClass},{age},{formattedRate}");
                        }
                    }
                }
            }

            string outputFilePath = Path.Combine(companyFolder, $"{CompanyName.ToLower()}_rates.csv");
            await File.WriteAllTextAsync(outputFilePath, output.ToString());
        }
        catch (Exception ex)
        {
            throw new Exception("Error in Generating base rate CSV file", ex);
        }
    }

    protected virtual Task GenerateWopRateCsvAsync(IFormFile excelFile, string companyFolder)
        => Task.CompletedTask;

    protected virtual Task GenerateCrRateCsvAsync(IFormFile excelFile, string companyFolder)
        => Task.CompletedTask;

    protected virtual Task GenerateABDRateCsvAsync(IFormFile excelFile, string companyFolder)
        => Task.CompletedTask;

    #endregion

    #region calculation
    public decimal CalculateBaseAnnualPremium(decimal faceAmount, decimal rate)
    {
        return (faceAmount / 1000m) * rate;
    }

    public decimal CalculateBaseMonthlyPremium(decimal faceAmount, decimal rate)
    {
        decimal baseAnnual = CalculateBaseAnnualPremium(faceAmount, rate);
        return baseAnnual * MonthlyModalFactor;
    }

    public decimal CalculateAnnualPremium(decimal faceAmount, decimal rate, decimal policyFee)
    {
        decimal baseAnnual = CalculateBaseAnnualPremium(faceAmount, rate);
        return baseAnnual + policyFee;
    }

    public decimal CalculateMonthlyPremium(decimal faceAmount, decimal rate, decimal policyFee)
    {
        decimal annual = CalculateAnnualPremium(faceAmount, rate, policyFee);
        return annual * MonthlyModalFactor;
    }

    public decimal CalculateWopPremium(decimal faceAmount, decimal rate)
    {
        return (faceAmount / 1000) * rate;
    }

    public decimal CalculateCrPremium(decimal faceAmount, decimal rate)
    {
        return (faceAmount / 1000) * rate;
    }

    public virtual bool TryNormalizeHealthClass(string healthClass, bool tobaccoUsage, out string? normalized)
    {
        normalized = (healthClass.ToUpper(), tobaccoUsage) switch
        {
            ("PP", false) => "Preferred Plus Non-Tob",
            ("P", false) => "Preferred Non-Tob",
            ("RP", false) => "Standard Plus Non-Tob",
            ("R", false) => "Standard Non-Tob",
            ("PP", true) or ("P", true) => "Preferred Tob",
            ("RP", true) or ("R", true) => "Standard Tob",
            _ => null
        };

        return normalized != null;
    }

    #endregion

}
