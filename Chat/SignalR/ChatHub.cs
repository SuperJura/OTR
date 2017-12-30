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

        //BUG Ova metoda se nikada ne poziva i zato je LoginUsers uvijek prazan. Nekako treba pozvati ovo. I onda maknuti ChatDatabase jer se u Login controleru puni ta lista.
        public void Login(string userName)
        {
            if (LoginUsers.Any(x=> x.UserName == userName) == false)
            {
                LoginUsers.Add(new ChatUser { UserName = userName, ConnectionId = Context.ConnectionId });
            }
        }

        public void JoinChat(string chatRoomName, string UserName)
        {
                if (!String.IsNullOrEmpty(UserName) && LoginUsers.Any(x=>x.UserName == UserName))
                {
                    if (!ChatingUsers.Any(x => x.UserName == UserName && x.ChatRoomName == chatRoomName))
                    {
                        ChatUser CurrentUser = new ChatUser { ConnectionId = Context.ConnectionId, ChatRoomName = chatRoomName, UserName = UserName };
                        ChatUser Reciver = ChatingUsers.Find(x => x.ConnectionId != Context.ConnectionId && x.ChatRoomName == chatRoomName);
                        if (Reciver != null)
                        {
                        // Ako postoji sugovornik u grupi uzmi javni ključ i napravi novi dervirani ključ
                        CurrentUser.SetDeriveKey(Reciver.PublicKey);
                        // Ako sugovornik ima već ključ za potpisivanje uzmi i stavi za ključ za potpisivanje trenutnog korisnika
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
                            // Ako sugovornik još nema naoravi novi dervirani ključ
                            CurrentUser.SetKeyForSigning(Guid.NewGuid().ToString());
                        }
                        ChatingUsers.Add(CurrentUser);

                        Groups.Add(Context.ConnectionId, chatRoomName);

                    }
                    else
                    {
                        ChatUser CurrentUser = ChatingUsers.Find(x => x.UserName == UserName && x.ChatRoomName == chatRoomName);
                        ChatingUsers.Find(x => x.UserName == UserName && x.ChatRoomName == chatRoomName).ConnectionId = Context.ConnectionId;
                        if (CurrentUser.CurrentDeriveKey == null)
                        {
                            ChatUser Reciver = ChatingUsers.Find(x => x.ConnectionId != Context.ConnectionId && x.ChatRoomName == chatRoomName);
                            if (Reciver != null)
                            {
                                ChatingUsers.Find(x => x.UserName == UserName && x.ChatRoomName == chatRoomName).SetDeriveKey(Reciver.PublicKey);
                            }
                        }
                    }
                }

                GetListOfUsers(chatRoomName);            
        }

        public void GetListOfUsers(string chatRoomName)
        {
            Clients.Group(chatRoomName).fillListOfUsers(ChatingUsers.Where(x => x.ChatRoomName == chatRoomName));
        }

        public void SendChatInvite(string from, string to)
        {
            //BUG Login Users je uvijek prazan
            if (LoginUsers.Any(x => x.UserName == to))
            {
                ChatInvite Invite = new Models.ChatInvite { From = from, To = to, ChatRoom = Guid.NewGuid().ToString() };
                ChatInvites.Add(Invite);
                Clients.Client(LoginUsers.Find(x => x.UserName == to).ConnectionId).newChatRequest(Invite);
            }
        }

        public void AcceptChatInvite(string chatRoom)
        {
            ChatInvite Invite = ChatInvites.Where(x => x.ChatRoom == chatRoom).FirstOrDefault();
            if (Invite != null && LoginUsers.Any(x => x.UserName == Invite.To) && LoginUsers.Any(x => x.UserName == Invite.From))
            {
                Clients.Client(LoginUsers.Find(x => x.UserName == Invite.To).ConnectionId).redirectToChat(chatRoom);
                Clients.Client(LoginUsers.Find(x => x.UserName == Invite.From).ConnectionId).redirectToChat(chatRoom);
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
            ChatUser Sender = ChatingUsers.Find(x => x.ConnectionId == Context.ConnectionId);
            string ChatRoomName = Sender.ChatRoomName;
            string Name = Sender.UserName;
            ChatUser Reciver = ChatingUsers.Find(x => x.ConnectionId != Context.ConnectionId && x.ChatRoomName == ChatRoomName);
            byte[] EncryptedMessage;
            IM.OTRSend(Sender.CurrentDeriveKey, message, out EncryptedMessage);
            byte[] Signature = MAC.Sign(Sender.KeyForSigning, message);
            Clients.Client(Reciver.ConnectionId).reciveMessage(Name, EncryptedMessage, Signature, ChatRoomName);
        }

        public void Recive(string name, byte[] encryptedMessage, byte[] signature)
        {
            ChatUser Reciver = ChatingUsers.Find(x => x.ConnectionId == Context.ConnectionId);
            string ChatRoomName = Reciver.ChatRoomName;
            String Message = IM.OTRReceive(Reciver.CurrentDeriveKey, encryptedMessage);
            if (MAC.Verify(Reciver.KeyForSigning, signature))
            {
                Clients.Group(ChatRoomName).showMessage(name, Message);
            }      
        }

        public void NewsDeriveKey(string chatRoomName)
        {
            ChatUser Bob = ChatingUsers.Find(x => x.ConnectionId == Context.ConnectionId);
            ChatUser Alice = ChatingUsers.Find(x => x.ConnectionId != Context.ConnectionId && x.ChatRoomName == chatRoomName);

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
            if (LoginUsers.Any(x => x.ConnectionId == Context.ConnectionId))
            {
                ChatUser user = LoginUsers.Find(x => x.ConnectionId == Context.ConnectionId);
                LoginUsers.Remove(user);
                //GetListOfUsers(user.ChatRoomName);
            }
        }
        public override System.Threading.Tasks.Task OnDisconnected(bool stopCalled)
        {
            if (ChatingUsers.Any(x => x.ConnectionId == Context.ConnectionId))
            {
                ChatUser user = ChatingUsers.Find(x => x.ConnectionId == Context.ConnectionId);
                ChatingUsers.Remove(user);
                //GetListOfUsers(user.ChatRoomName);
            }
            return base.OnDisconnected(stopCalled);
        }


    }
}