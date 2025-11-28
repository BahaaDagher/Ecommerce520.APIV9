
using Ecommerce520.APIV9.Models;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce520.APIV9.Areas.Identity.Controllers
{
    [Area("Identity")]
    [Route("[Area]/[controller]")]
    [ApiController]
    [Authorize]
    public class ProfileController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public ProfileController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }
        [HttpGet]
        public async Task<IActionResult> GetInfo()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user is null)
            {
                return NotFound(new ErrorModelResponse
                {
                    ErrorCode = 404,
                    ErrorMessage = "User not found."
                });
            }
            //var userVM = new ApplicationUserVM()
            //{
            //    Email = user.Email,
            //    PhoneNumber = user.PhoneNumber,
            //    Address = user.Address,
            //    FullName = $"{user.FirstName} {user.LastName} " 
            //};
            //TypeAdapterConfig<ApplicationUser, ApplicationUserVM>
            //    .NewConfig()
            //    .Map(dest=> dest.FullName , src=> $"{src.FirstName} {src.LastName}");

            var userVM = user.Adapt<ApplicationUserResponse>();
            return Ok(userVM); 
        }
        [HttpPut("UpdateProfile")]
        public async Task<IActionResult> UpdateProfile(ApplicationUserRequest applicationUserVM)
        {
            //var user = applicationUserVM.Adapt<ApplicationUser>();

            var user = await _userManager.GetUserAsync(User);

            if (user is null)
            {
                return NotFound( new ErrorModelResponse
                {
                    ErrorCode = 404,
                    ErrorMessage = "User not found."
                });
            }
            //user.Email = applicationUserVM.Email;
            user.PhoneNumber = applicationUserVM.PhoneNumber;
            user.Address = applicationUserVM.Address;
            var names = applicationUserVM.FullName?.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            user.FirstName = names?.Length > 0 ? names[0] : "";
            user.LastName = names?.Length > 1 ? names[1] : "";
            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                return Ok(new
                {
                    Success = "Profile updated successfully."
                }); 
            }
            else
            {
                return BadRequest(new ErrorModelResponse
                {
                    ErrorCode = 400,
                    ErrorMessage = String.Join(", ", result.Errors.Select(e => e.Description))
                });
            }
        }
        [HttpPut("UpdatePassword")]
        public async Task<IActionResult> UpdatePassword(ApplicationUserRequest applicationUserVM)
        {
            if (String.IsNullOrEmpty(applicationUserVM.CurrentPassword) || String.IsNullOrEmpty(applicationUserVM.NewPassword))
            {
                return BadRequest(new ErrorModelResponse
                {
                    ErrorCode = 400,
                    ErrorMessage = "Current password and new password are required."
                });
            }
            var user = await _userManager.GetUserAsync(User);
            var result = await _userManager.ChangePasswordAsync(user, applicationUserVM.CurrentPassword, applicationUserVM.NewPassword);
            if (result.Succeeded)
            {
                return Ok(new
                {
                    Success = "Password updated successfully."
                });
            }
            else
            {
                return BadRequest( new ErrorModelResponse
                {
                    ErrorCode = 400,
                    ErrorMessage = String.Join(", ", result.Errors.Select(e => e.Description))
                });
            }
        }
    }
}
