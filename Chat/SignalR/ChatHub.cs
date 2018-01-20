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
        //public void Hello()
        //{
        //    Clients.All.hello();
        //}
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
                    //RUKOVANJE
                    if (!ChatingUsers.Any(x => x.DisplayUserName == UserName && x.ChatRoomName == chatRoomName))
                    {
                        ChatUser CurrentUser = new ChatUser { ChatUserName = Context.User.Identity.Name, ChatRoomName = chatRoomName, DisplayUserName = UserName };
                        ChatUser Reciver = ChatingUsers.Find(x => x.ChatUserName != Context.User.Identity.Name && x.ChatRoomName == chatRoomName);
                        if (Reciver != null)
                        {
                        // Ako postoji sugovornik u grupi uzmi javni ključ i napravi novi dervirani ključ
                        CurrentUser.SetDeriveKey(Reciver.PublicKey);
                        // Ako sugovornik ima već ključ za potpisivanje (MAC) uzmi i stavi za ključ za potpisivanje trenutnog korisnika
                        // Ako nema napravi novi ključ i postavi ga
                            if (Reciver.KeyForSigning != null)
                            {
                                CurrentUser.KeyForSigning = Reciver.KeyForSigning;
                            }
                            else
                            {
                                CurrentUser.SetKeyForSigning(Guid.NewGuid().ToString());
                                Reciver.KeyForSigning = CurrentUser.KeyForSigning;
                            }
                            // Ako sugovornik nema dervirani ključ postavi ga koristeči javni ključ trenutnog korisnika
                            if (Reciver.CurrentDeriveKey == null)
                            {
                                Reciver.SetDeriveKey(CurrentUser.PublicKey);
                                ChatingUsers[ChatingUsers.IndexOf(Reciver)] = Reciver;
                            }
                        }
                        else
                        {
                             // Ako sugovornik još nema napravi novi  ključ za potpisivanje
                            CurrentUser.SetKeyForSigning(Guid.NewGuid().ToString());
                        }

                        ChatingUsers.Add(CurrentUser);
                        Groups.Add(Context.ConnectionId, chatRoomName);

                    }
                    else
                    {
                        ChatUser CurrentUser = ChatingUsers.Find(x => x.DisplayUserName == UserName && x.ChatRoomName == chatRoomName);
                        ChatingUsers.Find(x => x.DisplayUserName == UserName && x.ChatRoomName == chatRoomName).ChatUserName = Context.User.Identity.Name;
                        if (CurrentUser.CurrentDeriveKey == null)
                        {
                            ChatUser Reciver = ChatingUsers.Find(x => x.ChatUserName != Context.User.Identity.Name && x.ChatRoomName == chatRoomName);
                            if (Reciver != null)
                            {
                                ChatingUsers.Find(x => x.DisplayUserName == UserName && x.ChatRoomName == chatRoomName).SetDeriveKey(Reciver.PublicKey);
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
                ChatInvite Invite = new Models.ChatInvite { From = from.Trim(), To = to.Trim(), ChatRoom = Guid.NewGuid().ToString() };
                ChatInvites.Add(Invite);
                
                string UserId = LoginUsers.Find(x => x.DisplayUserName == to).ChatUserName;
                Clients.User(UserId).newChatRequest(Invite);
            }
        }

        public void AcceptChatInvite(string chatRoom)
        {
            ChatInvite Invite = ChatInvites.Where(x => x.ChatRoom == chatRoom).FirstOrDefault();
            if (Invite != null && LoginUsers.Any(x => x.DisplayUserName == Invite.To) && LoginUsers.Any(x => x.DisplayUserName == Invite.From))
            {
                string To = LoginUsers.Find(x => x.DisplayUserName == Invite.To).ChatUserName;
                string From = LoginUsers.Find(x => x.DisplayUserName == Invite.From).ChatUserName;
                Clients.User(To).redirectToChat(chatRoom);
                Clients.User(From).redirectToChat(chatRoom);
            }                    
        }

        public void DenyChatInvite(string chatRoom)
        {
                ChatInvite Invite = ChatInvites.Where(x => x.ChatRoom == chatRoom).FirstOrDefault();
                if (Invite != null)
                {
                    ChatInvites.Remove(Invite);
                }            
        }

        public void Send(string message)
        {
            ChatUser Sender = ChatingUsers.Find(x => x.ChatUserName == Context.User.Identity.Name);
            string ChatRoomName = Sender.ChatRoomName;
            string Name = Sender.DisplayUserName;
            ChatUser Reciver = ChatingUsers.Find(x => x.ChatUserName != Context.User.Identity.Name && x.ChatRoomName == ChatRoomName);
            byte[] EncryptedMessage;
            if (Sender.CurrentDeriveKey != null)
            {
                IM.OTRSend(Sender.CurrentDeriveKey, message, out EncryptedMessage);
                byte[] Signature = MAC.Sign(Sender.KeyForSigning, message);
                Clients.User(Reciver.ChatUserName).reciveMessage(Name, EncryptedMessage, Signature, ChatRoomName);
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
                Clients.Group(ChatRoomName).showMessage(name, Message);
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
                //GetListOfUsers(user.ChatRoomName);
            }
        }
        public override System.Threading.Tasks.Task OnDisconnected(bool stopCalled)
        {
            //if (ChatingUsers.Any(x => x.ChatUserName == Context.User.Identity.Name))
            //{
            //    ChatUser user = ChatingUsers.Find(x => x.ChatUserName == Context.User.Identity.Name);
            //    ChatingUsers.Remove(user);
            //    //Clients.User(Context.User.Identity.Name).closeWindow();
            //    //GetListOfUsers(user.ChatRoomName);
            //}
            return base.OnDisconnected(stopCalled);
        }


    }
}
