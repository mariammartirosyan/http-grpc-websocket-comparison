using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AccountService.Library.Services;
using AccountService.Library.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.VisualBasic;
using System.Security.Claims;
using AccountService.Library.DTOs;

namespace AccountService.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AccountController : ControllerBase
    {
        private readonly AccountService.Library.Services.AccountService accountService;
        private readonly ILogger<AccountController> logger;
        private readonly IConfiguration config;

        public AccountController(AccountService.Library.Services.AccountService accountService,
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
            if (result.StatusCode != 200)
            {
                logger.LogError($"AccountService.API - {result.Message}");
            }
            else
            {
                logger.LogInformation($"AccountService.API - {result.Message}");
            }

            return StatusCode(result.StatusCode, result.Message);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDTO loginDTO)
        {
            var result = await accountService.Login(loginDTO, config["JWT:Secret"]);

            if (result.StatusCode != 200)
            {
                logger.LogError($"AccountService.API - {result.Message}");
            }
            else
            {
                logger.LogInformation($"AccountService.API - {result.Message}");
            }

            return StatusCode(result.StatusCode, result.Message);
        }

        [AllowAnonymous]
        [HttpGet("ping")]
        public IActionResult Ping()
        {
            return Ok();
        }
    }
}

