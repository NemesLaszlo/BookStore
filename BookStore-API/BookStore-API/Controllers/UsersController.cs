using BookStore_API.Contracts;
using BookStore_API.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BookStore_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ILoggerService _logger;
        private readonly IConfiguration _config;

        private const string EMAIL_VALIDATION_PATTERN = @"^((([a-z]|\d|[!#\$%&'\*\+\-\/=\?\^_`{\|}~]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])+(\.([a-z]|\d|[!#\$%&'\*\+\-\/=\?\^_`{\|}~]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])+)*)|((\x22)((((\x20|\x09)*(\x0d\x0a))?(\x20|\x09)+)?(([\x01-\x08\x0b\x0c\x0e-\x1f\x7f]|\x21|[\x23-\x5b]|[\x5d-\x7e]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(\\([\x01-\x09\x0b\x0c\x0d-\x7f]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF]))))*(((\x20|\x09)*(\x0d\x0a))?(\x20|\x09)+)?(\x22)))@((([a-z]|\d|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(([a-z]|\d|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])([a-z]|\d|-|\.|_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])*([a-z]|\d|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])))\.)+(([a-z]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(([a-z]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])([a-z]|\d|-|\.|_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])*([a-z]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF]))){2,63}\.?$";

        public UsersController(SignInManager<IdentityUser> signInManager, UserManager<IdentityUser> userManager, ILoggerService logger, IConfiguration config)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _logger = logger;
            _config = config;
        }

        private ObjectResult InternalError(string message)
        {
            _logger.LogError(message);
            return StatusCode(500, "Something went wrong.");
        }

        private string GetControllerActionNames()
        {
            var controller = ControllerContext.ActionDescriptor.ControllerName;
            var action = ControllerContext.ActionDescriptor.ActionName;

            return $"{controller} - {action}";
        }

        /// <summary>
        /// USer Registration Endpoint.
        /// </summary>
        /// <param name="userDTO">For Registration</param>
        /// <returns></returns>
        [Route("register")]
        [HttpPost]
        public async Task<IActionResult> Register([FromBody] UserRegistrationDTO userDTO)
        {
            var location = GetControllerActionNames();
            try
            {
                var userName = userDTO.UserName;
                var emailAdress = userDTO.EmailAddress;
                var password = userDTO.Password;
                _logger.LogInfo($"{location}: Registration Attempt for {emailAdress} ");
                var isEmail = IsValidEmailAddress(emailAdress);
                if (!isEmail)
                {
                    return InternalError($"{location}: {emailAdress} User Registration Attempt Failed. (Not valid email!)");
                }
                var user = new IdentityUser { UserName = userName, Email = emailAdress };
                var result = await _userManager.CreateAsync(user, password);

                if (!result.Succeeded)
                {
                    foreach (var error in result.Errors)
                    {
                        _logger.LogError($"{location}: {error.Code} {error.Description}");
                    }
                    return InternalError($"{location}: {emailAdress} User Registration Attempt Failed");
                }
                await _userManager.AddToRoleAsync(user, "Customer");
                return Created("login", new { result.Succeeded });
            }
            catch (Exception e)
            {
                return InternalError($"{location}: {e.Message} - {e.InnerException}");
            }
        }

        /// <summary>
        /// User Login Endpoint. You are able to login with username or email.
        /// </summary>
        /// <param name="userDTO">For Login</param>
        /// <returns>With the token.</returns>
        [Route("login")]
        [HttpPost]
        public async Task<IActionResult> Login([FromBody] UserLoginDTO userDTO)
        {
            var location = GetControllerActionNames();
            try
            {
                var emailOrUserName = userDTO.EmailAddressOrUserName;
                var password = userDTO.Password;
                _logger.LogInfo($"{location}: Login Attempt from user {emailOrUserName} ");
                var isEmail = IsValidEmailAddress(emailOrUserName);
                if (!isEmail)
                {
                    // UserName
                    var userName = emailOrUserName;
                    var result = await _signInManager.PasswordSignInAsync(userName, password, false, false);
                    if (result.Succeeded)
                    {
                        _logger.LogInfo($"{location}: {userName} Successfully Authenticated");
                        var user = await _userManager.FindByNameAsync(userName);
                        _logger.LogInfo($"{location}: Generating Token");
                        var tokenString = await GenerateJSONWebToken(user);
                        return Ok(new { token = tokenString });
                    }
                }
                else
                {
                    // Email
                    var email = emailOrUserName;
                    var user = await _userManager.FindByEmailAsync(email);
                    if(user != null)
                    {
                        var result = await _signInManager.PasswordSignInAsync(user.UserName, password, false, false);
                        if (result.Succeeded)
                        {
                            _logger.LogInfo($"{location}: {email} Successfully Authenticated");
                            _logger.LogInfo($"{location}: Generating Token");
                            var tokenString = await GenerateJSONWebToken(user);
                            return Ok(new { token = tokenString });
                        }
                    }
                }
                _logger.LogInfo($"{location}: {emailOrUserName} Not Authenticated");
                return Unauthorized(userDTO);
            }
            catch (Exception ex)
            {
                return InternalError($"{location}: {ex.Message} - {ex.InnerException}");
            }
        }

        private async Task<string> GenerateJSONWebToken(IdentityUser user)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.NameIdentifier, user.Id)
            };
            var roles = await _userManager.GetRolesAsync(user);
            claims.AddRange(roles.Select(r => new Claim(ClaimsIdentity.DefaultRoleClaimType, r)));

            var token = new JwtSecurityToken(_config["Jwt:Issuer"]
                , _config["Jwt:Issuer"],
                claims,
                null,
                expires: DateTime.Now.AddHours(5),
                signingCredentials: credentials
            );
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private bool IsValidEmailAddress(string email)
        {
            if (string.IsNullOrEmpty(email))
                return false;

            var pattern = EMAIL_VALIDATION_PATTERN;
            const RegexOptions options = RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture;

            var emailValidator = new Regex(pattern, options);

            return emailValidator.IsMatch(email);
        }


    }
}
