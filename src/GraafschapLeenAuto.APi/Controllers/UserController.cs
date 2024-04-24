using GraafschapLeenAuto.Api.Entities;
using GraafschapLeenAuto.Shared;
using Microsoft.AspNetCore.Mvc;

namespace GraafschapLeenAuto.APi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UserController : ControllerBase
    {
        public static List<User> Users = new List<User>
        {
            new User
            {
                Name = "Name",
                Email = "Mail",
                Password = "Secret password"
            }
        };

        private readonly ILogger<UserController> _logger;

        public UserController(ILogger<UserController> logger)
        {
            _logger = logger;
        }

        [HttpGet(Name = "Users")]
        public IActionResult Get()
        {
            var usersDtos = Users.Select(user => new UserDto
            {
                Name = user.Name,
                Email = user.Email
            });

            return Ok(usersDtos);
        }

        [HttpPost]
        public IActionResult Post([FromBody] User user)
        {
            Users.Add(user);
            return Ok();
        }
    }
}
