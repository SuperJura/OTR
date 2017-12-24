using System.Web.Mvc;
using Chat.Serivces;
using System.Collections.Generic;
using Chat.Filters;
using OTR;

namespace Chat.Controllers
{
    public class ChatController : Controller
    {
        public ActionResult Login(string name)
        {
            bool success = ChatDatabase.getInstance().addNewUser(name);
            if (success)
            {
                Session["name"] = name;
                return RedirectToAction("Index");
            }
            else
            {
                ViewBag.ErrorMsg = "Ime je zauzeto";
                return RedirectToAction("Index","Login");
            }
        }

        // GET: Chat
        [AuthFilter]
        public ActionResult Index()
        {
            List<string> allUsers = ChatDatabase.getInstance().getAllUsers();
            return View(allUsers);
        }

        public ActionResult StartChat(string name)
        {
            User user = new User();
            return View();
        }
    }
}