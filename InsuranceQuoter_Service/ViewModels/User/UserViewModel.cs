using System.ComponentModel.DataAnnotations;

namespace InsuranceQuoter_Service.ViewModels;

public class UserViewModel
{   
    [Required(ErrorMessage = "First name is required.")]
    [StringLength(50, ErrorMessage = "First name cannot exceed 50 characters.")]
    [RegularExpression(@"^[a-zA-Z]+(?: [a-zA-Z]+)*$", ErrorMessage = "Firstname is not valid")]
    public string FirstName { get; set; } = null!;

    [Required(ErrorMessage = "Last name is required.")]
    [StringLength(50, ErrorMessage = "Last name cannot exceed 50 characters.")]
    [RegularExpression(@"^[a-zA-Z]+(?: [a-zA-Z]+)*$", ErrorMessage = "Lastname is not valid")]
    public string LastName { get; set; } = null!;

    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Invalid email format.")]
    public string Email { get; set; } = null!;

    [Required(ErrorMessage = "Password is required.")]
    [RegularExpression(@"^(?=.*[A-Z])(?=.*[a-z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$",
            ErrorMessage = "Password must be at least 8 characters, include uppercase, lowercase, a number, and a special character.")]
    public string Password { get; set; } = null!;

    [Required(ErrorMessage = "Username is required.")]
    [StringLength(50, ErrorMessage = "Username cannot exceed 50 characters.")]
    [RegularExpression(@"^(?!.*[_.]{2})(?![_.])[a-zA-Z0-9._]+(?<![_.])$", ErrorMessage = "Username is not valid.")]
    public string Username { get; set; } = null!;

    [Required(ErrorMessage = "Contact number is required.")]
    [RegularExpression(@"^\+91[6-9]\d{9}$", ErrorMessage = "Invalid Indian contact number.")]
    public string ContactNumber { get; set; } = null!;

    [Required(ErrorMessage = "Gender is required.")]
    [RegularExpression(@"^(Male|Female)$", ErrorMessage = "Gender must be 'Male' or 'Female'.")]
    public string Gender { get; set; } = null!;

    [Required(ErrorMessage = "Date of birth is required.")]
    [RegularExpression(@"^\d{4}-\d{2}-\d{2}$", ErrorMessage = "Date of birth must be in yyyy-MM-dd format.")]
    public string DateOfBirth { get; set; } = null!;

    [Required(ErrorMessage = "State is required.")]
    public int StateId { get; set; }
}
