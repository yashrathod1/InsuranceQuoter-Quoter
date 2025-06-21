using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using InsuranceQuoter_Repository.Interface;
using InsuranceQuoter_Repository.Models;
using InsuranceQuoter_Service.Interface;
using InsuranceQuoter_Service.ViewModels;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace InsuranceQuoter_Service.Implementation;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IConfiguration _configuration;

    public UserService(IUserRepository userRepository, IConfiguration configuration)
    {
        _userRepository = userRepository;
        _configuration = configuration;
    }

    public async Task<string?> AuthenticateAsync(AuthenticateViewModel model)
    {
        try
        {
            User? user = await _userRepository.GetUserByEmailAsync(model.Email);

            if (user == null) return null;

            if (!user.Password.StartsWith("$2a") && !user.Password.StartsWith("$2b") && !user.Password.StartsWith("$2y"))
            {
                string hashPassword = BCrypt.Net.BCrypt.HashPassword(user.Password);
                user.Password = hashPassword;
                await _userRepository.UpdateUser(user);
            }

            if (!BCrypt.Net.BCrypt.Verify(model.Password, user.Password))
            {
                return null;
            }

            JwtSecurityTokenHandler? tokenHandler = new();

            string jwtKey = _configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT key is missing in configuration.");
            byte[]? key = Encoding.UTF8.GetBytes(jwtKey);

            SecurityTokenDescriptor? tokenDescriptor = new()
            {
                Subject = new ClaimsIdentity(new[]{
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Email, user.Email)
                }),
                Expires = DateTime.UtcNow.AddHours(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
                Issuer = _configuration["Jwt:Issuer"],
                Audience = _configuration["Jwt:Audience"]
            };

            SecurityToken token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
        catch (Exception ex)
        {
            throw new Exception("Error in Authenticating user and Generating jwt token", ex);
        }
    }

    public async Task<List<UserListViewModel>> GetAllUserAsync()
    {
        try
        {
            List<User> users = await _userRepository.GetAllUserAsync();

            return users.Select(u => new UserListViewModel
            {

                Id = u.Id,
                FirstName = u.FirstName,
                LastName = u.LastName,
                ContactNumber = u.ContactNumber,
                Gender = u.Gender,
                DateOfBirth = u.DateOfBirth,
                Username = u.Username,
                StateId = u.StateId,
                Email = u.Email

            }).ToList();
        }
        catch (Exception ex)
        {
            throw new Exception("Error in getting all user", ex);
        }
    }

    public async Task<(bool Success, string Message)> CreateUserAsync(UserViewModel model)
    {
        try
        {
            if (await _userRepository.IsEmailOrUsernameTakenAsync(model.Email, model.Username))
                return (false, "Email or username already exists.");

            if (!await _userRepository.DoesStateExistAsync(model.StateId))
                return (false, $"State with Id{model.StateId} does not exits");

            if (!DateOnly.TryParseExact(model.DateOfBirth, "yyyy-MM-dd", out DateOnly dob))
                return (false, "Invalid date format. Use yyyy-MM-dd (e.g., 2004-05-08).");

            if (dob >= DateOnly.FromDateTime(DateTime.Today))
                return (false, "Date of birth must be a past date.");

            User user = new()
            {
                FirstName = model.FirstName,
                LastName = model.LastName,
                Email = model.Email,
                Password = BCrypt.Net.BCrypt.HashPassword(model.Password),
                Username = model.Username,
                ContactNumber = model.ContactNumber,
                Gender = model.Gender,
                DateOfBirth = dob,
                StateId = model.StateId
            };

            await _userRepository.AddUserAsync(user);
            return (true, "User created successfully.");
        }
        catch (Exception ex)
        {
            throw new Exception("Error in creating user", ex);
        }
    }

    public async Task<StateViewModel?> GetStateByIdAsync(int id)
    {
        try
        {
            State? state = await _userRepository.GetStateByIdAsync(id);
            if (state == null) return null;

            return new StateViewModel
            {
                Id = state.Id,
                Name = state.Name
            };
        }
        catch (Exception ex)
        {
            throw new Exception("Error in getting the state by the id", ex);
        }
    }

    public async Task<(bool Success, string Message)> UpdateUserAsync(int id, UserViewModel model)
    {
        try
        {
            User? user = await _userRepository.GetUserByIdAsync(id);
            if (user == null)
                return (false, "User not found.");

            if (!DateOnly.TryParseExact(model.DateOfBirth, "yyyy-MM-dd", out DateOnly dob))
                return (false, "Invalid date format. Use yyyy-MM-dd.");

            if (!await _userRepository.DoesStateExistAsync(model.StateId))
                return (false, $"State with Id {model.StateId} does not exist.");

            if (await _userRepository.IsEmailOrUsernameTakenByOtherAsync(model.Email, model.Username, id))
                return (false, "Email or username is already taken by another user.");

            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.Email = model.Email;
            user.Username = model.Username;
            user.ContactNumber = model.ContactNumber;
            user.Gender = model.Gender;
            user.DateOfBirth = dob;
            user.StateId = model.StateId;
            user.Password = BCrypt.Net.BCrypt.HashPassword(model.Password);

            await _userRepository.UpdateUserAsync(user);
            return (true, "User updated successfully.");
        }
        catch (Exception ex)
        {
            throw new Exception("Error in updating the user", ex);
        }
    }

    public async Task<(bool Success, string Message)> SoftDeleteUserAsync(int id)
    {
        try
        {
            User? user = await _userRepository.GetUserByIdAsync(id);

            if (user == null)
                return (false, "User not found.");

            user.IsDeleted = true;
            await _userRepository.UpdateUserAsync(user);

            return (true, "User soft-deleted successfully.");
        }
        catch (Exception ex)
        {
            throw new Exception("Error in soft-deleting the user", ex);
        }
    }
}

