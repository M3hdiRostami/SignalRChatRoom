
var currentChannel;
var JoinedChannelList;
var currentChannelStatusSelector = $("#currentChannelStatus");
var currentChannelnameSelector = $("#currentChannelname");
let channelssContainerSelector = $("#chats ul");
let messagessContainerSelector = $("#messagesContainer");

var connection = new signalR
	.HubConnectionBuilder()
	.withUrl("/chatHub")
	.withAutomaticReconnect()
	.configureLogging(signalR.LogLevel.Information)
	.build();

function RenderNewMessage(message) {
	console.log("newmessage");
	console.log(message);
	
	let messagesListSeceltor = $(".messageList[data-contact-id='" + message.GroupName + "']");	

	let rowFormat = "<li class='<M-Cssclass>'><img src='<M-Image>' alt='<M-Sender>' /><p><span class='username'><M-Sender>:</span><M-Content></p><time><M-Time></time></li>";

	messagesListSeceltor.append(
		rowFormat
			.replaceAll("<M-Sender>", message.Sender)
			.replaceAll("<M-Time>", message.SentTime)
			.replaceAll("<M-Cssclass>", message.Sender == me() ? "sent" : "replies")
			.replaceAll("<M-Content>", message.Content)
			.replaceAll("<M-Image>", message.Sender == "notifier" ? "/avatars/users/notifier.png":"/avatars/users/default.jpg")

	);


	let relatedChannelSelector = $(".contact[data-contact-id='" + message.GroupName + "']");

	relatedChannelSelector.find(".preview").html('<span>' + message.Sender + ': </span>' + message.Content);
	if (message.GroupName != currentChannel && message.Sender != "notifier") {
		playSound("/sounds/newmessage.mp3");
		relatedChannelSelector.find(".contact-status").addClass("online");
		relatedChannelSelector.find(".name").append("<code class='flash'>{new message}</code>");
	}

}

function RenderGroupsList(list) {
			
	channelssContainerSelector.html("");

	let rowFormat = "<li class='<G-cssclass>' data-contact-id='<G-name>'> <div class='wrap'><span class='contact-status'></span><img class='<G-joinstatus>' src='<G-image>' alt='<G-name>' /><div class='meta' ><p class='name'><G-name><code>{members:<G-memberCount>}</code></p><p class='preview' ></p></div></div></li>";
	let messagesRowFormat = "<ul class='messageList' data-contact-id='<G-name>'></ul >";
	for (let i = 0; i < list.length; i++)
	{
		
		channelssContainerSelector.append(
			rowFormat
				.replaceAll("<G-name>", list[i].GroupName.toLowerCase())
				.replaceAll("<G-cssclass>", list[i].GroupName == currentChannel ? "contact active" : "contact")
				.replaceAll("<G-joinstatus>", JoinedChannelList?.filter(G => G.GroupName === list[i].GroupName).length ? "online" : "away")
				.replaceAll("<G-memberCount>", list[i].MembersCount)
				.replaceAll("<G-image>", "/avatars/groups/" + list[i].GroupAvatar)
			
		);
	
		if (!messagessContainerSelector.has(".messageList[data-contact-id='" + list[i].GroupName + "']").length)
		{
			messagessContainerSelector.append(
				messagesRowFormat
					.replaceAll("<G-name>", list[i].GroupName)
			);
		}

	}
}
function SendMessage() {
	let message = $("#messageInput").val();
	if (message)
	{
		connection.invoke("sendToGroup", { type: "sendToGroup", sender: me(), ConnectionId: connection.connection.connectionId, content: message, groupName: currentChannel }).catch(function (err) {
			return console.error(err.toString());
		});
		$("#messageInput").val(null);
	}
}
function playSound(url) {
	const audio = new Audio(url);
	audio.play();
}
function me()
{

	return document.getElementById("userInput").value;
}
function ReceiveMyJoinedGroupsList() {
	connection.invoke("SendMyJoinedGroupsList",null).catch(function (err) {
		return console.error(err.toString());
	});
}
function loadGroupsList() {
	connection.invoke("loadGroupsList").catch(function (err) {
		return console.error(err.toString());
	});
}
connection.on("ReceiveUserAction", function (user, message) {
    let msg = message.replace(/&/g, "&amp;").replace(/</g, "&lt;").replace(/>/g, "&gt;");
	let encodedMsg = user + " " + msg;
	
	currentChannelStatusSelector.text(encodedMsg);
	setTimeout(function () { currentChannelStatusSelector.text("") }, 1500);
});
function ShowChannelMessageHistory(channelName)
{
	let currentchannelOBJ = JoinedChannelList.filter(G => G.GroupName === channelName);

	let messagesListSeceltor = $(".messageList[data-contact-id='" + channelName + "']");
	messagesListSeceltor.html(null);
	currentchannelOBJ[0].Messages.forEach(m => RenderNewMessage(m));
}
connection.on("ReceiveMessage", function (message) {
	messagejson = JSON.parse(message);
	//load message history for joined channel
	if (messagejson.Type == "join"
		&& messagejson.ConnectionId == connection.connection.connectionId
		&& messagejson.GroupName == currentChannel)
	{

		ShowChannelMessageHistory(currentChannel);
		
	}
	//show current message
	RenderNewMessage(messagejson);
	$(".messages").animate({ scrollTop: $(document).height() }, "fast");

});

