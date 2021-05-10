
$(document).ready(function() {
    addMessages();
    
    window.chat = createChatController();
    window.chat.startListenWS();

    document.getElementById("btnSend").onclick = function() {
        window.chat.sendMessage();
    }
    document.getElementById("txtMessage").onkeyup = function(event) {
        if (event.key == "Enter") {
            event.preventDefault();
            window.chat.sendMessage();
        }
    }
})

function createChatController() {
    var state = {};

    return {
        webSocket: undefined,
        state: state,
        startListenWS: function() {
            this.webSocket = new WebSocket("wss://localhost:5001/ws");

            this.webSocket.onmessage = function(event) {
                msg = JSON.parse(event.data);
                console.log(msg);
                addMessage(msg);
            }

            this.webSocket.onopen = function() {
                this.webSocket.send("Hello!\0");
            }
        },
        sendMessage: function() {
            let text = document.getElementById("txtMessage").value;
            let author = document.getElementById("author").value;

            msg = {
                author: author,
                text: text,
            };

            this.webSocket.send(JSON.stringify(msg));

            document.getElementById("txtMessage").value = "";
        }
    }
}

function addMessage(msg) {
    let author = msg.author;
    let text = msg.text;

    let chatMessage = `<li>${author}: ${text}</li>`;

    $(`#msgList`).append(chatMessage);
}

function addMessages() {
    fetch("https://localhost:5001/messages")
        .then(function(response) {
            return response.json()
        })
        .then(function(msgs) {
            msgs.forEach(msg => {
                addMessage(msg)
            });
        })
}

// function listenWS() {
//     let webSocket = new WebSocket("wss://localhost:5001/ws");

//     webSocket.onmessage = function(event) {
//         console.log(event.data);
//     }

//     webSocket.onopen = function() {
//         webSocket.send("Hello!\0");
//     }
// }
