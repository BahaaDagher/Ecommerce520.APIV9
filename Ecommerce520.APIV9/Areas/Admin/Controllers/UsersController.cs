using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce520.APIV9.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("[Area]/[controller]")]
    [ApiController]
    [Authorize(Roles = $"{SD.SUPER_ADMIN_ROLE}")]
    public class UsersController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public UsersController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }
        [HttpGet]
        public IActionResult GetAll()
        {
            var users = _userManager.Users.ToList();
            var userResponces = users.Adapt<IEnumerable<UserResponse>>();
            return Ok(users);
        }
        [HttpPut("LockUnLock/{id}")]
        public async Task<IActionResult> LockUnLock(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound( new ErrorModelResponse
                {
                    ErrorCode = 404,
                    ErrorMessage = "User not found."
                });
            }
            if (await _userManager.IsInRoleAsync(user, SD.SUPER_ADMIN_ROLE))
            {
                return BadRequest( new ErrorModelResponse
                {
                    ErrorCode = 400,
                    ErrorMessage = "Cannot lock/unlock a Super Admin user."
                });
            }
            if (user.LockoutEnd != null && user.LockoutEnd > DateTime.UtcNow)
            {
                user.LockoutEnd = null;
            }
            else
            {
                user.LockoutEnd = DateTime.UtcNow.AddYears(1);
            }
            await _userManager.UpdateAsync(user);
            return Ok(new
            {
                Success = "User lock status updated successfully."
            });
        }
    }
}
