local Object = require "Framework.Base.Object"
local ChatBaseData = ReusedClass("ChatBaseData", Object)
local M = ChatBaseData;

local MsgType = require "Net.Protocols.protolua.ares.logic.msg.msgtype";

local CHARACTER_LIMIT = 210; -- 字数上限，lua中一个中文字占3个字节，控制显示70个中文字

function M:GetName()
	return nil;
end

function M:GetMsgType()
	return self.msgType;
end

function M:GetContent()
	return self.msg;
end

function M:GetMessage()
	return self.msg;
end

function M:GetShowInfo()
	if not self.showInfos then return nil end;
	return self.showInfos[1];
end

function M:GetVoiceUrl()
	return self.url;
end

function M:GetVoiceTime()
	return self.voicetime;
end

-- 消息类型（0文本消息| 1语音消息 | 2图片消息）
function M:GetUrlType()
	return self.urlType;
end

function M:IsVoice()
	if self.url == "" then
		return false;
	else
		return true;
	end
end

-- 消息传递的链接数据
function M:GetLink()
	return self.link;
end

function M:IsSelf()
	return false;
end

function M:IsNotice()
	return self.isNotice;
end

function M:GetChannelType()
	return self.channeltype;
end

function M:GetChannelId()
	return self.channelid;
end

function M:Ctor()
	self.msg = "";
	self.channeltype = 0;
	self.channelid = 0;
	self.urlType = 0;
	self.isNotice = false;
	self.time = 0;
	self.paramtype = 0;
	self.param = CsIO.Octets.New();

end

function M:Parse(detail)
	self.msg = detail.msg;
	self.channeltype = detail.channeltype;
	self.channelid = detail.channelid;
	self.urlType = detail.urlType;
	self.isNotice = detail.isNotice;
	self.paramtype = detail.paramtype or self.paramtype;
	self.param = detail.param or self.param;
end

function M:SetMessage(msg)
	self.msg = msg;
end

function M:SetChannelType(channeltype)
	self.channeltype = channeltype;
end

function M:SetParam(type, param)
	self.paramtype = type;
	self.param = param;
end

function M:SetChannelId(channelid)
	self.channelid = channelid;
end

function M:SetTime(time)
	self.time = time;
end

function M:Update(data)
	self:Parse(data);
end

function M:OnClear()
    self.msg = nil;
    self.channeltype = nil;
    self.channelid = nil;
    self.urlType = nil;
end

return M;
