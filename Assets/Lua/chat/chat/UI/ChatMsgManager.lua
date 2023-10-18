--[[
	聊天消息管理器

	管理聊天消息数据，控制消息处理速度，提供相应的聊天消息数据
--]]
require "Framework.Tick.Timer";
require "UI.Communication.Common.ChatCommon";
require "Logic.System.Communication.ChatMsgPoolManager";
--require "UI.Communication.Mod.Photo.PhotoManager";

local Object = require "Framework.Base.Object";
ChatMsgManager = Class("ChatMsgManager", Object);
local M = ChatMsgManager;

local MsgType = require "Net.Protocols.protolua.ares.logic.msg.msgtype";
local ShowInfo = require "Net.Protocols.protolua.ares.logic.msg.showinfo";

local REF_MSG_TIP_ID = 160590;       -- @消息提示ID
local CHAT_TIME_DISPLAY_CD = tonumber(require("Utils.BeanUtil").GetEnumerValue("CHAT_TIME_DISPLAY_CD"));

DirtyType = {
	None = "None", 	   -- 数据没有变化
	Entire = "Entire", -- 整体刷新	
	Insert = "Insert", -- 插入数据
	Remove = "Remove", -- 删除数据	
	Update = "Update", -- 单条数据发生变化
	Locate = "Locate", -- 定位到某条数据
	UpdateIcon = "UpdateIcon",--刷新自定义头像
}

--======================================================================================
--                                      基类方法
--======================================================================================
--[[
	@public 数据是否是变化
--]]
function M:IsDirty()
	return self.dirtyType ~= DirtyType.None;
end

--[[
	@public 获得数据变化类型
--]]
function M:GetDirtyType()
	return self.dirtyType;
end

--[[
	@public 获取数据列表
--]]
function M:GetList()
	return self.dataList and self.dataList or {};
end

--[[
	@public 获得数据列表最后一条数据(最新插入的数据)
--]]
function M:GetListLast()
	return self:GetListAt(self:GetCount());
end

--[[
	@public 获得指定索引数据
--]]
function M:GetListAt(index)
	local list = self:GetList();
	if index < 0 or index > self:GetCount() then
		if error then error("[" .. self.__cname .. "]: index out of range. index = " .. index .. " count = " .. self:GetCount()) end
		return nil;
	end

	return list[index];
end

--[[
	@public 获得数据列表尺寸
--]]
function M:GetCount()
	if self.dataList == nil then
		if info then info("聊天dataList为空，尚未创建") end
		return 0;
	else
		return table.maxn(self.dataList);
	end
end

--[[
	@public 数据变化标志重置
--]]
function M:ResetDirtyType()
	self.dirtyType = DirtyType.None;
end

--[[
	@public 设置数据列表
--]]
function M:SetList(list)
	if list == nil then return end
	self.dataList = list;
	self:SetDirtyType(DirtyType.Entire, 0);
end

--[[
	@public 插入数据
--]]
function M:Insert(data, pos, internal)
	if self.dataList == nil then
		self.dataList = {};
	end

	self.latestIsRef = false;

	-- 消息类型处理
	local urlType = data:GetUrlType();
	urlType = 0;
	-- 文本消息
	if urlType == 0 then
		-- 消息插入列表
		local temp = pos;
		if not temp then
			temp = table.maxn(self.dataList) + 1;
		end
	end


	data.time = nil;
	local hour, min = GSystem.GetServerTimeHMS();
	self.lastShowTime = GSystem.GetServerTimeInSec();
	
	-- 消息插入列表

	if pos then
		table.insert(self.dataList, pos, data);
	else
		table.insert(self.dataList, data);
		pos = table.maxn(self.dataList);
	end
	if not internal then
		self:SetDirtyType(DirtyType.Insert, pos);
	end

	-- 私信存储消息
	self:SaveToLocal();	

	-- 到达上限，删除老消息
	if table.maxn(self.dataList) > self.dataSize then
		self:Remove(1, internal);
	end	
end

function M:IsPublicChannel(msgType)
	if  msgType == MsgType.CHANNEL_TEAM or
		msgType == MsgType.CHANNEL_FACTION or
		msgType == MsgType.CHANNEL_WORLD or
		msgType == MsgType.CHANNEL_CAMP or
		msgType == MsgType.CHANNEL_SCREEN or
		msgType == MsgType.CHANNEL_GROUPCHAT then
		return true;
	end
	return false;
end

