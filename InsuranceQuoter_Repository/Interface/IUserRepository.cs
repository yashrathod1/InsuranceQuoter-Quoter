using InsuranceQuoter_Repository.Models;

namespace InsuranceQuoter_Repository.Interface;

public interface IUserRepository
{
    Task<User?> GetUserByEmailAsync(string email);

    Task<List<User>> GetAllUserAsync();

    Task<bool> UpdateUser(User user);

    Task<bool> IsEmailOrUsernameTakenAsync(string email, string username);

    Task AddUserAsync(User user);

    Task<State?> GetStateByIdAsync(int id);

    Task<bool> DoesStateExistAsync(int stateId);

    Task<User?> GetUserByIdAsync(int id);

    Task<bool> IsEmailOrUsernameTakenByOtherAsync(string email, string username, int userId);

    Task UpdateUserAsync(User user);
    
}
