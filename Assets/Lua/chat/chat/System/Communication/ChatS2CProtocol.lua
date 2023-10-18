require "Logic.System.Communication.ChatSystem";
local p = require "Net.Protocols.protolua.ares.logic.msg.ssendchatmsg"
function p:process()
	ChatSystem:GetInstance():ReceiveChatMessage(self);
end

p = require "Net.Protocols.protolua.ares.logic.msg.schangeworldchannel"
function p:process()
	ChatSystem:GetInstance():ReceiveChangeWorldChannel(self.channelid);
end

p = require "Net.Protocols.protolua.ares.logic.friend.sofflinemsgmessagetorole"
function p:process()
	ChatSystem:GetInstance():ReceiveOffLineMsg(self.offlinemsglist);
end

p = require "Net.Protocols.protolua.ares.logic.friend.srecentchaters"
function p:process()
	ChatSystem:GetInstance():ReceiveRecentChaters(self.chaters);
end

p = require "Net.Protocols.protolua.ares.logic.friend.sfriendmessagetorole"
function p:process()
	ChatSystem:GetInstance():ReceivePrivateChatMessage(self);
end