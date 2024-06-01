using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using AccountService.Library.DTOs;
using AccountService.WebSocket.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AccountService.WebSocket.Controllers
{
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly Library.Services.AccountService _accountService;
        private readonly ILogger<AccountController> _logger;
        private readonly IConfiguration _config;

        public AccountController(Library.Services.AccountService accountService,
                                 ILogger<AccountController> logger,
                                 IConfiguration config)
        {
            _accountService = accountService;
            _logger = logger;
            _config = config;
        }

        [Route("/login")]
        public async Task Login()
        {
            if (HttpContext.WebSockets.IsWebSocketRequest)
            {
                using var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
                var buffer = new byte[1024 * 4];
                var received = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                if (received.MessageType == WebSocketMessageType.Text)
                {
                    var jsonMessage = Encoding.UTF8.GetString(buffer, 0, received.Count);
                    var loginDTO = JsonSerializer.Deserialize<LoginDTO>(jsonMessage);
                    var result = await _accountService.Login1(loginDTO, _config["JWT:Secret"]);

                    var responseBuffer = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(result));
                    await webSocket.SendAsync(new ArraySegment<byte>(responseBuffer), WebSocketMessageType.Text, true, CancellationToken.None);
                }

                await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
            }
            else
            {
                HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            }
        }

        [Authorize]
        [Route("/getUserDetails")]
        public async Task GetUserDetails()
        {
            if (HttpContext.WebSockets.IsWebSocketRequest)
            {
                using var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
                var buffer = new byte[1024 * 4];
                var received = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                if (received.MessageType == WebSocketMessageType.Text)
                {
                    var jsonMessage = Encoding.UTF8.GetString(buffer, 0, received.Count);
                    var request = JsonSerializer.Deserialize<UserDetailsRequest>(jsonMessage);

                    UserDetailsReply reply = new UserDetailsReply() { Succeeded = true };
                    try
                    {
                        var user = await _accountService.GetUserByName(request.UserName);
                        if (user != null)
                        {
                            reply.User = new UserDTO()
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

                    var responseBuffer = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(reply));
                    await webSocket.SendAsync(new ArraySegment<byte>(responseBuffer), WebSocketMessageType.Text, true, CancellationToken.None);
                }

                await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
            }
            else
            {
                HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            }

        }
    }

}