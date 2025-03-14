const user = JSON.parse(localStorage.getItem("userdata"));
var this_chat = null;
var chatmate_id = null;
var chatmate = null;
var chats = null;
let list_of_users = [];

var connection = new signalR.HubConnectionBuilder().withUrl("/messagehub").build();
connection.start();

connection.on("RecieveMessage", (chatId, msg) => {
    if (this_chat == null || this_chat.id != chatId) return; 
    console.log(msg);
    if (msg.senderId != user.id) addMessage(msg.text, msg.dateTime, 'beforeend', chatmate_id);
});

function ChangeName() {
    var div = document.getElementById('Yourename');
    div.innerText = user.name;

    var div2 = document.getElementById('youretimeonline');
    var user_date = user.lastLogin.toLocaleString('en-GB', { timeZone: 'UTC' }).slice(0, 10);
    var user_time = user.lastLogin.toLocaleString('en-GB', { timeZone: 'UTC' }).slice(11, 19);
    var user_date_time = user_date + "\n" + user_time;
    div2.innerText = user_date_time;//   user.lastLogin;



    // ¬ыбираем элемент на странице, и мен¤ем содержимое нужного пол¤
    //document.getElementById('Yourename')[0].textContent = " то сказал м¤у?";
}

function removeAllChildNodes(parent) {
    while (parent.firstChild) {
        parent.removeChild(parent.firstChild);
    }
}

async function SetDialogueData(name, id) {
    var t = document.getElementById('messagezone');
    removeAllChildNodes(t);
    t.innerHTML = '<div class="messages"></div>';


    var newname = document.getElementById('chatmatename');
    newname.innerText = name;
    chatmate_id = id;
    var f = await fetch("/getuser?" + new URLSearchParams({
        userId: id
    }), {
        method: "GET",
        headers: { "Accept": "application/json" }
    });
    chatmate = await f.json();

    const cf = await fetch("/getchat?" + new URLSearchParams({
        id1: user.id,
        id2: id
    }), {
        method: "GET",
        headers: { "Accept": "application/json" }
    });
    if (!cf.ok) {
        this_chat = null;
        return;
    }
    this_chat = await cf.json();

    if (this_chat != null) {
        var messagefromuser = await fetch("/getmessages/fromuser?" + new URLSearchParams({
            id: this_chat.id,
            count: 100,
            skip: 0

        }), {
            method: "GET",
            headers: { "Accept": "application/json" }
        });
        message_this_chat = await messagefromuser.json();
        console.log(message_this_chat);
        GenerateMyMessages(message_this_chat);
    }
}



function GenerateMyMessages(message_this_chat) {

    // removeAllChildNodes(document.getElementById('users_id'));

    for (i = 0; i < message_this_chat.length; i++) {
        // var user_date = message_this_chat[i].dateTime.toLocaleString('en-GB', { timeZone: 'UTC' }).slice(0, 10);
        //  var user_time = message_this_chat[i].dateTime.toLocaleString('en-GB', { timeZone: 'UTC' }).slice(11, 19);
        // var user_date_time = user_date + "\n" + user_time;
        addMessage(message_this_chat[i].text, message_this_chat[i].dateTime, 'afterbegin', message_this_chat[i].senderId);
    }

}


async function GetChats() {
    const thisChat_Data = await fetch("/getchats?" + new URLSearchParams({
        userId: user.id
    }), {
        method: "GET",
        headers: { "Accept": "application/json" }
    });
    chats = await thisChat_Data.json();
    SetChats();
}

async function SetChats() {
    // const forhowlong= .lenght;
    for (i = 0; i < chats.length; i++) {
        var varid;
        if (chats[i].users[0] != user.id) {
            varid = chats[i].users[0];
        }
        else {
            varid = chats[i].users[1];
        }

        const getuser = await fetch("/getuser?" + new URLSearchParams({
            userId: varid
        }), {
            method: "GET",
            headers: { "Accept": "application/json" }
        });
        list_of_users.push(await getuser.json());
    }
    GenerateMyChatmate();
}

function GenerateMyChatmate() {

    removeAllChildNodes(document.getElementById('users_id'));

    for (i = 0; i < list_of_users.length; i++) {
        var user_date = list_of_users[i].lastLogin.toLocaleString('en-GB', { timeZone: 'UTC' }).slice(0, 10);
        var user_time = list_of_users[i].lastLogin.toLocaleString('en-GB', { timeZone: 'UTC' }).slice(11, 19);
        var user_date_time = user_date + "\n" + user_time;
        addUser(list_of_users[i].name, user_date_time, list_of_users[i].id);
    }

}


addEventListener("load", () => GetChats());
addEventListener("load", () => ChangeName());

//const changeText = () =>


//Create an array where the message along with it's ID will be stored.
let message = [];

// This fuction will enables us to add the message to the DOM

