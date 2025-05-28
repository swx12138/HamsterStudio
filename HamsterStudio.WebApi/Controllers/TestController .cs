using Microsoft.AspNetCore.Mvc;

namespace HamsterStudio.WebApi.Controllers;

[Route("[controller]")]
public class TestController : Controller
{
    [HttpGet]
    public object Get()
    {
        return new
        {
            Fuck = "You world"
        };
    }
}

