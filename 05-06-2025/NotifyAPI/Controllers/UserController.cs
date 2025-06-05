using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NotifyAPI.Interfaces;
using NotifyAPI.Models.DTOs;

namespace NotifyAPI.Controllers
{
    [ApiController]
    [Route("api/users")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpPost]
        public async Task<IActionResult> AddUser([FromBody] UserAddRequestDto userDto)
        {
            try
            {
                var user = await _userService.AddUser(userDto);
                return CreatedAtAction(nameof(AddUser), new { id = user.Username }, user);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
