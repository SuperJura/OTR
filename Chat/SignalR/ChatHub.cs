using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;
using Chat.Models;
using OTR;

namespace Chat.SignalR
{
    public class ChatHub : Hub
    {
        public static List<ChatUser> LoginUsers = new List<ChatUser>();
        public static List<ChatUser> ChatingUsers = new List<ChatUser>();
        public static List<ChatInvite> ChatInvites = new List<ChatInvite>();

        
        public void Login(string userName)
        {
            if (LoginUsers.Any(x=> x.DisplayUserName == userName) == false)
            {
                LoginUsers.Add(new ChatUser { DisplayUserName = userName, ChatUserName = Context.User.Identity.Name });
            }
            GetListOfUsers();
        }

        public void JoinChat(string chatRoomName, string UserName)
        {
                if (!String.IsNullOrEmpty(UserName) && LoginUsers.Any(x=>x.DisplayUserName == UserName))
                {
                    if (!ChatingUsers.Any(x => x.DisplayUserName == UserName && x.ChatRoomName == chatRoomName))
                    {
                        ChatUser currentUser = new ChatUser { ChatUserName = Context.User.Identity.Name, ChatRoomName = chatRoomName, DisplayUserName = UserName };
                        ChatUser reciver = ChatingUsers.Find(x => x.ChatUserName != Context.User.Identity.Name && x.ChatRoomName == chatRoomName);
                        if (reciver != null)
                        {
                        // Ako postoji sugovornik u grupi uzmi javni ključ i napravi novi dervirani ključ
                        currentUser.SetDeriveKey(reciver.PublicKey);
                        // Ako sugovornik ima već ključ za potpisivanje uzmi i stavi za ključ za potpisivanje trenutnog korisnika
                        // Ako nema napravi novi ključ i postavi ga
                            if (reciver.KeyForSigning != null)
                            {
                                currentUser.KeyForSigning = reciver.KeyForSigning;
                            }
                            else
                            {
                                currentUser.SetKeyForSigning(Guid.NewGuid().ToString());
                                reciver.KeyForSigning = currentUser.KeyForSigning;
                            }
                            // Ako sugovornik nema dervirani ključ postavi ga koristeči javni ključ trenutnog korisnika
                            if (reciver.CurrentDeriveKey == null)
                            {
                                reciver.SetDeriveKey(currentUser.PublicKey);
                                ChatingUsers[ChatingUsers.IndexOf(reciver)] = reciver;
                            }
                        }
                        else
                        {
                             // Ako sugovornik još nema napravi novi  ključ za potpisivanje
                            currentUser.SetKeyForSigning(Guid.NewGuid().ToString());
                        }

                        ChatingUsers.Add(currentUser);
                        Groups.Add(Context.ConnectionId, chatRoomName);

                    }
                    else
                    {
                        ChatUser currentUser = ChatingUsers.Find(x => x.DisplayUserName == UserName && x.ChatRoomName == chatRoomName);
                        ChatingUsers.Find(x => x.DisplayUserName == UserName && x.ChatRoomName == chatRoomName).ChatUserName = Context.User.Identity.Name;
                        if (currentUser.CurrentDeriveKey == null)
                        {
                            ChatUser reciver = ChatingUsers.Find(x => x.ChatUserName != Context.User.Identity.Name && x.ChatRoomName == chatRoomName);
                            if (reciver != null)
                            {
                                ChatingUsers.Find(x => x.DisplayUserName == UserName && x.ChatRoomName == chatRoomName).SetDeriveKey(reciver.PublicKey);
                            }
                        }
                    }
                }

                GetListOfUsersInRoom(chatRoomName);            
        }

        public void GetListOfUsersInRoom(string chatRoomName)
        {
            Clients.Group(chatRoomName).fillListOfUsers(ChatingUsers.Where(x => x.ChatRoomName == chatRoomName));
        }