--[[
	@public 移除数据
--]]
function M:Remove(pos, internal)
	if self.dataList == nil then return end
	local data = table.remove(self.dataList, pos);
	if data then
		local urlType = data:GetUrlType();
		-- 文本消息
		if urlType == 0 then
			-- 更新@消息的索引
			if self.latestRef then
				if self.latestRef.index > pos then
					self.latestRef.index = self.latestRef.index - 1;
				elseif self.latestRef.index == pos then
					self.latestRef = nil;
				end
			end
		-- 语音消息
		elseif urlType == 1 then
			local url = data:GetVoiceUrl();
			self.voiceDict[url] = nil;
		-- 图片消息
		elseif urlType == 2 then
			--PhotoManager:GetInstance():DeletePhotoMessage(data);
		end

		self.readIndex = self.readIndex - 1;
		if self.readIndex < 0 then
			self.readIndex = 0;
		end

		ChatMsgPoolManager.Recyle(data);

		if not internal then
			self:SetDirtyType(DirtyType.Remove, pos);
		end
	end
end

--[[
	@public 更新索引数据
--]]
function M:Update(data, pos)
	self.dataList[pos] = data;
	self:SaveToLocal(); -- 存储本地消息
	self:SetDirtyType(DirtyType.Update, pos);
end

--[[
	@public 获取消息索引
--]]
function M:GetIndex(data)
	local index = 0;
	for k,v in pairs(self.dataList) do
		if v == data then
			index = k;
		end
	end
	return index;
end

--[[
	@public 定位索引数据
--]]
function M:Locate(pos)
	self:SetDirtyType(DirtyType, pos);
end

--[[
	@public 注册数据变化回调
--]]
function M:Register(callback, ...)
	if self.callbacks == nil then
		self.callbacks = {};
	end

	local key = tostring(callback) .. tostring(...);
	local target = self.callbacks[key];
	if target == nil then
		target = {};
		target.callback = callback;
		target.data = {...};
		self.callbacks[key] = target;
	else
		target.data = {...};
	end
end

--[[
	@public 注销数据变化回调
--]]
function M:Unregister(callback)
	if self.callbacks == nil then return end
	local key = tostring(callback);
	self.callbacks[key] = nil;
end

--[[
	@public 数据源激活（Scroll调用）
--]]
function M:Active()
	if self.isActive then return end
	self.isActive = true;
	self:OnActive();
	self:SetDirtyType(DirtyType.Entire, 0);

	--self:SaveToLocal();
end

--[[
	@public 数据源待机（Scroll调用）
--]]
function M:Inactive()
	if not self.isActive then return end
	self.isActive = false;
	self:OnInactive();
end

--[[
	@private 激活时自定义操作
--]]
function M:OnActive()
end

--[[
	@private 待机时自定义操作
--]]
function M:OnInactive()
end

--[[
	@private 数据源变化
--]]
function M:SetDirtyType(dirtyType, index)
	self.dirtyType = dirtyType;
	if not self.callbacks then return end
	for k, v in pairs(self.callbacks) do
		if v.callback then
			if self.roleId == ChatSystem:GetInstance():GetCurrentChannelId() or self.channel ~= ChatChannel.Friend then

				v.callback(dirtyType, index, unpack(v.data));
			end
		end
	end
end

--群聊时roleId指的是群id
function M:Ctor(size, dataClass, channel, roleId)
	self.dataList = {};						-- 消息列表
	self.dataClass = dataClass; 			-- 消息类型
	self.dataSize = size and size or 10;	-- 消息上限
	self.isActive = false;	-- 数据源是否在使用
	self.dirtyType = DirtyType.None;	-- 数据变化类型

	self.channel = channel;	-- 消息频道
	self.dataQueue = {};	-- 消息数据队列，用于前期控制数据的处理速度
	self.readIndex = 0;		-- 已读消息的最大索引值
	self.voiceDict = {};	-- 语音消息映射列表（url为key）

	self.lastShowTime = 0;

	self.roleId = roleId and roleId or 0;

	self.timer = Timer.New(ChatCommon.DelayHandleShow, M.DelayHandle, self, true);
	self.timer:Start();

	self:AddListener();
end

function M:AddListener()
	BroadcastEventManager:GetInstance():AddListener("PlayerLeaveScene", M.OnChangeScene, self, self);
end

function M:RemoveListener()
	BroadcastEventManager:GetInstance():RemoveListener("PlayerLeaveScene", M.OnChangeScene, self);
end

function M:Destroy()
	self:RemoveListener();

	for i, v in ipairs(self.dataList) do
		ChatMsgPoolManager.Recyle(v);
	end

	self.callbacks = nil;
	self.dataList = nil;
	self.lastShowTime = 0;

	if self.timer then
		self.timer:Stop();
		self.timer = nil;
	end
end

function M.OnChangeScene(object, data, self)
	if self.channel ~= nil and self.channel ~= MsgType.CHANNEL_SCREEN then return end
	self:ClearDelay();
	self:SetDirtyType(DirtyType.Entire, 0);
end

function M:ClearAllMsg()
	self.dataList = {};

	self:ClearDelay();
	self:SetDirtyType(DirtyType.Entire, 0);
end
--======================================================================================
--                                      子类方法
--======================================================================================

