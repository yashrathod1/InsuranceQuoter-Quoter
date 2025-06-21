using InsuranceQuoter_Service.ViewModels;

namespace InsuranceQuoter_Service.Interface;

public interface IProductValidator
{
    (bool isValid, List<string> errors) ValidateInput(UserInputViewModel input);

    string MapHealthClass(string healthClass, bool tobaccoUse);

    int DetermineBand(int faceAmount);

    int CalculateAge(DateOnly dob, string method);

    string GetCsvPath(string company);
}
