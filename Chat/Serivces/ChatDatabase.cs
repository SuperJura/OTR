using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Chat.Serivces
{
    public class ChatDatabase
    {
        static ChatDatabase instance;

        public List<string> currentUsers;

        public static ChatDatabase getInstance()
        {
            if (instance == null) instance = new ChatDatabase();
            return instance;
        }

        public ChatDatabase()
        {
            currentUsers = new List<string>();
        }

        internal void removeUser(string name)
        {
            currentUsers.Remove(name);
        }

        public bool addNewUser(string name)
        {
            if (currentUsers.Contains(name)) return false;

            currentUsers.Add(name);
            return true;
        }

        internal List<string> getAllUsers()
        {
            return currentUsers;
        }
    }
}