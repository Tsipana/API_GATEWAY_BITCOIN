using Microsoft.AspNetCore.Mvc;

namespace Exchange.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ExchangeController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get()
            => Ok("Exchange Service is running!");
    }
}