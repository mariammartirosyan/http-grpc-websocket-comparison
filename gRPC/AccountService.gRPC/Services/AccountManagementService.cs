using System;
using AccountService.Library.DTOs;
using Grpc.Core;
using Microsoft.AspNetCore.Authorization;

namespace AccountService.gRPC.Services
{
	public class AccountManagementService : AccountManagement.AccountManagementBase
    {
        private readonly Library.Services.AccountService _accountService;
        private readonly ILogger<AccountManagementService> _logger;
        private readonly IConfiguration _config;

        public AccountManagementService(Library.Services.AccountService accountService,
            ILogger<AccountManagementService> logger,
             IConfiguration config)
		{
            _accountService = accountService;
            _logger = logger;
            _config = config;
        }

        public override async Task<LoginReply> Login(LoginRequest request, ServerCallContext context)
        {
            var result = await _accountService.Login(new LoginDTO(request.UserName, request.Password), _config["JWT:Secret"]);

            return await Task.FromResult(new LoginReply
            {
                Succeeded = (result.StatusCode == Library.DTOs.StatusCode.Success),
                Message = result.Message,
                Token = result.Message
            });
        }

        [Authorize]
        public override async Task<UserDetailsReply> GetUserDetails(UserDetailsRequest request, ServerCallContext context)
        {
            UserDetailsReply reply = new UserDetailsReply() { Succeeded = true };
            try
            {
                var user = await _accountService.GetUserByName(request.UserName);
                if (user != null)
                {
                    reply.User = new User()
                    {
                        Id = user.Id,
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        UserName = user.UserName,
                        Email = user.Email
                    };
                }
                else
                {
                    reply.Succeeded = false;
                    reply.Message = $"User with user name - {request.UserName} was not found";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(DateTime.Now + $" - Get User Details - UserName = {request.UserName}\n" + ex.ToString());
                reply.Succeeded = false;
                reply.Message = ex.ToString();
            }
            return await Task.FromResult(reply);
        }
    }
}