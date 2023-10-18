--[[
	UI对象池管理器
	-- 创建
	require " UI.TestComponent";
	local test = ChatMsgPoolManager.Get(TestComponent, args, callback, data);
	self:AddChild(test);

	-- 回收进池
	test:Close();或者
	-- 不进池直接销毁
	ChatMsgPoolManager.RecyleImmediately(test);
--]]

require "Logic.System.Communication.ChatMsgPool";

ChatMsgPoolManager = {};
local M = ChatMsgPoolManager;

local pools = {}
function M.Get(uiClass, data)
	local key = uiClass.__cname;
	local pool = pools[key];
	if pool == nil then
		pool = ChatMsgPool.New(uiClass);
		pools[key] = pool;
	end

	return pool:Get(data);
end

function M.Recyle(instance)
	if not instance then return end
	local key = instance.__cname;
	local pool = pools[key];
	if pool then
		pool:Recyle(instance);
	else
		instance:RecycleObject();
	end
end
return M
