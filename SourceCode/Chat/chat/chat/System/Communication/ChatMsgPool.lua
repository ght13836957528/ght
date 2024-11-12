--[[
	聊天消息数据池
--]]

local Object = require "Framework.Base.Object";
ChatMsgPool = Class("ChatMsgPool", Object);
local M = ChatMsgPool;

function M:Ctor(uiClass)
	self.uiClass = uiClass;
	self.poolKey = uiClass.__cname;

	self.actives = {};
	self.inactives = {};
	self.refCount = 0;
	self.notRefCount = 0;
end

function M:Get(data)
	local instance = nil;
	if self.notRefCount < 1 then
		instance = self.uiClass.New();
	else
		local key = nil;
		for k, v in pairs(self.inactives) do
			key = k;
			instance = v;
			break;
		end
		self.inactives[key] = nil;
		self.notRefCount = self.notRefCount - 1;
	end

	self.refCount = self.refCount + 1;
	self.actives[tostring(instance)] = instance;

	instance:Update(data);

	return instance;
end

function M:Recyle(instance)
	if not instance then return end

	local key = tostring(instance);
	if self.actives[key] then
		self.actives[key] = nil;
		self.refCount = self.refCount - 1;

		self.inactives[key] = instance;
		self.notRefCount = self.notRefCount + 1;
		instance:OnClear();

	elseif not self.inactives[key] then
		instance:OnDestroy();
	end
end

return M