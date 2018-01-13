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


    chat.client.fillListOfAllUsers = function (model) {
        if (model != null) {
            $("#UserList").empty();
            $.each(model, function (index, data) {
                if (data.DisplayUserName == $('#UserName').val()) {
                    $("#UserList").append('<li><span>' + data.DisplayUserName + ' (me)</span></li>');
                }
                else {
                    $("#UserList").append('<li><span class="clickable" onclick="SendChatInviteClient(\'' + data.DisplayUserName + '\')">' + data.DisplayUserName + '</span></li>');
                }
            });
        }
    };

    chat.client.redirectToChat = function (chatRoom) {
        window.open('../Chat/StartChat' + '?chatRoomName=' + chatRoom, "_blank");
    }

    chat.client.reciveMessage = function (name, encryptedMessage, signature, chatRoomName) {
        if ($('#ChatRoomName').length && $('#ChatRoomName').val() == chatRoomName) {
            chat.server.recive(name, encryptedMessage, signature);
        }
    }

    chat.client.showMessage = function (name, message) {
        appendText(name, message);
    }

    chat.client.closeWindow = function () {
        window.close();
    }

    chat.client.blockChat = function () {
        $('#btnSend').prop('disabled', true);
    }

    chat.client.allowChat = function () {
        $('#btnSend').prop('disabled', false);
    }
    
    $("#logout").click(function () {
        $.connection.hub.start().done(function () {
            chat.server.logout();
        });
    });
});

function login(userName) {
    var chat = $.connection.chatHub;
        $.connection.hub.start().done(function () {
            chat.server.login(userName);
            console.log("login");
        }).fail(function (reason) {
            console.log("SignalR connection failed: " + reason);
        });
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
    $.connection.hub.start().done(function () {
        chat.server.joinChat(chatRoomName, userName);
    });
}

function sendChatInvite(from, to) {
    var chat = $.connection.chatHub;
    $.connection.hub.start().done(function () {
        chat.server.sendChatInvite(from, to);
    });
   
    alert("Zahtjev je poslan ako odobren razgovor će se otvoriti u novoj kartici.");
}

function sendMsg() {
    console.log("sendMsg");
    var chat = $.connection.chatHub;
    $.connection.hub.start().done(function () {
        chat.server.send($('#Message').val());
        $('#Message').val("");
        //appendText($('#UserName').val(), $('#Message').val());
    });
    
   
}