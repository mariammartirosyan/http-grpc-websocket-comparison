using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AccountService.Library.Domain;
using AccountService.Library.DTOs;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace AccountService.Library.Services
{
	public class AccountService
	{
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;

        public AccountService(UserManager<User> userManager,
            SignInManager<User> signInManager)
		{
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public async Task<Response> Register(RegisterDTO registerDTO)
        {
            var response = new Response();
            try
            {
                var user = await _userManager.FindByEmailAsync(registerDTO.Email);
                if (user == null)
                {
                    user = new User
                    {
                        UserName = registerDTO.UserName,
                        Email = registerDTO.Email,
                        FirstName = registerDTO.FirstName,
                        LastName = registerDTO.LastName
                    };
                    var result = await _userManager.CreateAsync(user, registerDTO.Password);
                    if (result.Succeeded)
                    {
                        await _userManager.AddToRoleAsync(user, Constants.Roles.User.ToString());

                        response.StatusCode = 200;
                        response.Message = $"User with email - {registerDTO.Email} is successfully registered";
                        return response;
                    }
                    else
                    {
                        response.StatusCode = 500;
                        response.Message = result?.Errors?.ToString();
                    }
                    
                }
                else
                {
                    response.StatusCode = 409;
                    response.Message = $"User with email - {registerDTO.Email} is already registered";
                }
               
            }
            catch(Exception ex)
            {
                response.StatusCode = 500;
                response.Message = ex.ToString();
            }
            return response;
        }

        public async Task<Response> Login(LoginDTO loginDTO, string jwtSecret)
        {
            var response = new Response();
            try
            {
                var result = await _signInManager.PasswordSignInAsync(loginDTO.UserName, loginDTO.Password, false, false);

                if (result.Succeeded)
                {
                    var user = await _userManager.FindByNameAsync(loginDTO.UserName);
                    var claims = new List<Claim>
                    {
                       new Claim(ClaimTypes.Email, user.Email),
                       new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    };

                    (await _userManager.GetRolesAsync(user)).ToList().ForEach(role => claims.Add(new Claim(ClaimTypes.Role, role)));

                    response.StatusCode = 200;
                    response.Message = GenerateToken(claims, jwtSecret);
                }
                else
                {
                    response.StatusCode = 401;
                    response.Message = "Login failed with the given userName and password";

                }
            }
            catch (Exception ex)
            {
                response.StatusCode = 500;
                response.Message = $"Error occured: {ex}";
            }
            return response;
        }

        string GenerateToken(IEnumerable<Claim> claims, string jwtSecret)
        {
            var descriptor = new SecurityTokenDescriptor
            {
                Expires = DateTime.UtcNow.AddHours(2),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)), SecurityAlgorithms.HmacSha256),
                Subject = new ClaimsIdentity(claims)
            };

            var handler = new JwtSecurityTokenHandler();
            return handler.WriteToken(handler.CreateToken(descriptor));
        }

    }
}

