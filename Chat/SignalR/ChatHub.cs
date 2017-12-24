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

        public static List<ChatUser> ConnectedUsers = new List<ChatUser>();


        public void JoinChat(string chatRoomName, string UserName)
        {
                if (!String.IsNullOrEmpty(UserName))
                {
                    if (!ConnectedUsers.Any(x => x.UserName == UserName && x.ChatRoomName == chatRoomName))
                    {
                        ChatUser CurrentUser = new ChatUser { ConnectionId = Context.ConnectionId, ChatRoomName = chatRoomName, UserName = UserName };
                        ChatUser Reciver = ConnectedUsers.Find(x => x.ConnectionId != Context.ConnectionId && x.ChatRoomName == chatRoomName);
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
                                ConnectedUsers[ConnectedUsers.IndexOf(Reciver)] = Reciver;
                            }
                        }
                        else
                        {
                            // Ako sugovornik još nema naoravi novi dervirani ključ
                            CurrentUser.SetKeyForSigning(Guid.NewGuid().ToString());
                        }
                        ConnectedUsers.Add(CurrentUser);

                        Groups.Add(Context.ConnectionId, chatRoomName);

                    }
                    else
                    {
                        ChatUser CurrentUser = ConnectedUsers.Find(x => x.UserName == UserName && x.ChatRoomName == chatRoomName);
                        ConnectedUsers.Find(x => x.UserName == UserName && x.ChatRoomName == chatRoomName).ConnectionId = Context.ConnectionId;
                        if (CurrentUser.CurrentDeriveKey == null)
                        {
                            ChatUser Reciver = ConnectedUsers.Find(x => x.ConnectionId != Context.ConnectionId && x.ChatRoomName == chatRoomName);
                            if (Reciver != null)
                            {
                                ConnectedUsers.Find(x => x.UserName == UserName && x.ChatRoomName == chatRoomName).SetDeriveKey(Reciver.PublicKey);
                            }
                        }
                    }
                }

                GetListOfUsers(chatRoomName);            
        }

        public void GetListOfUsers(string chatRoomName)
        {
            Clients.Group(chatRoomName).fillListOfUsers(ConnectedUsers.Where(x => x.ChatRoomName == chatRoomName));
        }

        public void Send(string name, string message, string chatRoomName)
        {
            ChatUser Sender = ConnectedUsers.Find(x => x.ConnectionId == Context.ConnectionId);
            ChatUser Reciver = ConnectedUsers.Find(x => x.ConnectionId != Context.ConnectionId && x.ChatRoomName == chatRoomName);
            byte[] EncryptedMessage;
            IM.OTRSend(Sender.CurrentDeriveKey, message, out EncryptedMessage);
            byte[] Signature = MAC.Sign(Sender.KeyForSigning, message);
            Clients.Client(Reciver.ConnectionId).reciveMessage(name, EncryptedMessage, Signature);
        }

        public void Recive(string name, byte[] encryptedMessage, string chatRoomName, byte[] signature)
        {
            ChatUser Reciver = ConnectedUsers.Find(x => x.ConnectionId == Context.ConnectionId);
            String Message = IM.OTRReceive(Reciver.CurrentDeriveKey, encryptedMessage);
            if (MAC.Verify(Reciver.KeyForSigning, signature))
            {
                Clients.Group(chatRoomName).showMessage(name, Message);
            }      
        }

        public void NewsDeriveKey(string chatRoomName)
        {
            ChatUser Bob = ConnectedUsers.Find(x => x.ConnectionId == Context.ConnectionId);
            ChatUser Alice = ConnectedUsers.Find(x => x.ConnectionId != Context.ConnectionId && x.ChatRoomName == chatRoomName);

            Clients.Group(chatRoomName).blockChat();

            Alice.GenerateNewKey();
            Bob.GenerateNewKey(Alice.PublicKey);
            Alice.SetDeriveKey(Bob.PublicKey);
            Alice.PreviousDeriveKey = null;
            Bob.PreviousDeriveKey = null;
            ConnectedUsers[ConnectedUsers.IndexOf(Alice)] = Alice;
            ConnectedUsers[ConnectedUsers.IndexOf(Bob)] = Bob;

            Clients.Group(chatRoomName).allowChat();

        }

        public override System.Threading.Tasks.Task OnDisconnected(bool stopCalled)
        {
            if (ConnectedUsers.Any(x => x.ConnectionId == Context.ConnectionId))
            {
                ChatUser user = ConnectedUsers.Find(x => x.ConnectionId == Context.ConnectionId);
                ConnectedUsers.Remove(ConnectedUsers.Find(x => x.ConnectionId == Context.ConnectionId));
                GetListOfUsers(user.ChatRoomName);
            }
            return base.OnDisconnected(stopCalled);
        }


    }
}