function addMessage(text, date, pos, id) {
    //Object where message will be stored
    const chat = {
        text,
        id: Date.now(),
        showDate: date//'en-GB' { timeZone: 'UTC' },
    }



    var re = /\n/gi;
    var str = chat.text;
    var newstr = str.replace(re, "</br>");
    console.log(newstr);

    message.push(chat);

    var style;
    if (id != user.id) {
        style = "style=\" right: 0%;\""
    }
    else {
        style = "style=\" left: 47%;\""
    }

    //Render message to the screen
    const list = document.querySelector('.messages');
    list.insertAdjacentHTML(pos,
        `<div class="message-item" ${style} data-key="${chat.id}">
            <div class="message_text">${newstr}</div> 
            <div class="message_time"><small>${chat.showDate.toLocaleString().slice(11, 20)}</small></div> 
        </div>`
    );

}

//Create event listener to detect when a message has been submitted

const div = document.getElementById('sendmessageicon');
div.addEventListener('click', async function (event) {
    event.preventDefault();

    //input to save the message itself
    const input = document.querySelector('.typedMessage');

    //This helps us to detect empty messages and ignore them
    const text = input.value.trim();
    if (text !== '') {

        if (this_chat == null) {
            const thisChat_Data = await fetch("/createprivatechat?" + new URLSearchParams({
                id1: user.id,
                id2: chatmate_id
            }), {
                method: "POST",
                headers: { "Accept": "application/json" }
            });
            if (!thisChat_Data.ok) return;
            this_chat = await thisChat_Data.json();
            list_of_users.push(chatmate);
        }

        await fetch("/sendmessage/touser?" + new URLSearchParams({
            sender: user.id,
            reciever: this_chat.id,
            message: text
        }), {
            method: "POST",
            headers: { "Accept": "application/json" }
        });


        addMessage(text, new Date(Date.now()), 'beforeend',user.id);
        input.value = '';
        input.focus();

    }
});


//Create an array where the user along with it's ID will be stored.
//let users = [];

// This fuction will enables us to add the message to the DOM

function addUser(name, lastLogin, id_user) { //
    //Object where message will be stored
    const thisuser = {
        name,
        lastLogin,
        id_user,
        id: Date.now()
    }

    // users.push(thisuser);

    //const prepared = """ + thisuser.name+ """; ""
    //Render message to the screen
    const list = document.querySelector('.users');
    list.insertAdjacentHTML('beforeend',
        `<div class="users-item" onclick="SetDialogueData(\'${thisuser.name}\',\'${thisuser.id_user}\')" id="${thisuser.id}">
            <div class="user_text">${thisuser.name}</div>            
            <div class="user_time"><small>${thisuser.lastLogin}</small></div> 
        </div>`        // <input type="button"   "value="Do">
    );

}


const findusericon = document.getElementById('findusericon');
findusericon.addEventListener('click', async function (event) {
    event.preventDefault();

    parentElement = document.getElementById('users_id');
    /*  //  parentElement.innerHTML = '';
        const thisuser = users.pop();*/

    removeAllChildNodes(parentElement);


    const username = document.getElementById('finduser_input').value;

    const fatchusername = await fetch("/search?" + new URLSearchParams({
        prompt: username,
        user: user.id
    }), {
        method: "GET",
        headers: { "Accept": "application/json" }
    });

    // console.log(await fatchusername.json());

    const users_array = await fatchusername.json();

    if (users_array.length > 0) {
        for (let i = 0; i < users_array.length; i++) {
            var user_date = users_array[i].lastLogin.toLocaleString('en-GB', { timeZone: 'UTC' }).slice(0, 10);
            var user_time = users_array[i].lastLogin.toLocaleString('en-GB', { timeZone: 'UTC' }).slice(11, 19);
            var user_date_time = user_date + "\n" + user_time;
            addUser(users_array[i].name, user_date_time, users_array[i].id);
        }
    }

    // 


    // const text = "Name";
    // addUser(text,text);

});

async function Exit() {

    await fetch("/logout?" + new URLSearchParams({
        id: user.id
    }), {
        method: "PUT",
        headers: { "Accept": "application/json" }
    });
    document.location.href = "index.html";
}

addEventListener("pagehide", () => { if (document.location.href.endsWith("Chats.html")) Exit(); });



/*

//Create an array where the message along with it's ID will be stored.
let message = [];

// This fuction will enables us to add the message to the DOM

function addMessage(text) {
    //Object where message will be stored
    const chat = {
        text,
        id: Date.now()
    }

    message.push(chat);

    //Render message to the screen
    const list = document.querySelector('.messages');
    list.insertAdjacentHTML('beforeend',
        `<p class="message-item" data-key="${chat.id}">
            <span>${chat.text}</span>
        </p>`

    );

}

//Create event listener to detect when a message has been submitted

const div = document.getElementById('sendmessageicon');
div.addEventListener('click', function (event) {
    event.preventDefault();
    fetch("/sendmessage/touser?" + new URLSearchParams({

    }))

    //input to save the message itself
    const input = document.querySelector('.typedMessage');

    //This helps us to detect empty messages and ignore them
    const text = input.value.trim();

    if (text !== '') {
        addMessage(text);
        input.value = '';
        input.focus();

    }
});
*/