using AccountService.Library.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AccountService.API.Controllers
{
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly Library.Services.AccountService accountService;
        private readonly ILogger<AccountController> logger;
        private readonly IConfiguration config;

        public AccountController(Library.Services.AccountService accountService,
            ILogger<AccountController> logger,
             IConfiguration config)
		{
            this.accountService = accountService;
            this.logger = logger;
            this.config = config;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDTO registerDTO)
        {
            var result = await accountService.Register(registerDTO);
            int statusCode = (int)result.StatusCode;

            if (statusCode != 200)
            {
                logger.LogError($"AccountService.API - {result.Message}");
            }
            else
            {
                logger.LogInformation($"AccountService.API - {result.Message}");
            }

            return StatusCode(statusCode, result.Message);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDTO loginDTO)
        {
            var result = await accountService.Login(loginDTO, config["JWT:Secret"]);
            int statusCode = (int)result.StatusCode;

            if (statusCode != 200)
            {
                logger.LogError($"AccountService.API - {result.Message}");
            }
            else
            {
                logger.LogInformation($"AccountService.API - {result.Message}");
            }

            return StatusCode(statusCode, result.Message);
        }

        [Authorize]
        [HttpGet("getUserDetails")]
        public async Task<IActionResult> GetUserDetails(string userName)
        {
            try
            {
                var user = await accountService.GetUserByName(userName);
                if (user != null)
                {
                    UserDTO userDTO = new UserDTO()
                    {
                        Id = user.Id,
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        UserName = user.UserName,
                        Email = user.Email
                    };

                    return Ok(userDTO);
                }
                return NotFound($"User with user name - {userName} was not found");
            }
            catch(Exception ex)
            {
                return StatusCode(500, ex.ToString());
            }
        }

        [AllowAnonymous]
        [HttpGet("ping")]
        public IActionResult Ping()
        {
            return Ok();
        }
    }
}

