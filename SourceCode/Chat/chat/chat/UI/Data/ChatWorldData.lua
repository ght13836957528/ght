local ChatBaseData = require "UI.Communication.Data.ChatBaseData"
local ChatWorldData = ReusedClass("ChatWorldData", ChatBaseData)
local M = ChatWorldData;

function M:Ctor()
	ChatBaseData.Ctor(self);
	self.roleid = 0;
	self.rolename = "";
	self.sex = 0;
	self.rolelevel = 0;
	self.avatarid = 0;
	self.timestamp = 0;
	self.receiveroleid = 0;
	self.channelid = 0;
	self.unread = false;
	self.recruitInfo = nil;
end

function M:Parse(detail)
	ChatBaseData.Parse(self, detail);
	self.roleid = detail.roleid;
	self.rolename = detail.rolename;
	self.sex = detail.sex;
	self.rolelevel = detail.rolelevel;
	self.avatarid = detail.avatarid;
	self.receiveroleid = detail.receiveroleid;
	if detail.timestamp then
		self.timestamp = detail.timestamp;
	end
	if detail.recruitInfo then
		self.recruitInfo = detail.recruitInfo;
	end
	if detail.channelid then
		self.channelid = detail.channelid;
	end
end

function M:ParseImagePlayerData(detail)
	self.roleid = detail:GetKey();
	self.rolename = detail:GetName();
	self.rolelevel = detail:GetLevel();
	self.avatarid = detail:GetAvatarId(); --detail.avatarid;
end

function M:SetChannelId(channelId)
	self.channelid = channelId;
end

function M:GetChannelId()
	return self.channelid;
end

function M:GetChannelName()
	if self:GetChannelType() == ChatChannel.World then
		--return BeanUtil.GetResStringMsg(10307, {self:GetChannelId()});
		return BeanUtil.GetResStringMsg(10403);

	elseif	self:GetChannelType() == ChatChannel.Team then
		return BeanUtil.GetResStringMsg(10402);
	elseif self:GetChannelType() == ChatChannel.Friend then
		return BeanUtil.GetResStringMsg(10138);
	elseif self:GetChannelType() == ChatChannel.Allies then
		return BeanUtil.GetResStringMsg(10380);
	else
		return BeanUtil.GetResStringMsg(10310);
	end
end

function M:GetChannelHyperTextIcon()
	if self:GetChannelType() == ChatChannel.World then
		return "<size=20><sprite=\"battle\" name=\"ui_comm_chat_sj\"></size>"
	elseif	self:GetChannelType() == ChatChannel.Team then
		return "<size=20><sprite=\"battle\" name=\"ui_comm_chat_dw\"></size>"
	elseif self:GetChannelType() == ChatChannel.Friend then
		return "<size=20><sprite=\"battle\" name=\"ui_comm_chat_hy\"></size>"
	elseif self:GetChannelType() == ChatChannel.Allies then
		return "<size=20><sprite=\"battle\" name=\"ui_comm_chat_dw\"></size>"
	else
		return "<size=20><sprite=\"battle\" name=\"ui_comm_chat_xt\"></size>"
	end
end

function M:GetChannelTextColor()
	if self:GetChannelType() == ChatChannel.World then
		return "<color=#FFC954>"
	elseif	self:GetChannelType() == ChatChannel.Team then
		return "<color=#86D9FF>"
	elseif self:GetChannelType() == ChatChannel.Friend then
		return "<color=#6EF787>"
	elseif self:GetChannelType() == ChatChannel.Allies then
		return "<color=#86D9FF>"
	else
		return "<color=#FF8B82>"
	end
end

function M:GetFinalText()
	if self:GetChannelType() == ChatChannel.World then
		return "<color=#FFC954>"
	elseif	self:GetChannelType() == ChatChannel.Team then
		return "<color=#86D9FF>"
	elseif self:GetChannelType() == ChatChannel.Friend then
		return "<color=#6EF787>"
	else
		return "<color=#FF8B82>"
	end
end

function M:GetRoleID()
	return self.roleid;
end

function M:GetRoleName()
	return self.rolename;
end

function M:GetRoleSex()
	return self.sex;
end

function M:GetRoleLevel()
	return self.rolelevel;
end

function M:GetAvatarId()
	return self.avatarid;
end

function M:GetReceiveRoleId()
	return self.receiveroleid;
end

function M:IsSelf()
	return UserManager:GetInstance():GetID() == self.roleid;
end

function M:GetTimeStamp()
	return self.timestamp;
end

function M:SetTimeStamp(timeStamp)
	self.timestamp = timeStamp;
end

function M:SetUnread(unread)
	self.unread = unread;
end

return M;
