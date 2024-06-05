using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AccountService.Library.Domain;
using AccountService.Library.DTOs;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

namespace AccountService.Library.Services
{
    public class AccountService
	{
        private readonly UserManager<User> _userManager;

        public AccountService(UserManager<User> userManager)
		{
            _userManager = userManager;
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

                        response.StatusCode = StatusCode.Success;
                        response.Message = $"User with email - {registerDTO.Email} is successfully registered";
                        return response;
                    }
                    else
                    {
                        response.StatusCode = StatusCode.InternalServerError;
                        response.Message = result?.Errors?.ToString();
                    }
                    
                }
                else
                {
                    response.StatusCode = StatusCode.Conflict;
                    response.Message = $"User with email - {registerDTO.Email} is already registered";
                }
               
            }
            catch(Exception ex)
            {
                response.StatusCode = StatusCode.InternalServerError;
                response.Message = ex.ToString();
            }
            return response;
        }

        public async Task<Response> Login(LoginDTO loginDTO, string jwtSecret)
        {
            var response = new Response();
            try
            {
                var user = await _userManager.FindByNameAsync(loginDTO.UserName);
                if (user != null)
                {
                    var passwordCheck = await _userManager.CheckPasswordAsync(user, loginDTO.Password);
                    if (passwordCheck)
                    {
                        var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                };

                        (await _userManager.GetRolesAsync(user)).ToList().ForEach(role => claims.Add(new Claim(ClaimTypes.Role, role)));

                        response.StatusCode = StatusCode.Success;
                        response.Message = GenerateToken(claims, jwtSecret);
                    }
                    else
                    {
                        response.StatusCode = StatusCode.Unauthorized;
                        response.Message = "Login failed with the given userName and password";
                    }
                }
                else
                {
                    response.StatusCode = StatusCode.Unauthorized;
                    response.Message = "Login failed with the given userName and password";
                }
            }
            catch (Exception ex)
            {
                response.StatusCode = StatusCode.InternalServerError;
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

        public async Task<User> GetUserByName(string userName)
        {
            return await _userManager.FindByNameAsync(userName);
        }

    }
}