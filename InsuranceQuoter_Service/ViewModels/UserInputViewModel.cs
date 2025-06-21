using System.ComponentModel.DataAnnotations;

namespace InsuranceQuoter_Service.ViewModels;

public class UserInputViewModel
{
    [Required(ErrorMessage = "Company name is Required")]
    public string CompanyName { get; set; } = null!;

    [Required(ErrorMessage = "DateofBirth is Required")]
    public DateOnly DateOfBirth { get; set; }

    [Required(ErrorMessage = "Gender is Required")]
    [RegularExpression(@"^(?i)(Male|Female)$", ErrorMessage = "Gender must be 'Male' or 'Female'.")]
    public string Gender { get; set; } = null!;

    [Required(ErrorMessage = "State is Required")]
    public string State { get; set; } = null!;

    [Required(ErrorMessage = "Term is Required")]
    public int Term { get; set; }

    [Required(ErrorMessage = "FaceAmount is Required")]
    public int FaceAmount { get; set; }

    [Required(ErrorMessage = "HealthClass is Required")]
    public string HealthClass { get; set; } = null!;

    [Required(ErrorMessage = "TobaccoUse is Required")]
    public bool TobaccoUse { get; set; }
    
    public bool WOP { get; set; }
    
    public decimal? ChildRiderAmount { get; set;}
}