--[[
	@public 获得未读消息数量（仅供在线消息使用）
--]]
function M:GetUnreadCount()
	return table.maxn(self.dataList) - self.readIndex;
end

--[[
	@public 接受消息数据
--]]
function M:Receive(data)
	if not data then return end
	-- 当前频道/世界频道/Mini，控制聊天消息处理速度
	if self.channel == ChatChannel.World or self.channel == ChatChannel.AllExceptNotice or self.channel == nil then
		-- 频道在激活状态，控制速度
		if self.isActive then
			if not self.dataQueue then
				self.dataQueue = {};
			end
			table.insert(self.dataQueue, data);
			
		-- 频道在非激活状态，直接处理数据
		else
			self:ClearDelay();
			self:Handle(data);
		end		
	else
		self:Handle(data);
	end
end

--[[
	@public 设置最新的已读消息索引，并处理@消息列表
--]]
function M:SetReadIndex(index)
	if self.latestRef and self.latestRef.index <= index then
		self.latestRef = nil;
	end

	if self.readIndex > index then return end
	self.readIndex = index;
end

--[[
	@public 触发消息定位
--]]
function M:TriggerLocate()
	if self.latestRef then
		local index = self.latestRef.index;
		self:SetDirtyType(DirtyType.Locate, index);
	elseif self.readIndex < table.maxn(self.dataList) then
		self:SetDirtyType(DirtyType.Entire);
	end
end

--[[
	@private 控制消息数据处理
--]]
function M:DelayHandle()
	if not self.dataQueue then return end

	local len = table.maxn(self.dataQueue);

	if len <= 0 then return end

	-- 抛弃部分旧消息（大于接受上限的）
	if len > ChatCommon.MsgLimit then
		for i = len, len - ChatCommon.MsgLimit, -1 do
			table.remove(self.dataQueue, 1);
		end
	end

	len = table.maxn(self.dataQueue);

	-- 抛弃部分旧消息（自己发言除外）
	if len > ChatCommon.DelayHandleLimit then
		local temp = {};
		local count = len - ChatCommon.DelayHandleLimit;
		for i = len, 1, -1 do
			if i < count then
				local isSelf = UserManager and UserManager:HasInstance() and (UserManager:GetInstance():GetID() == self.dataQueue[i].roleid);
				if not isSelf then table.remove(self.dataQueue, 1); end
			end
		end
	end

	local data = table.remove(self.dataQueue, 1);
	self:Handle(data);
end

--[[
	@private 真正处理消息数据
--]]
function M:Handle(data, internal)
	if not data then return end

	local msg = ChatMsgPoolManager.Get(self.dataClass, data);
	self:CheckHasRename(data);
	self:Insert(msg, nil, internal);
end

function M:CheckHasRename(data)
	if not data or not data.rolename or not data.receiveroleid then
		return;
	end

	local list = self:GetList();
	if not list or TableUtil.TableLength(list) == 0 then
		return;
	end

	local function CheckParamSame(dataInList, param)
		local found = false
		if dataInList and dataInList.roleid and dataInList[param] then
			if dataInList.roleid == data.roleid and data[param] ~= dataInList[param] then
				dataInList[param] = data[param];
				found = true;
			end
		end
		return found;
	end
	local foundName = false;
	local foundAvatar = false;
	local foundLevel = false;
	local needRefresh = false;
	for k, v in pairs(list) do
		foundName = CheckParamSame(v, "rolename");
		foundAvatar = CheckParamSame(v, "avatarid");
		foundLevel = CheckParamSame(v, "rolelevel");
		if foundName or foundAvatar or foundLevel then
			needRefresh = true;
		end
	end

	if needRefresh then
		self:SetDirtyType(DirtyType.Entire, 0);
		ChatSystem:GetInstance():ModifyRecentFriendInfo(data.roleid, data);
		FriendSystem:GetInstance():ModifyFriendInfoByRoleId(data.roleid, data);
		return;
	end

end

--[[
	@private 处理消息队列
--]]
function M:ClearDelay()
	if not self.dataQueue then return end
	while table.maxn(self.dataQueue) > 0 do
		local data = table.remove(self.dataQueue, 1);
		self:Handle(data, true);
	end
end

-- 私聊和群聊存储本地聊天信息
function M:SaveToLocal()
	if self.channel ~= MsgType.CHANNEL_FRIEND and self.channel ~= MsgType.CHANNEL_TEAM then return end

	if not self.dataList or TableUtil.TableLength(self.dataList) == 0 or self.roleId == 0 then return end

	local saveList = {};
	local total = TableUtil.TableLength(self.dataList);
	local count = 0;
	for i = total, 1, -1 do
		if count >= 10 then
			break;
		end

		local data = self.dataList[i];
		if not data.isNotice then
			table.insert(saveList, 1, data);
			count = count + 1;
		end
	end
	LocalSaveChatManager:GetInstance():SaveChatToLocal(self.roleId, self.channel, saveList);
end

return M