using BankingApplication.Interfaces;
using BankingApplication.DTOs;
using BankingApplication.Models; 
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace BankingApplication.Controllers
{
    [ApiController]
    [Route("api/[controller]")] 
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        
        [HttpPost]
        [ProducesResponseType(typeof(User), StatusCodes.Status201Created)] 
        [ProducesResponseType(StatusCodes.Status400BadRequest)] 
        [ProducesResponseType(StatusCodes.Status500InternalServerError)] 
        public async Task<IActionResult> AddUser([FromBody] CreateUserRequest userRequest)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var newUser = await _userService.AddUser(userRequest);
                return CreatedAtAction(nameof(AddUser), new { id = newUser.Id }, newUser);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error adding user: {ex.Message}");
            }
        }
    }
}