using InsuranceQuoter_Service.Interface;
using InsuranceQuoter_Service.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InsuranceQuoter.Controllers;


[ApiController]
[Route("api/[controller]")]
public class UserController : Controller
{
    private readonly IUserService _userService;

    public UserController(IUserService userService)
    {
        _userService = userService;
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login(AuthenticateViewModel model)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            string? token = await _userService.AuthenticateAsync(model);
            if (token == null)
                return Unauthorized(new { message = "Invalid credentials" });

            return Ok(new { Token = token });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error: {ex.Message}");
        }
    }

    [HttpGet("All")]
    public async Task<IActionResult> GetAllUser()
    {
        try
        {
            List<UserListViewModel> users = await _userService.GetAllUserAsync();
            return Ok(users);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error: {ex.Message}");
        }
    }

    [HttpPost("AddUser")]
    public async Task<IActionResult> AddUser([FromBody] UserViewModel model)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var (success, message) = await _userService.CreateUserAsync(model);

            if (!success)
                return BadRequest(new { message });

            return Ok(new { message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error: {ex.Message}");
        }
    }

    [HttpGet("GetState/{id}")]
    public async Task<IActionResult> GetStateById(int id)
    {
        try
        {
            StateViewModel? state = await _userService.GetStateByIdAsync(id);
            if (state == null)
            {
               return BadRequest(new { message = $"State with Id{id} not found." });
            }
            return Ok(state);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error: {ex.Message}");
        }
    }

    [HttpPut("EditUser/{id}")]
    public async Task<IActionResult> EditUser(int id, [FromBody] UserViewModel model)
    {
        try
        {
            var (Success, Message) = await _userService.UpdateUserAsync(id, model);
            return Success ? Ok(Message) : BadRequest(Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error: {ex.Message}");
        }
    }

    [HttpDelete("DeleteUser/{id}")]
    public async Task<IActionResult> SoftDeleteUser(int id)
    {
        try
        {
            var (Success, Message) = await _userService.SoftDeleteUserAsync(id);
            return Success ? Ok(Message) : BadRequest(Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error: {ex.Message}");
        }
    }
}