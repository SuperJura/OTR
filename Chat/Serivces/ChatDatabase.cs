using Chat.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Chat.Serivces
{
    public class ChatDatabase
    {
        static ChatDatabase instance;

        public List<string> currentUsers;
        public List<ChatInvite> ChatInvites;

        public static ChatDatabase getInstance()
        {
            if (instance == null) instance = new ChatDatabase();
            return instance;
        }

        private ChatDatabase()
        {
            currentUsers = new List<string>();
            ChatInvites = new List<ChatInvite>();
        }

        public void removeUser(string name)
        {
            currentUsers.Remove(name);
        }

        public bool addNewUser(string name)
        {
            if (currentUsers.Contains(name)) return false;

            currentUsers.Add(name);
            return true;
        }

        public List<string> getAllUsers()
        {
            return currentUsers;
        }

        public void CreateChatInvite(ChatInvite chatInvite)
        {
            ChatInvites.Add(chatInvite);
        }

        public ChatInvite CheckChatInvite(string name)
        {
            ChatInvite Invite = ChatInvites.Where(x => x.To == name).FirstOrDefault();
            if (Invite != null)
            {
                ChatInvites.Remove(Invite);                
            }
            return Invite;
        }

        public ChatInvite CheckIfInviteAccepted(string name)
        {
            ChatInvite Invite = ChatInvites.Where(x => x.From == name && x.Accepted).FirstOrDefault();
            if (Invite != null)
            {
                ChatInvites.Remove(Invite);              
            }
            return Invite;
        }

        public void AcceptChatInvite(string chatRoomName)
        {
            ChatInvites.Find(x => x.ChatRoom == chatRoomName).Accepted = true;
        }

        public void DenyChatInvite(string chatRoomName)
        {
            ChatInvites.Remove(ChatInvites.Where(x => x.ChatRoom == chatRoomName).FirstOrDefault());
        }
    }
}