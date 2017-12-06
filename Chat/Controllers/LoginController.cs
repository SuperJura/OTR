using System.Web.Mvc;
using Chat.Serivces;
using Chat.Filters;

namespace Chat.Controllers
{
    public class LoginController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        //nema registracije
        public ActionResult Login()
        {
            return View();
        }

        public ActionResult Logout()
        {
            string name = Session["name"].ToString();
            ChatDatabase.getInstance().removeUser(name);
            Session.Remove("name");
            return View("Index");
        }

    }
}