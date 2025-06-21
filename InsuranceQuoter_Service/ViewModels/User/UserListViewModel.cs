namespace InsuranceQuoter_Service.ViewModels;

public class UserListViewModel
{   
    public int Id { get; set; }
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Username { get; set; } = null!;
    public string? ContactNumber { get; set; }
    public string Gender { get; set; } = null!;
    public DateOnly DateOfBirth { get; set; } 
    public int StateId { get; set; }
}
