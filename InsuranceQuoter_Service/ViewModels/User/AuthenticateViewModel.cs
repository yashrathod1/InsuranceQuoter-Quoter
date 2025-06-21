using System.ComponentModel.DataAnnotations;

namespace InsuranceQuoter_Service.ViewModels;

public class AuthenticateViewModel
{
    [Required(ErrorMessage = "Email is Required")]
    [EmailAddress(ErrorMessage = "Invalid Email Format")]
    public string Email { get; set; } = null!;

    [Required(ErrorMessage = "Password is Required")]
    public string? Password { get; set; }
}
