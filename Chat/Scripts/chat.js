$(function () {
    // Declare a proxy to reference the hub.
    var chat = $.connection.chatHub; 
    chat.client.newChatRequest = function (model) {
        if (model != null) {
            if (confirm("Korisnik " + model.From + " vam je poslo zahtjev za razgovor želite li prihvatiti?")) {
                chat.server.acceptChatInvite(model.ChatRoom);
            }
            else {
                chat.server.denyChatInvite(model.ChatRoom);
            }
        }
    };

    chat.client.redirectToChat = function (chatRoom) {
        window.open('../Chat/StartChat' + '?chatRoomName=' + chatRoom, "_blank");
    }

});

function login() {
    var chat = $.connection.chatHub;
    chat.server.login($('#userName').val());
}

function joinChat(chatRoomName,userName) {
    var chat = $.connection.chatHub;
    chat.server.joinChat(chatRoomName, userName);
}

function sendChatInvite(from, to) {
    var chat = $.connection.chatHub;
    chat.server.sendChatInvite(from, to);
    alert("Zahtjev je poslan ako odobren razgovor će se otvoriti u novoj kartici.");
}