using InsuranceQuoter_Service.ViewModels;

namespace InsuranceQuoter_Service.Interface;

public interface IUserService
{
    Task<string?> AuthenticateAsync(AuthenticateViewModel model);

    Task<List<UserListViewModel>> GetAllUserAsync();

    Task<(bool Success, string Message)> CreateUserAsync(UserViewModel model);

    Task<StateViewModel?> GetStateByIdAsync(int id);

    Task<(bool Success, string Message)> UpdateUserAsync(int id,UserViewModel model);

    Task<(bool Success, string Message)> SoftDeleteUserAsync(int id);
}
