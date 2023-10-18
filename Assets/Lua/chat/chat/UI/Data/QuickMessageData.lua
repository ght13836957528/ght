local Object = require "Framework.Base.Object"
local QuickMessageData = ReusedClass("QuickMessageData", Object)
local M = QuickMessageData;

function M:Ctor()
	self.id	= 0;
	self.tag = "";
	self.message = "";
	self.type = 0;
end

function M:Parse(detail)
	self.id = detail.id;
	self.tag = detail.tag;
	self.message = detail.message;
	self.type = detail.type;
end

function M:Update(data)
	self:Parse(data);
end

function M:OnClear()
	self.id	= 0;
	self.tag = "";
	self.message = "";
	self.type = 0;
end

function M:GetType()
	return self.type;
end

function M:GetMessage()
	return self.message;
end

function M:GetTag()
	return self.tag;
end

function M:GetShowMsg()
	return self.tag;
end

function M:GetRealMsg()
	return self.message;
end

function M:SetShowMsg(showMsg)
	self.tag = showMsg;
end

function M:SetRealMsg(realMsg)
	self.message = realMsg;
end

return M;
