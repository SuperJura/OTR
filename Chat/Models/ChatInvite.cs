using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Chat.Models
{
    public class ChatInvite
    {
        public string From { get; set; }
        public string To { get; set; }

        public string ChatRoom { get; set; }
    }
}