        public void GetListOfUsers()
        {
            Clients.All.fillListOfAllUsers(LoginUsers);
        }
        [Authorize]
        public void SendChatInvite(string from, string to)
        {
           
            if (LoginUsers.Any(x => x.DisplayUserName == to))
            {
                ChatInvite invite = new Models.ChatInvite { From = from.Trim(), To = to.Trim(), ChatRoom = Guid.NewGuid().ToString() };
                ChatInvites.Add(invite);
                
                string userId = LoginUsers.Find(x => x.DisplayUserName == to).ChatUserName;
                Clients.User(userId).newChatRequest(invite);
            }
        }

        public void AcceptChatInvite(string chatRoom)
        {
            ChatInvite invite = ChatInvites.FirstOrDefault(x => x.ChatRoom == chatRoom);
            if (invite != null && LoginUsers.Any(x => x.DisplayUserName == invite.To) && LoginUsers.Any(x => x.DisplayUserName == invite.From))
            {
                string to = LoginUsers.Find(x => x.DisplayUserName == invite.To).ChatUserName;
                string from = LoginUsers.Find(x => x.DisplayUserName == invite.From).ChatUserName;
                Clients.User(to).redirectToChat(chatRoom);
                Clients.User(from).redirectToChat(chatRoom);
            }                    
        }

        public void DenyChatInvite(string chatRoom)
        {
                ChatInvite invite = ChatInvites.FirstOrDefault(x => x.ChatRoom == chatRoom);
                if (invite != null)
                {
                    ChatInvites.Remove(invite);
                }            
        }

        public void Send(string message)
        {
            ChatUser sender = ChatingUsers.Find(x => x.ChatUserName == Context.User.Identity.Name);
            string chatRoomName = sender.ChatRoomName;
            string name = sender.DisplayUserName;
            ChatUser reciver = ChatingUsers.Find(x => x.ChatUserName != Context.User.Identity.Name && x.ChatRoomName == chatRoomName);
            if (sender.CurrentDeriveKey != null)
            {
                IM.OTRSend(sender.CurrentDeriveKey, message, out byte[] encryptedMessage);
                byte[] signature = MAC.Sign(sender.KeyForSigning, message);
                Clients.User(reciver.ChatUserName).reciveMessage(name, encryptedMessage, signature, chatRoomName);
            }
            
        }

        public void Recive(string name, byte[] encryptedMessage, byte[] signature)
        {
            ChatUser Reciver = ChatingUsers.Find(x => x.ChatUserName == Context.User.Identity.Name);
            string ChatRoomName = Reciver.ChatRoomName;
            String Message = IM.OTRReceive(Reciver.CurrentDeriveKey, encryptedMessage);
            if (MAC.Verify(Reciver.KeyForSigning, signature))
            {
                NewsDeriveKey(ChatRoomName);
                Clients.Group(ChatRoomName).showMessage(name, Message + " EM: " + System.Text.Encoding.Default.GetString(encryptedMessage));
            }      
        }

        public void NewsDeriveKey(string chatRoomName)
        {
            ChatUser Bob = ChatingUsers.Find(x => x.ChatUserName == Context.User.Identity.Name);
            ChatUser Alice = ChatingUsers.Find(x => x.ChatUserName != Context.User.Identity.Name && x.ChatRoomName == chatRoomName);

            Clients.Group(chatRoomName).blockChat();

            Alice.GenerateNewKey();
            Bob.GenerateNewKey(Alice.PublicKey);
            Alice.SetDeriveKey(Bob.PublicKey);
            Alice.PreviousDeriveKey = null;
            Bob.PreviousDeriveKey = null;
            ChatingUsers[ChatingUsers.IndexOf(Alice)] = Alice;
            ChatingUsers[ChatingUsers.IndexOf(Bob)] = Bob;

            Clients.Group(chatRoomName).allowChat();

        }

        public void Logout()
        {
            if (LoginUsers.Any(x => x.ChatUserName == Context.User.Identity.Name))
            {
                ChatUser user = LoginUsers.Find(x => x.ChatUserName == Context.User.Identity.Name);
                LoginUsers.Remove(user);             
            }
        }


    }
}