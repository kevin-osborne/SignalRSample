using Microsoft.AspNetCore.Mvc;

namespace SignalRSample.Controllers
{
    public class HomeController : Controller
    {
        [Route("/")]
        public ActionResult Index()
        {
            return View();
        }
    }
}
