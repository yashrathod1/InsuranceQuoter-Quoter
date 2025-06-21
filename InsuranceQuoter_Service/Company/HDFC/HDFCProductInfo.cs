using System.Globalization;
using System.Text;
using CsvHelper;
using InsuranceQuoter_Service.CompanyProduct;
using InsuranceQuoter_Service.ViewModels;
using InsuranceQuoter_Service.ViewModels.HDFC;
using Microsoft.AspNetCore.Http;
using OfficeOpenXml;

namespace InsuranceQuoter_Service.Company.HDFC;

public class HDFCProductInfo : ProductInfoBase
{
    public override string CompanyName => "HDFC";
    public override string ProductName => "HDFC Term";
    public override string TermRating => "A";
    public override int MinIssueAge => 20;
    public override decimal MonthlyModalFactor => 0.0845m;
    public override string AgeDetermination => "Nearest";
    public override List<string> ExcludedStates => new() { "BH", "UP" };
    public override List<int> AllowedTerms => new() { 10, 15, 20, 25, 30, 35 };
    public override decimal MinimumFaceAmount => 100000;
    public override decimal MaximumFaceAmount => 100000000;

    protected override async Task GenerateWopRateCsvAsync(IFormFile excelFile, string companyFolder)
    {
        try
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            string mappingCsvPath = Directory.GetFiles(companyFolder, "*_wop_sheet.csv").FirstOrDefault() ?? "";
            if (string.IsNullOrEmpty(mappingCsvPath))
                throw new FileNotFoundException($"Mapping CSV not found for company: {CompanyName}");

            List<HdfcWopSheetDataViewModel> configRecords = new();
            using (StreamReader reader = new(mappingCsvPath))
            using (CsvReader csv = new(reader, CultureInfo.InvariantCulture))
            {
                configRecords = csv.GetRecords<HdfcWopSheetDataViewModel>().ToList();
            }

            StringBuilder wopOutput = new();
            wopOutput.AppendLine("Age,RatePerThousand");

            using (ExcelPackage package = new(excelFile.OpenReadStream()))
            {
                foreach (HdfcWopSheetDataViewModel record in configRecords)
                {
                    ExcelWorksheet worksheet = package.Workbook.Worksheets[record.SheetNo - 1];
                    int row = record.StartingRow;

                    for (int age = record.MinimumAge; age <= record.MaximumAge; age++, row++)
                    {
                        string rateStr = worksheet.Cells[row, record.Column].Value?.ToString() ?? "";
                        if (decimal.TryParse(rateStr, out decimal rate))
                        {
                            wopOutput.AppendLine($"{age},{rate:0.0000}");
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

            List<HdfcCrSheetDataViewModel> configRecords = new();
            using (StreamReader reader = new(mappingCsvPath))
            using (CsvReader csv = new(reader, CultureInfo.InvariantCulture))
            {
                configRecords = csv.GetRecords<HdfcCrSheetDataViewModel>().ToList();
            }

            StringBuilder crOutput = new();
            crOutput.AppendLine("Age,RatePerThousand");

            using (ExcelPackage package = new(excelFile.OpenReadStream()))
            {
                foreach (HdfcCrSheetDataViewModel record in configRecords)
                {
                    ExcelWorksheet worksheet = package.Workbook.Worksheets[record.SheetNo - 1];
                    int row = record.StartingRow;
                    for (int age = record.MinimumAge; age <= record.MaximumAge; age++, row++)
                    {
                        string rateStr = worksheet.Cells[row, record.Column].Value?.ToString() ?? "";
                        if (decimal.TryParse(rateStr, out decimal rate))
                        {
                            crOutput.AppendLine($"{age},{rate:0.0000}");
                        }
                    }
                }
            }
            string outputFilePath = Path.Combine(companyFolder, $"{CompanyName.ToLower()}_cr_rates.csv");
            await File.WriteAllTextAsync(outputFilePath, crOutput.ToString());
        }
        catch (Exception ex)
        {
            throw new Exception("Error in generating the CR CSV", ex);
        }
    }

    protected override async Task GenerateABDRateCsvAsync(IFormFile excelFile, string companyFolder)
    {
        try
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            string mappingCsvPath = Directory.GetFiles(companyFolder, "*_adb_sheet.csv").FirstOrDefault() ?? "";
            if (string.IsNullOrEmpty(mappingCsvPath))
                throw new FileNotFoundException($"Mapping CSV not found for company: {CompanyName}");

            List<HdfcAbdSheetDataViewModel> configRecords = new();
            using (StreamReader reader = new(mappingCsvPath))
            using (CsvReader csv = new(reader, CultureInfo.InvariantCulture))
            {
                configRecords = csv.GetRecords<HdfcAbdSheetDataViewModel>().ToList();
            }

            StringBuilder crOutput = new();
            crOutput.AppendLine("Age,RatePerThousand");

            using (ExcelPackage package = new(excelFile.OpenReadStream()))
            {
                foreach (HdfcAbdSheetDataViewModel record in configRecords)
                {
                    ExcelWorksheet worksheet = package.Workbook.Worksheets[record.SheetNo - 1];
                    int row = record.StartingRow;
                    for (int age = record.MinimumAge; age <= record.MaximumAge; age++, row++)
                    {
                        string rateStr = worksheet.Cells[row, record.Column].Value?.ToString() ?? "";
                        if (decimal.TryParse(rateStr, out decimal rate))
                        {
                            crOutput.AppendLine($"{age},{rate:0.0000}");
                        }
                    }
                }
            }
            string outputFilePath = Path.Combine(companyFolder, $"{CompanyName.ToLower()}_adb_rates.csv");
            await File.WriteAllTextAsync(outputFilePath, crOutput.ToString());
        }
        catch (Exception ex)
        {
            throw new Exception("Error in generating the ABD CSV", ex);
        }
    }
}
