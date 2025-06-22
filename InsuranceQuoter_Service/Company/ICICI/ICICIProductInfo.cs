using System.Globalization;
using System.Text;
using CsvHelper;
using InsuranceQuoter_Service.CompanyProduct;
using InsuranceQuoter_Service.ViewModels;
using InsuranceQuoter_Service.ViewModels.Base;
using Microsoft.AspNetCore.Http;
using OfficeOpenXml;

namespace InsuranceQuoter_Service.Company.ICICI;

public class ICICIProductInfo : ProductInfoBase
{
    public override string CompanyName => "ICICI";
    public override string ProductName => "ICICI Term";
    public override string TermRating => "A+";
    public override int MinIssueAge => 20;
    public override decimal MonthlyModalFactor => 0.085m;
    public override string AgeDetermination => "Nearest";
    public override List<string> ExcludedStates => new() { "BH", "UP" };
    public override List<int> AllowedTerms => new() { 10, 15, 20, 25, 30, 40 };
    public override decimal MinimumFaceAmount => 100000;
    public override decimal MaximumFaceAmount => 10000000;

    #region GenerateCsv
    protected override async Task GenerateWopRateCsvAsync(IFormFile excelFile, string companyFolder)
    {
        try
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            string mappingCsvPath = Directory.GetFiles(companyFolder, "*_wop_sheet.csv").FirstOrDefault() ?? "";
            if (string.IsNullOrEmpty(mappingCsvPath))
                throw new FileNotFoundException($"Mapping CSV not found for company: {CompanyName}");

            List<IciciWopSheetDataViewModel> configRecords = new();
            using (StreamReader reader = new(mappingCsvPath))
            using (CsvReader csv = new(reader, CultureInfo.InvariantCulture))
            {
                configRecords = csv.GetRecords<IciciWopSheetDataViewModel>().ToList();
            }

            StringBuilder wopOutput = new();
            wopOutput.AppendLine("Term,Age,TobaccoUse,RatePerThousand");

            using (ExcelPackage package = new(excelFile.OpenReadStream()))
            {
                foreach (IciciWopSheetDataViewModel record in configRecords)
                {
                    ExcelWorksheet worksheet = package.Workbook.Worksheets[record.SheetNo - 1];
                    int row = record.StartingRow;

                    for (int age = record.MinimumAge; age <= record.MaximumAge; age++, row++)
                    {
                        string rateStr = worksheet.Cells[row, record.Column].Value?.ToString() ?? "";
                        if (decimal.TryParse(rateStr, out decimal rate))
                        {
                            wopOutput.AppendLine($"{record.Term},{age},{record.TobaccoUse},{rate:0.0000}");
                        }
                    }
                }
            }

            string outputFilePath = Path.Combine(companyFolder, $"{CompanyName.ToLower()}_wop_rates.csv");
            await File.WriteAllTextAsync(outputFilePath, wopOutput.ToString());
        }
        catch (Exception ex)
        {
            throw new Exception("Error in generating the WOP CSV", ex);
        }
    }

    protected override async Task GenerateCrRateCsvAsync(IFormFile excelFile, string companyFolder)
    {
        try
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            string mappingCsvPath = Directory.GetFiles(companyFolder, "*_cr_sheet.csv").FirstOrDefault() ?? "";
            if (string.IsNullOrEmpty(mappingCsvPath))
                throw new FileNotFoundException($"Mapping CSV not found for company: {CompanyName}");

            List<IciciCrSheetDataViewModel> configRecords = new();
            using (StreamReader reader = new(mappingCsvPath))
            using (CsvReader csv = new(reader, CultureInfo.InvariantCulture))
            {
                configRecords = csv.GetRecords<IciciCrSheetDataViewModel>().ToList();
            }

            StringBuilder crOutput = new();
            crOutput.AppendLine("RatePerThousand");

            using (ExcelPackage package = new(excelFile.OpenReadStream()))
            {
                foreach (IciciCrSheetDataViewModel record in configRecords)
                {
                    ExcelWorksheet worksheet = package.Workbook.Worksheets[record.SheetNo - 1];
                    int row = record.StartingRow;
                    string rateStr = worksheet.Cells[row, record.Column].Value?.ToString() ?? "";
                    if (decimal.TryParse(rateStr, out decimal rate))
                    {
                        crOutput.AppendLine($"{rate:0.0000}");
                    }
                }
            }
            string outputFilePath = Path.Combine(companyFolder, $"{CompanyName.ToLower()}_cr_rates.csv");
            await File.WriteAllTextAsync(outputFilePath, crOutput.ToString());
        }
        catch (Exception ex)
        {
            throw new Exception("Error in generating the WOP CSV", ex);
        }
    }

    #endregion

    #region ReadCsv

    public override async Task<decimal?> LoadWopRiderRateAsync(RiderSearchViewModel input)
    {
        string path = Path.Combine(BasePath, CompanyName.ToLower(), $"{CompanyName.ToLower()}_wop_rates.csv");

        if (!File.Exists(path))
            return null;

        using var reader = new StreamReader(path);
        using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
        await csv.ReadAsync();
        csv.ReadHeader();

        while (await csv.ReadAsync())
        {
            int term = csv.GetField<int>("Term");
            int age = csv.GetField<int>("Age");
            bool tobacco = csv.GetField<bool>("TobaccoUse");

            if (term == input.Term && age == input.Age && tobacco == input.TobaccoUse)
            {
                return csv.GetField<decimal>("RatePerThousand");
            }
        }

        return null; // not found
    }

    public override async Task<decimal?> LoadCrRiderRateAsync(RiderSearchViewModel input)
    {
        string path = Path.Combine(BasePath, CompanyName.ToLower(), $"{CompanyName.ToLower()}_cr_rates.csv");

        if (!File.Exists(path))
            return null;

        using var reader = new StreamReader(path);
        using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
        await csv.ReadAsync();
        csv.ReadHeader();

        if (await csv.ReadAsync())
        {
            return csv.GetField<decimal>("RatePerThousand");
        }

        return null;
    }

    public override Dictionary<string, RiderRuleViewModel> RiderRules => new()
    {
        ["Waiver of Premium"] = new RiderRuleViewModel
        {
            RiderName = "Waiver of Premium",
            IsAvailable = true,
            IsSheetBasedMaxAge = true,
        },
        ["Child Rider"] = new RiderRuleViewModel
        {
            RiderName = "Child Rider",
            IsAvailable = true,
            MaxIssueAge = 55,
            MinAmount = 5000,
            MaxAmount = 10000
        }
    };

    public override async Task<int?> GetWopMaxIssueAgeAsync(RiderSearchViewModel input)
    {
        string path = Path.Combine(BasePath, CompanyName.ToLower(), $"{CompanyName.ToLower()}_wop_rates.csv");
        if (!File.Exists(path)) return null;

        using var reader = new StreamReader(path);
        using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
        await csv.ReadAsync(); csv.ReadHeader();

        var ages = new List<int>();
        while (await csv.ReadAsync())
        {
            int term = csv.GetField<int>("Term");
            int age = csv.GetField<int>("Age");
            bool tob = csv.GetField<bool>("TobaccoUse");

            if (term == input.Term && tob == input.TobaccoUse)
                ages.Add(age);
        }

        return ages.Any() ? ages.Max() : null;
    }


    #endregion
}
