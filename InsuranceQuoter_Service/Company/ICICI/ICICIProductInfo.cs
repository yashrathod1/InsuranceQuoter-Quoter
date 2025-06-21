using System.Globalization;
using System.Text;
using CsvHelper;
using InsuranceQuoter_Service.CompanyProduct;
using InsuranceQuoter_Service.ViewModels;
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
    public override decimal? MinChildRiderAmount => 5000;
    public override decimal? MaxChildRiderAmount => 10000;

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


}
