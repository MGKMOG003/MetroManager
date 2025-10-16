using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MetroManager.Web.Controllers
{
    [Authorize(Roles = "Client")]
    public class ClientController : Controller
    {
        [HttpGet]
        public IActionResult Index() => RedirectToAction("Index", "Dashboard");
    }
}
