using System.Web.Mvc;
using Chat.Serivces;
using System.Collections.Generic;
using Chat.Filters;
using OTR;
using System;

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

        //public void SendChatInvite(string to)
        //{
        //    ChatDatabase.getInstance().CreateChatInvite(
        //        new Models.ChatInvite { From = Session["name"] as string, To = to, ChatRoom = Guid.NewGuid().ToString() });
        //}

        //public JsonResult CheckChatInvite()
        //{
        //    return Json(ChatDatabase.getInstance().CheckChatInvite(Session["name"] as string));
        //}

        //public JsonResult CheckIfInviteAccepted()
        //{
        //    return Json(ChatDatabase.getInstance().CheckIfInviteAccepted(Session["name"] as string));
        //}

        //public void AcceptChatInvite(string chatRoomName)
        //{
        //    ChatDatabase.getInstance().AcceptChatInvite(chatRoomName);
        //}
        //public void DenyChatInvite(string chatRoomName)
        //{
        //    ChatDatabase.getInstance().DenyChatInvite(chatRoomName);
        //}

        public ActionResult StartChat(string chatRoomName)
        {
            ViewBag.ChatRoomName = chatRoomName;
            return View();
        }
    }
}