connection.on("ReceiveMyJoinedGroupsList", function (list) {
	JoinedChannelList = JSON.parse(list);

	console.log("JoinedChannelList");
	console.log(JoinedChannelList);
});
connection.on("loadGroupsList", function (list) {
	
	listjson = JSON.parse(list);
	RenderGroupsList(listjson);
});

connection.start().then(function () {

	
	loadGroupsList();
	ReceiveMyJoinedGroupsList();

}).catch(function (err) {
    return console.error(err.toString());
});

$("#sendButton").click(function (event) {
	SendMessage();
    event.preventDefault();
});
$("#messageInput").on('keydown', function (e) {
	if (e.which == 13) {
		SendMessage();
		return false;
	}
}); 

$("#searchInput").on("keyup", function (event) {

	let searchedName = $(this).val().trim();
	if (searchedName.length >= 2)
	{
		let relatedChannelSelector = $(".contact[data-contact-id*='" + searchedName + "']");
		relatedChannelSelector.siblings().fadeOut();
		relatedChannelSelector.fadeIn();

	}
	else
	{
		$(".contact").show();
    }

});

$("#messageInput").on("keypress", function (event) {
	let user = me();
    let message = "is typing...";
    connection.invoke("SendUserAction", user, message).catch(function (err) {
        return console.error(err.toString());
    });
   
});

$("#leaveChannel").click(function () {

	let relatedChannelMessagesSelector = $(".messageList[data-contact-id='" + currentChannel + "']");
	relatedChannelMessagesSelector.removeClass("active");
	currentChannelnameSelector.text("Please select a group to join the conversation");

	connection.invoke("LeaveGroup", currentChannel, null, me()).catch(function (err) {
		return console.error(err.toString());
	});
})
	
$("#chats").on("click", "ul li.contact", function () {
	//refresh list
	loadGroupsList();
	//know selected chat
	currentChannel = $(this).attr("data-contact-id");
	//mark selected chat
	$(this).siblings().removeClass("active");
	$(this).addClass("active");

	//load channel info
	currentChannelImage = $(this).find("img").attr("src");
	$('.contact-profile img').prop("src", currentChannelImage);
	$(this).find(".contact-status").removeClass("online");
	currentChannelnameSelector.text(currentChannel);

	//join channel if not joined already
	let currentchannelOBJ = JoinedChannelList.filter(G => G.GroupName === currentChannel);
	if (currentchannelOBJ.length == 0) {
		connection.invoke("JoinGroup", currentChannel, null, me()).catch(function (err) {
			return console.error(err.toString());
		});
	}
	

	//show selected channel messages
	let relatedChannelMessagesSelector = $(".messageList[data-contact-id='" + currentChannel + "']");
	relatedChannelMessagesSelector.siblings().removeClass("active");
	relatedChannelMessagesSelector.addClass("active");
	

	
});



$("#profile-img").click(function () {
	$("#status-options").toggleClass("active");
});


$(".expand-button").click(function () {
	$("#profile").toggleClass("expanded");
	$("#contacts").toggleClass("expanded");
});


$("#status-options ul li").click(function () {
	$("#profile-img").removeClass();
	let profileStatus;
	$(this).siblings().removeClass("active");
	$(this).addClass("active");

	if ($("#status-online").hasClass("active")) {
		profileStatus="online";
	} else if ($("#status-away").hasClass("active")) {
		profileStatus="away";
	} else if ($("#status-busy").hasClass("active")) {
		profileStatus="busy";
	} else if ($("#status-offline").hasClass("active")) {
		profileStatus="offline";
	} 
	
	$("#Userstatus").text(profileStatus);
	$("#profile-img").addClass(profileStatus);
	$("#status-options").removeClass("active");
});
