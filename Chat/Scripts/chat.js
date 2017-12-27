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

    chat.client.reciveMessage = function (name, encryptedMessage, signature, chatRoomName) {
        chat.server.recive(name, encryptedMessage, signature, chatRoomName);
    }

    chat.client.showMessage = function (name, message) {
        appendText(name, message);
    }
    $("#logout").click(function () {
        chat.server.logout();
    });
});

function login() {
    var chat = $.connection.chatHub;
    chat.server.login($('#UserName').val());
}

function appendText(name, message) {
    // Html encode display name and message.
    var encodedName = $('<div />').text(name).html();
    var encodedMsg = $('<div />').text(message).html();
    // Add the message to the page.
    $('#discussion').append('<li><strong>' + encodedName
        + '</strong>:&nbsp;&nbsp;' + encodedMsg + '</li>');
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

function sendMsg() {
    chat.server.send($('#Message').val());
    appendText($('#UserName').val(), $('#Message').val());
}