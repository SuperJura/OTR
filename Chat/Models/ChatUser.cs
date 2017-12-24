using OTR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Chat.Models
{
    public class ChatUser : User
    {
        public ChatUser() : base()
        {

        }

        public string ConnectionId { get; set; }
        public string UserName { get; set; }
        public string ChatRoomName { get; set; }

    }
}