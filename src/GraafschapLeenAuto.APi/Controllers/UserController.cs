using GraafschapLeenAuto.Api.Entities;
using GraafschapLeenAuto.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace GraafschapLeenAuto.APi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UserController : ControllerBase
    {
        private readonly UserService userService;

        public UserController(UserService userService)
        {
            this.userService = userService;
        }

        [HttpGet]
        public IActionResult GetUsers()
        {
            var users = userService.GetUsers();
            return Ok(users);
        }

        [HttpGet("{id}")]
        public IActionResult GetUser(int id)
        {
            return Ok();
        }

        [HttpPost]
        public IActionResult CreateUser([FromBody] User user)
        {
            var createdUser = userService.CreateUser(user);

            return Ok(createdUser); 
        }

        [HttpPut("{id}")]
        public IActionResult UpdateUser(int id, [FromBody] User user)
        {
            return Ok();
        }
    }
}
