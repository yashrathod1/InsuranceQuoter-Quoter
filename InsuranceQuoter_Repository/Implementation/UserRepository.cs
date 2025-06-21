using InsuranceQuoter_Repository.Database;
using InsuranceQuoter_Repository.Interface;
using InsuranceQuoter_Repository.Models;
using Microsoft.EntityFrameworkCore;

namespace InsuranceQuoter_Repository.Implementation;

public class UserRepository : IUserRepository
{
    private readonly InsuranceDbContext _context;

    public UserRepository(InsuranceDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetUserByEmailAsync(string email)
    {
        try
        {
            return await _context.Users.Where(u => u.Email == email && !u.IsDeleted).FirstOrDefaultAsync();
        }
        catch (Exception ex)
        {
            throw new Exception("Error in getting the user by email", ex);
        }
    }

    public async Task<List<User>> GetAllUserAsync()
    {
        try
        {
            return await _context.Users.Where(u => !u.IsDeleted).OrderBy(u => u.Id).ToListAsync();
        }
        catch (Exception ex)
        {
            throw new Exception("Error in getting all user", ex);
        }
    }

    public async Task<bool> UpdateUser(User user)
    {
        try
        {
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            throw new Exception("Error in updating the user", ex);
        }
    }

    public async Task<bool> IsEmailOrUsernameTakenAsync(string email, string username)
    {
        try
        {
            return await _context.Users.AnyAsync(u => (u.Email == email || u.Username == username) && !u.IsDeleted);
        }
        catch (Exception ex)
        {
            throw new Exception("Error in checking the user emain and username", ex);
        }
    }

    public async Task AddUserAsync(User user)
    {
        try
        {
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            throw new Exception("Error in adding the user", ex);
        }
    }

    public async Task<State?> GetStateByIdAsync(int id)
    {
        try
        {
            return await _context.States.FindAsync(id);
        }
        catch (Exception ex)
        {
            throw new Exception("Error in getting the state by id", ex);
        }
    }

    public async Task<bool> DoesStateExistAsync(int stateId)
    {
        try
        {
            return await _context.States.AnyAsync(s => s.Id == stateId);
        }
        catch (Exception ex)
        {
            throw new Exception("Error in checking the state", ex);
        }
    }

    public async Task<User?> GetUserByIdAsync(int id)
    {
        try
        {
            return await _context.Users.Where(u => u.Id == id && !u.IsDeleted).FirstOrDefaultAsync();
        }
        catch (Exception ex)
        {
            throw new Exception("Error in getting user by id", ex);
        }
    }

    public async Task<bool> IsEmailOrUsernameTakenByOtherAsync(string email, string username, int userId)
    {
        try
        {
            return await _context.Users
                .AnyAsync(u => (u.Email == email || u.Username == username) && u.Id != userId && !u.IsDeleted);
        }
        catch (Exception ex)
        {
            throw new Exception("Error in checking the email or username", ex);
        }
    }

    public async Task UpdateUserAsync(User user)
    {
        try
        {
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
        }
        catch(Exception ex)
        {
            throw new Exception("Error in updating the user",ex);
        }
    }
}
