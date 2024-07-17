using Microsoft.AspNetCore.Mvc;

namespace EProjet.NETCore.Controllers
{
    public class UserController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
