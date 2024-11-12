local ChatWorldData = require "UI.Communication.Data.ChatWorldData"
local Object = require "Framework.Base.Object"
local ChatOfflineData = ReusedClass("ChatOfflineData", Object)
local M = ChatOfflineData;

function M:Ctor()
	self.chatmsg = ChatWorldData.New();
	self.sendtime = 0;
end

function M:Parse(detail)
	self.chatmsg:Parse(detail.chatmsg);
	self.sendtime = detail.sendtime;
end

function M:GetRoleID()
	return self.chatmsg.roleid;
end

function M:GetChannelType()
	return self.chatmsg.channeltype;
end

function M:GetChannelId()
	return self.chatmsg.channelid;
end

function M:GetReceiveRoleId()
	return self.chatmsg.receiveroleid;
end

function M:GetMsg()
	return self.chatmsg.msg;
end

function M:IsSelf()
	return UserManager:GetInstance():GetID() == self.chatmsg.roleid;
end

function M:GetTimeStamp()
	return self.sendtime;
end

function M:GetChatMsg()
	return self.chatmsg;
end

function M:GetRoleName()
	return self.chatmsg.rolename;
end

function M:GetRoleLevel()
	return self.chatmsg.rolelevel;
end

function M:GetAvatarId()
	return self.chatmsg.avatarid;
end

function M:SetUnread(unread)
	self.chatmsg.unread = unread;
end

return M;
