using Ecommerce520.APIV9.DTOs.Response;
using Ecommerce520.APIV9.JwtFeatures;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Ecommerce520.APIV9.Areas.Identity.Controllers
{
    [Route("[Area]/[controller]")]
    [ApiController]
    [Area("Identity")]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IEmailSender _emailSender;
        private readonly IRepository<ApplicationUserOTP> _applicationUserOTPRepository;
        private readonly IJwtHandler _jwtHandler;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IEmailSender emailSender,
            IRepository<ApplicationUserOTP> applicationUserOTPRepository,
            IJwtHandler jwtHandler)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
            _applicationUserOTPRepository = applicationUserOTPRepository;
            _jwtHandler = jwtHandler;
        }

        [HttpPost("Register")]  
        public async Task<IActionResult> Register(RegisterRequest registerVM)
        {
            
            ApplicationUser user = new ApplicationUser()
            {
                UserName = registerVM.UserName,
                Email = registerVM.Email,
                FirstName = registerVM.FirstName,
                LastName = registerVM.LastName,
            };
            var result = await _userManager.CreateAsync(user, registerVM.Password);
            if (!result.Succeeded)
            {
                string errors = "";
                foreach (var error in result.Errors)
                {
                   errors += $"{error.Description} \n";
                }
                return BadRequest(new
                {
                    msg = errors
                }); 
            }
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var link = Url.Action(nameof(ConfirmEmail), "Account", new { Area = "Identity", token, userId = user.Id }, Request.Scheme);
            await _emailSender.SendEmailAsync(registerVM.Email, "Ecommerce 520 Confirm Email",
                $"<h1> confirm your email by clicking <a href='{link}'> here</a>  </h1>");
            return Ok(new
            {
                msg = "Registration Successful. Please check your email to confirm your account."
            }); 
        }
        [HttpPost("ConfirmEmail")]
        public async Task<IActionResult> ConfirmEmail(string token, string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user is null)
            {
                return NotFound(new
                {
                    msg = "Invalid User"
                }); 
            }
            var result = await _userManager.ConfirmEmailAsync(user, token);
            if (!result.Succeeded)
            {
                return BadRequest(new
                {
                    msg = "Email Confirmation Failed",
                });
            }
            else
            {
                return Ok(new
                {
                    msg = "Email Confirmed Successfully"
                });
            }
        }
        [HttpPost("Login")]
        public async Task<IActionResult> Login(LoginRequest loginVm)
        {
            var user = await _userManager.FindByNameAsync(loginVm.UserNameOrEmail) ?? await _userManager.FindByEmailAsync(loginVm.UserNameOrEmail);
            if (user is null)
            {
                return NotFound(new ErrorModelResponse
                {
                    ErrorCode = 404,
                    ErrorMessage = "Invalid UserName Or Email"
                });
            }
            //await _userManager.CheckPasswordAsync(user , loginVm.Password); 
            var result = await _signInManager.PasswordSignInAsync(user, loginVm.Password, loginVm.RememberMe, true);
            if (!result.Succeeded)
            {
                if (result.IsLockedOut)
                {
                    return BadRequest(new ErrorModelResponse
                    {
                        ErrorCode = 400,
                        ErrorMessage = "Your account is locked out. Please try again later."
                    });
                }
                else if (!user.EmailConfirmed)
                {
                    return BadRequest(new ErrorModelResponse
                    {
                        ErrorCode = 400,
                        ErrorMessage = "Please confirm your email before logging in." , 
                    });
                }
                else
                {
                    return BadRequest(new ErrorModelResponse
                    {
                        ErrorCode = 400,
                        ErrorMessage = "Invalid Password"
                    });
                }
            }

            var AccessToken = await _jwtHandler.GenerateAccessTokenAsync(user);
            return Ok(new AuthenticatedResponse
            {
                AccessToken = AccessToken 
            });  
        }
        [HttpPost("ResendEmailConfirmation")]
        public async Task<IActionResult> ResendEmailConfirmation(ResendEmailConfirmationRequest  resendEmailConfirmationVM)
        {
            var user = await _userManager.FindByNameAsync(resendEmailConfirmationVM.UserNameOrEmail) ?? await _userManager.FindByEmailAsync(resendEmailConfirmationVM.UserNameOrEmail);
            if (user is null)
            {
                return NotFound (new ErrorModelResponse
                {
                    ErrorCode = 404,
                    ErrorMessage = "Invalid UserName Or Email"
                });
            }
            if (user.EmailConfirmed)
            {
                return BadRequest (new ErrorModelResponse
                {
                    ErrorCode = 400,
                    ErrorMessage = "Email is already confirmed"
                });
            }
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var link = Url.Action(nameof(ConfirmEmail), "Account", new { Area = "Identity", token, userId = user.Id }, Request.Scheme);
            await _emailSender.SendEmailAsync(user.Email, "Ecommerce 520 Confirm Email",
                $"<h1> confirm your email by clicking <a href='{link}'> here</a>  </h1>");
            return Ok( new
            {
                msg = "Confirmation Email Resent. Please check your email to confirm your account."
            }); 

        }
        [HttpPost("ForgetPassword")]
        public async Task<IActionResult> ForgetPassword(ForgetPasswordRequest forgetPassword)
        {
            var user = await _userManager.FindByNameAsync(forgetPassword.UserNameOrEmail) ?? await _userManager.FindByEmailAsync(forgetPassword.UserNameOrEmail);
            if (user is null)
            {
                return NotFound(new ErrorModelResponse
                {
                    ErrorCode = 404,
                    ErrorMessage = "Invalid UserName Or Email"
                });
            }
            if (!user.EmailConfirmed)
            {
                return BadRequest(new ErrorModelResponse
                {
                    ErrorCode = 400,
                    ErrorMessage = "Please confirm your email before resetting your password."
                });
            }
            var otps = await _applicationUserOTPRepository.GetAsync(opt => opt.ApplicationUserId == user.Id);
            var last12hoursOTP = otps.Count(otp => otp.CreatedAt > DateTime.UtcNow.AddHours(-24));
            if (last12hoursOTP >= 10)
            {
                return BadRequest(new ErrorModelResponse
                {
                    ErrorCode = 400,
                    ErrorMessage = "You have exceeded the maximum number of OTP requests allowed in the last 24 hours. Please try again later."
                });
            }
            foreach (var otp in otps)
            {
                otp.IsValid = false;
                _applicationUserOTPRepository.Update(otp);
                await _applicationUserOTPRepository.CommitAsync();
            }
            var OTP = new Random().Next(1000, 9999).ToString();
            ApplicationUserOTP applicationUserOTP = new ApplicationUserOTP(OTP, user.Id);
            await _applicationUserOTPRepository.AddAsync(applicationUserOTP);
            await _applicationUserOTPRepository.CommitAsync();
            await _emailSender.SendEmailAsync(user.Email, "Ecommerce 520 Reset Password",
                $"<h1> use this OTP <span style ='color:red;'>'{OTP}'</span> to Rest your Password  </h1>");

            return Ok(new
            {
                msg = "OTP has been sent to your email address."
            });
        }
        [HttpPost("ValidateOTP")]
        public async Task<IActionResult> ValidateOTP(ValidateOTPRequest validateOTPVM)
        {
            var user = await _userManager.FindByIdAsync(validateOTPVM.ApplicationUserId);
            if (user is null)
            {
                return NotFound(new ErrorModelResponse
                {
                    ErrorCode = 404,
                    ErrorMessage = "Invalid User"
                });
            }
            var applicationUserOTP = await _applicationUserOTPRepository.GetOneAsync(
                otp => otp.ApplicationUserId == validateOTPVM.ApplicationUserId &&
                otp.OTP == validateOTPVM.OTP &&
                otp.IsValid
                );
            if (applicationUserOTP is null)
            {
                return BadRequest(new ErrorModelResponse
                {
                    ErrorCode = 400,
                    ErrorMessage = "Invalid OTP"
                });
            }
            if (DateTime.UtcNow > applicationUserOTP.ExpireAt)
            {
                applicationUserOTP.IsValid = false;
                _applicationUserOTPRepository.Update(applicationUserOTP);
                await _applicationUserOTPRepository.CommitAsync();

                return BadRequest(new ErrorModelResponse
                {
                    ErrorCode = 400,
                    ErrorMessage = "OTP has expired"
                });
            }
            applicationUserOTP.IsValid = false;
            _applicationUserOTPRepository.Update(applicationUserOTP);
            await _applicationUserOTPRepository.CommitAsync();

            //TempData["OTPValidateUerId"] = user.Id;

            return Ok (new
            {
                msg = "OTP validated successfully",
            });

        }
        [HttpPost("ResetPassword")]
        public async Task<IActionResult> ResetPassword(ResetPasswordRequest resetPasswordVM)
        {
            //var OTPValidateUerId = TempData["OTPValidateUerId"]?.ToString();
            //if (String.IsNullOrEmpty(OTPValidateUerId) || OTPValidateUerId != resetPasswordVM.ApplicationUserId)
            //{
            //    ModelState.AddModelError(string.Empty, "Unauthorized Access to Reset Password");
            //    return RedirectToAction("ForgetPassword");
            //}
            var user = await _userManager.FindByIdAsync(resetPasswordVM.ApplicationUserId);
            if (user is null)
            {
                return NotFound(new ErrorModelResponse
                {
                    ErrorCode = 404,
                    ErrorMessage = "Invalid User"
                });
            }
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, token, resetPasswordVM.NewPassword);
            if (!result.Succeeded)
            {
                string errors = "";
                foreach (var error in result.Errors)
                {
                    errors += $"{error.Description} \n";
                }
                return BadRequest(new ErrorModelResponse
                {
                    ErrorCode = 400,
                    ErrorMessage = errors
                });
            }
            //TempData.Remove("OTPValidateUerId");
            return Ok(new
            {
                msg = "Password has been reset successfully"
            });

        }
        [HttpPost("Logout")]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return Ok (new
            {
                msg = "Logout Successful"
            });
        }


        [HttpPost]
        [Route("refresh")]
        public async Task<IActionResult> Refresh(TokenApiRequest tokenApiRequest)
        {
            if (tokenApiRequest is null || tokenApiRequest.AccessToken is null || tokenApiRequest.RefreshToken is null)
                return BadRequest("Invalid client request");

            string accessToken = tokenApiRequest.AccessToken;
            string refreshToken = tokenApiRequest.RefreshToken;

            var principal = _jwtHandler.GetPrincipalFromExpiredToken(accessToken);

            var username = principal.Identity.Name; //this is mapped to the Name claim by default
            //var user = _userContext.LoginModels.SingleOrDefault(u => u.UserName == username);
            var user = _userManager.Users.FirstOrDefault(u => u.UserName == username);
            if (user is null || user.RefreshToken != refreshToken || user.RefreshTokenExpiryTime <= DateTime.Now)
                return BadRequest("Invalid client request");




            var newAccessToken = await _jwtHandler.GenerateAccessTokenAsync(user);
            var newRefreshToken = _jwtHandler.GenerateRefreshToken();

            user.RefreshToken = newRefreshToken;

            await _userManager.UpdateAsync(user);
            return Ok(new AuthenticatedResponse()
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken
            });
        }
    }
}
