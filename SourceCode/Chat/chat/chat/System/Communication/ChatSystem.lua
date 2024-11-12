--[[
	hubin
	消息通过ChatMsgManager来管理
]]
local MsgType = require "Net.Protocols.protolua.ares.logic.msg.msgtype";


local ChatCommon = require "UI.Communication.Common.ChatCommon";
local ChatScrollComponent = require "UI.Component.ScrollView.ChatScrollComponent";
local ChatMsgManager = require "UI.Communication.ChatMsgManager";
local ChatWorldData = require "UI.Communication.Data.ChatWorldData";
local ChatOfflineData = require "UI.Communication.Data.ChatOfflineData";
local Singleton = require "Framework.Base.Singleton";
local FriendInfo = require "UI.Friend.Data.FriendInfoData"
local BattleImageParam = require("Net.Protocols.protolua.ares.logic.msg.battleimageparam");
ChatSystem = Class("ChatSystem", Singleton);

local M = ChatSystem;

-- 聊天频道
local Channel = {
	None   			= 0,
	Team			= 1,
	World 			= 2,
	Friend 			= 3,
	AllExceptNotice = 4,
}

function M:Ctor()
	Singleton.Ctor(self);
	self.chatMsgManagers = {};
	self.recentChatList = {};
	self.recentChatIdList = {};
	self.recentUnknownChatIdList = {};
	self.unreadList = {};
	self.emojiConfigIds = {};
	self.gifConfigIds = {};
	self:Initialize();
	self:InitEmojiConfigList();
	self.currentChannelId = 0;
	self.worldChannelId = 0;
	self.isDialogOpen = false;
	self.addLocalAndOfflineMsgFinished = false;
	self.hasLoadRecentChatList = false;
	self.hasAfterBattle = false;
	self.needAddToRecent = false;
	self.muteInBattle = false;
	self:AddListener();
end

function M:AddListener()
	BroadcastEventManager:GetInstance():AddListener("ServerEnterBattleEvent", self.OnEnterBattleState, self, self);
end

function M:RemoveListener()
	BroadcastEventManager:GetInstance():RemoveListener("ServerEnterBattleEvent", self.OnEnterBattleState, self);
end

function M:Initialize()
	self:InitChatMsgManager();
	self:LoadLocalUnreadList();
	self:LoadLocalOfflineMsg();
end

--[[
	======================================= 公共接口 =============================================
]]

-- ======================================== 开闭面板相关 ==========================================

function M:IsDialogOpen()
	return self.isDialogOpen;
end

function M:OpenChatDialog(chatChannel, focusFriendInfo)
	if not self.isDialogOpen then
		DialogManager:GetInstance():OpenDialog(DialogType.Chat, {channel = chatChannel, info = focusFriendInfo});
		self.isDialogOpen = true;
	else
		BroadcastEventManager:GetInstance():Bingo("ChatDialog_CreatePrivateChat", focusFriendInfo);
	end
end

function M:CloseChatDialog()
	if self.isDialogOpen then
		DialogManager:GetInstance():CloseDialog(DialogType.Chat);
		self.isDialogOpen = false
	end
end

-- ======================================== 表情相关 ==============================================
function M:ReplaceEmojiGifText(content)
	local index = 0;
	local finalContent = ""
	index = self:CheckContentEmoji(content);
	while index ~= nil do
		finalContent = finalContent .. string.sub(content, 1, index - 1);
		local config = self:GetEmojiByEmojiName(string.sub(content, index, index + 2));
		if config then
			local size = 30;
			if config.type == 2 then -- 大表情
				size = 80;
			end

			local emojiStr = StringUtil.CombineStr({"<size=", tostring(size), "><sprite=\"",config.atlasname,"\" anim=\" ", config.indexstart, ", ", config.indexend, ", 10\"></size>"})
			--local emojiStr = "<sprite anim=\" ".. config.indexstart .. ", " .. config.indexend .. ", 15\">"
			if config.type == 2 then -- 大表情
				emojiStr = config.name;
			end
			finalContent = finalContent .. self:_ReplaceEmoji(content, emojiStr);
		else
			finalContent = finalContent..string.sub(content, index, index + 2);
		end
		content = string.sub(content, index + 3, -1);
		index = self:CheckContentEmoji(content);
	end
	finalContent = finalContent .. string.sub(content, 1, -1);
	return finalContent;
end

-- 把文字替换成表情通用接口
function M:ReplaceEmojiText(content)
	local index = 0;
	local finalContent = ""
	index = self:CheckContentEmoji(content);
	while index ~= nil do
		finalContent = finalContent .. string.sub(content, 1, index - 1);
		local config = self:GetEmojiByEmojiName(string.sub(content, index, index + 2));
		if config then
			local size = 30;
			if config.type == 2 then -- 大表情
				size = 80;
			end
			local emojiStr = StringUtil.CombineStr({"<size=", tostring(size) , "><sprite=\"",config.atlasname,"\" anim=\" ", config.indexstart , ", " , config.indexend , ", 10\"></size>"})
			finalContent = finalContent .. self:_ReplaceEmoji(content, emojiStr);
		else
			finalContent = finalContent..string.sub(content, index, index + 2);
		end
		content = string.sub(content, index + 3, -1);
		index = self:CheckContentEmoji(content);
	end
	finalContent = finalContent .. string.sub(content, 1, -1);
	return finalContent;
end

function M:GetEmojiById(id)
	if TableUtil.TableLength(self.emojiConfigIds) == 0 then
		self:InitEmojiConfigList();
	end
	if not self.emojiConfigList[id] then
		if info then info("emoji config not found which id = " .. id);  end
		return;
	end
	return self.emojiConfigList[id];
end

function M:GetEmojiIds()
	if TableUtil.TableLength(self.emojiConfigIds) == 0 then
		self:InitEmojiConfigList();
	end
	return self.emojiConfigIds;
end

function M:GetGifIds()
	if TableUtil.TableLength(self.gifConfigIds) == 0 then
		self:InitEmojiConfigList();
	end
	return self.gifConfigIds;
end

function M:RemoveInvalidTag(content)
	if not content or content == "" then
		return content;
	end
	content = string.gsub(content, "</?[(SIZE)(ALIGN)(ALPHA)(B)(I)(CSPACE)(FONT)(INDENT)(LINE-HEIGHT)(LINE-INDENT)(LINK)(LOWERCASE)(UPPERCASE)(SMALLCAPS)(MARGIN)(MARK)(MSPACE)(NOPARSE)(NOBR)(PAGE)(POS)(SPACE)(S)(U)(STYLE)(SUB)(SUP)(VOFFSET)(WIDTH)(SPRITE)(size)(align)(alpha)(b)(i)(cspace)(font)(indent)(line-height)(line-indent)(link)(lowercase)(uppercase)(smallcaps)(margin)(mark)(mspace)(noparse)(nobr)(page)(pos)(space)(s)(u)(style)(sub)(sup)(voffset)(width)(sprite)].->", "");
	return content;
end

-- ======================================== 替换聊天空格 =========================================

function M:ReplaceSpace(content)
	content = string.gsub(content, " ", "\\u00A0");
	return content;
end

-- ======================================== 接收聊天消息相关 ======================================

--[[
	@public 获取对应roleid的角色的未读信息数量
]]
function M:GetUnreadCountByRoleId(roleid)
	for k, v in pairs(self.unreadList) do
		if v.roleid == roleid then
			return v.count;
		end
	end
	return 0;
end

--[[
	@public 获取总未读信息数量
]]
function M:GetTotalUnreadCount()
	local count = 0
	for k, v in pairs(self.unreadList) do
		count = count + v.count;
	end
	return count;
end

--[[
	@public 获取离线消息缓存
]]
function M:GetOfflineMsgCache()
	return self.offLineMsgCache;
end

--[[
	@public 获取对应频道或者私聊频道的信息管理器
]]
function M:GetChatMsgManagerByChannel(channel, roleid)
	if not self.chatMsgManagers or TableUtil.TableLength(self.chatMsgManagers) <= 0 then
		self:Initialize();
	end
	if self.chatMsgManagers[channel] then
		if channel == ChatChannel.World or channel == ChatChannel.Team or channel == ChatChannel.AfterBattle or channel == ChatChannel.Allies or channel == ChatChannel.AllPlayer or channel == ChatChannel.AllExceptNotice or channel == ChatChannel.InBattle then
			return self.chatMsgManagers[channel];
		else
			if not self.chatMsgManagers[channel][roleid] then
				self.chatMsgManagers[channel][roleid] = ChatMsgManager.New(ChatCommon.MsgLimit, ChatWorldData, channel, roleid);
			end
			return self.chatMsgManagers[channel][roleid];
		end
	end
	if info then info("channel = " .. channel .. " not found!"); end
	return nil;
end

function M:HasAfterBattleChannel()
	return self.hasAfterBattle;
end

--[[
	@public 清空未读信息
]]
function M:ClearUnreadCountByRoleId(roleid)
	for k, v in pairs(self.unreadList) do
		if v.roleid == roleid then
			v.count = 0;
		end
	end
	self:SaveUnreadList();
end

--[[
	@public 加载离线消息缓存列表
	当在线但没加载离线消息时（离线消息在加载聊天面板之后加载）
	如果收到在线消息，先把在线消息转换成离线消息加到离线消息缓存列表里
]]
function M:LoadOfflineMsgCache()
	if self.offLineMsgCache then
		self:OffLineMsgReceive(self.offLineMsgCache);
	end
	self.offLineMsgCache = {};
	LocalSaveChatManager:GetInstance():SaveOfflineMsgCache();
end

-- ======================================== 最近聊天好友相关 ===========================================
--[[
	@public 加载本地的最近聊天列表
]]
function M:LoadLocalRecentChatList()
	if self.hasLoadRecentChatList then
		return;
	end
	self.hasLoadRecentChatList = true;
	local chatList = LocalSaveChatManager:GetInstance():GetLocalRecentChatList();
	if not chatList then
		return;
	end
	if TableUtil.TableLength(chatList) == 0 then
		return;
	end
	for k, v in pairs(chatList) do
		local friendinfo = FriendInfo.New();
		friendinfo:Parse(v);
		local friend = FriendSystem:GetInstance():GetMyFriendByRoleId(friendinfo:GetRoleID());
		local friendIndex = self:GetRecentChatIndex(friendinfo:GetRoleID());
		if friendIndex == -1 then
			if not friend then
				table.insert(self.recentUnknownChatIdList, friendinfo:GetRoleID());
			end
			table.insert(self.recentChatIdList, friendinfo:GetRoleID());
			table.insert(self.recentChatList, friendinfo);
		end
	end
	self:SendRecentChaters();
	BroadcastEventManager:GetInstance():Bingo("ChatDialog_ReloadRecentChatTab");
end

--[[
	@public 获取最近聊天列表
]]
-- TODO 改成用id列表获取最近聊天列表
function M:GetRecentChatList()
	return self.recentChatList;
end

--[[
	@public 获取最近聊天id列表
]]
function M:GetRecentChatIdList()
	return self.recentChatIdList;
end

--[[
	@public 添加至最近聊天列表
	@params data: 如果在世界频道主动发起跟陌生人的聊天，data是FriendInfo，其他情况下均是ChatWorldData
	@params isEmptyChat: 如果在世界频道主动发起跟陌生人的聊天为true，其他情况为false或nil
]]
function M:AddToRecentChat(data, isEmptyChat)
    if not self.hasLoadRecentChatList then
        self:LoadLocalRecentChatList();
    end
	local currentRoleId = UserManager:GetInstance():GetID();
	local id = data:GetRoleID()
	if not isEmptyChat then
		if currentRoleId == data:GetReceiveRoleId() then
			id = data:GetRoleID()
		else
			id = data:GetReceiveRoleId();
		end
	end

	local friend = FriendSystem:GetInstance():GetMyFriendByRoleId(id);

	local index = self:GetRecentChatIndex(id);
	if index > 0 then
		table.remove(self.recentChatList, index);
		table.remove(self.recentChatIdList, index);
	end


	if not friend then
		self:RemoveRecentUnknownChatId(id);
	end

    self:SaveRecentChatListToLocal();
	if not friend then
		friend = FriendInfo.New();
		friend:SetState(0);
		friend:SetOfflineTime(0);
		friend:SetIntimacy(0);
		friend:SetRoleId(data:GetRoleID());
		friend:SetRoleName(data:GetRoleName());
		friend:SetLevel(data:GetRoleLevel());
		friend:SetAvatarId(data:GetAvatarId());
		table.insert(self.recentUnknownChatIdList, id);
	end
	if not friend.unreadCount then
		friend.unreadCount = 0;
	end
	if data.unread == true then
		friend.unreadCount = friend.unreadCount + 1;
	end
	table.insert(self.recentChatIdList, id);
	table.insert(self.recentChatList, friend);
	self:CheckRecentChatSize();
	self:SendRecentChaters();
	BroadcastEventManager:GetInstance():Bingo("ChatDialog_ReloadRecentChatTab");
	BroadcastEventManager:GetInstance():Bingo("ChatDialog_ShowSelection", friend);
	if self.addLocalAndOfflineMsgFinished then
		self.needAddToRecent = false;
	else
		self.needAddToRecent = true;
	end

end

--[[
	@public 根据roleid获取角色在最近聊天列表里面的位置
]]
function M:GetRecentChatIndex(roleid)
	for k, v in pairs(self.recentChatIdList) do
		if roleid == v then
			return k;
		end
	end
	return -1;
end

-- ============================ 频道相关 =====================================================================
--[[
	@public 切换频道公共接口
]]
function M:SwitchChannel(channel,isCreate)
	if channel == ChatChannel.World then
		self.currentChannelId = self.worldChannelId;
	end
	self:SetCurrentChannelType(channel);
end

--[[
	@public 设置当前频道channelId
]]
function M:SetCurrentChannelId(channelId)
	self.currentChannelId = channelId;
	self:GetLocalChatMsg(channelId, ChatChannel.Friend);
end

--[[
	@public 获取当前频道channelId
]]
function M:GetCurrentChannelId()
	return self.currentChannelId;
end

--[[
	@public 设置当前频道类型
]]
function M:SetCurrentChannelType(type)
	self.currentChannelType = type
end

--[[
	@public 获取当前频道类型
]]
function M:GetCurrentChannelType()
	return self.currentChannelType;
end

function M:LeaveAlliesChat()
	local msgManager = self:GetChatMsgManagerByChannel(ChatChannel.Team);
	msgManager:ClearAllMsg();
	BroadcastEventManager:GetInstance():Bingo("ChatDialog_ResetChannelType");
end

function M:OnEnterBattleState()
	local msgManager = self:GetChatMsgManagerByChannel(ChatChannel.Team);
	msgManager:ClearAllMsg();
	msgManager = self:GetChatMsgManagerByChannel(ChatChannel.InBattle);
	msgManager:ClearAllMsg();
	msgManager = self:GetChatMsgManagerByChannel(ChatChannel.Allies);
	msgManager:ClearAllMsg();
	msgManager = self:GetChatMsgManagerByChannel(ChatChannel.AllPlayer);
	msgManager:ClearAllMsg();
	msgManager = self:GetChatMsgManagerByChannel(ChatChannel.AfterBattle);
	msgManager:ClearAllMsg();
	BroadcastEventManager:GetInstance():Bingo("ChatDialog_ResetChannelType");
	self.hasAfterBattle = true;
end

--[[
	======================================= 私有接口 ==============================================
]]

-- ======================================== 表情相关 ==============================================

--[[
	@private 初始化表情
]]
function M:InitEmojiConfigList()
	self.emojiConfigList = BeanConfigManager:GetInstance():GetAllRecorders("ares.logic.chat.cemojiconfig");
	for k, v in pairs(self.emojiConfigList) do
		if v.type == 1 then
			table.insert(self.emojiConfigIds, k);
		else
			table.insert(self.gifConfigIds, k);
		end
	end
end

-- 检查文本内的表情
function M:CheckContentEmoji(content)
	local foundEmoji = false;
	local index = string.find(content, "#");
	if index then
		local num = string.sub(content, index + 1, index + 2);
		if tonumber(num) ~= nil then
			return index;
		end
	end
	return nil
end

-- 替换表情文本
function M:_ReplaceEmoji(str, emojiStr)
	local returnStr = "";
	local sb = StringBuilder:New();
	sb:SetEmoji(str, emojiStr);
	returnStr = sb:GetString(str);
	sb:Recycle();
	return returnStr;
end

function M:GetEmojiByEmojiName(emoji)
	for _, v in pairs(self.emojiConfigList) do
		if v.emoji == emoji then
			return v;
		end
	end
	if info then info("emoji config not found with emoji name: ".. emoji); end
	return nil;
end


-- ======================================== 接收聊天消息相关  ==========================================

--[[
	@private 初始化信息列表
]]
function M:InitChatMsgManager()
	-- 世界聊天管理器
	self.chatMsgManagers = {};
	local manager = ChatMsgManager.New(ChatCommon.MsgLimit, ChatWorldData, ChatChannel.World);
	self.chatMsgManagers[ChatChannel.World] = manager;
	manager = ChatMsgManager.New(ChatCommon.MsgLimit, ChatWorldData, ChatChannel.Team);
	self.chatMsgManagers[ChatChannel.Team] = manager;

	manager = ChatMsgManager.New(ChatCommon.MsgLimit, ChatWorldData, ChatChannel.Allies);
	self.chatMsgManagers[ChatChannel.Allies] = manager;

	manager = ChatMsgManager.New(ChatCommon.MsgLimit, ChatWorldData, ChatChannel.AllPlayer);
	self.chatMsgManagers[ChatChannel.AllPlayer] = manager;

	manager = ChatMsgManager.New(ChatCommon.MsgLimit, ChatWorldData, ChatChannel.AfterBattle);
	self.chatMsgManagers[ChatChannel.AfterBattle] = manager;

	manager = ChatMsgManager.New(ChatCommon.MsgLimit, ChatWorldData, ChatChannel.InBattle);
	self.chatMsgManagers[ChatChannel.InBattle] = manager;

	manager = ChatMsgManager.New(ChatCommon.MsgLimit, ChatWorldData, ChatChannel.None);
	self.chatMsgManagers[ChatChannel.None] = manager;

	-- 预览模块的聊天框信息管理，除了提示以外的信息均显示，最多显示4条
	manager = ChatMsgManager.New(4, ChatWorldData, ChatChannel.AllExceptNotice);
	self.chatMsgManagers[ChatChannel.AllExceptNotice] = manager;

	-- 好友信息管理器 针对不同好友及不同群聊分别创建
	self.chatMsgManagers[ChatChannel.Friend] = {};

	BroadcastEventManager:GetInstance():Bingo("ChatDialog_ResetAllDataSource");
end

--[[
	@private 处理聊天统一接口
]]
function M:ToChat(data, time)
	if not data then
		if info then info("no chat data received"); end
		return;
	end
	data = self:HandleHyperLinkMsg(data);
	local chatWorldData = ChatWorldData.New();
	chatWorldData:Parse(data);

	if FriendSystem:GetInstance():IsInBlackList(chatWorldData:GetRoleID()) then
		return;
	end


	if chatWorldData:GetChannelType() == ChatChannel.World then
		chatWorldData:SetChannelId(self.worldChannelId);
	else
		chatWorldData:SetChannelId(self.currentChannelId);
	end
	if not time and chatWorldData:GetTimeStamp() == 0 then -- 如果是离线消息会提前设置消息时间，如果等于0代表在线消息
		chatWorldData:SetTimeStamp(GSystem.GetServerTimeInSec());
	end
	if time then
		chatWorldData:SetTimeStamp(time / 1000);
	end
	if TableUtil.TableLength(self.chatMsgManagers) <= 0 then
		self:Initialize();
	end
	-- 如果存在chatmsgtype及chatmsgparam则是特殊消息，具体类型参考SpecialChatMsgType
	-- 以是否存在receiveroleid区分是否是私聊
	if data.receiveroleid then
		local currentRoleId = UserManager:GetInstance():GetID();
		local realReceiveRoleId = currentRoleId == data.receiveroleid and data.roleid or data.receiveroleid;
		if not self.chatMsgManagers[chatWorldData.channeltype][realReceiveRoleId] then
			self.chatMsgManagers[chatWorldData.channeltype][realReceiveRoleId] = ChatMsgManager.New(ChatCommon.MsgLimit, ChatWorldData, chatWorldData.channeltype, realReceiveRoleId);
		end
		-- 如果距离上一条消息大于十分钟添加时间提示
		local addTimeNotice = false;
		if not self.chatMsgManagers[chatWorldData.channeltype][realReceiveRoleId]:GetListLast() then
			addTimeNotice = true;
		else
			local lastMsg = self.chatMsgManagers[chatWorldData.channeltype][realReceiveRoleId]:GetListLast();
			if lastMsg.timestamp == 0 then
				addTimeNotice = true;
			end
			if GSystem.GetServerTimeInSec() - lastMsg.timestamp > 600 then
				addTimeNotice = true;
			end
		end
		if addTimeNotice then
			local timeData = {};
			timeData.isNotice = true;
			local year, month, day, hour, minute, second = TimerUtil.SToYMDHMS(chatWorldData:GetTimeStamp());
			timeData.channelid = realReceiveRoleId;
			timeData.channeltype = ChatChannel.World;
			timeData.msg = "<color=#000000FF>" .. tostring(month) .. "-" .. tostring(day) .. " " .. tostring(hour) .. ":" .. tostring(minute);
			self.chatMsgManagers[chatWorldData.channeltype][realReceiveRoleId]:Receive(timeData);
		end
		self.chatMsgManagers[chatWorldData.channeltype][realReceiveRoleId]:Receive(chatWorldData);
	else
		if not self.chatMsgManagers[chatWorldData.channeltype] then
			if info then info("chat msg type not found, channeltype = " .. chatWorldData.channeltype); end
			return;
		end
		self.chatMsgManagers[chatWorldData.channeltype]:Receive(chatWorldData);
		if chatWorldData.channeltype == ChatChannel.Allies or chatWorldData.channeltype == ChatChannel.AllPlayer then
			if self.muteInBattle and chatWorldData:GetRoleID() ~= UserManager:GetInstance():GetID() then
				dibug("msg muted!");
			else
				local finalContent = ChatSystem:GetInstance():ReplaceEmojiText(chatWorldData:GetContent());
				local startIndex, endIndex = string.find(finalContent, "<sprite=\"battle\" anim=\"1,1,\">", 1, true);

				if startIndex and endIndex then
					finalContent = string.sub(chatWorldData:GetContent(), endIndex + 1, -1)
					chatWorldData:SetMessage(finalContent);
				end
				self.chatMsgManagers[ChatChannel.InBattle]:Receive(chatWorldData);
				local inBattleStr = BeanUtil.GetResStringMsg(10311, {chatWorldData:GetRoleName(), finalContent});
				inBattleStr = StringUtil.CombineStr({"<sprite=\"battle\" anim=\"1,1,\">" , inBattleStr});
				BroadcastEventManager:GetInstance():Bingo("InBattleMsgReceived", inBattleStr);
			end
		end
	end
    if self:IsBattleMsg(chatWorldData.channeltype) == false then
        self:AllExceptNoticeChannelRceiveMsg(chatWorldData);
    end

	BroadcastEventManager:GetInstance():Bingo("ChatNewMsg", chatWorldData);
end

function M:IsBattleMsg(channeltype)
    if channeltype == ChatChannel.InBattle or channeltype == ChatChannel.Allies or channeltype == ChatChannel.AllPlayer then
        return true;
    end
    return false;
end

function M:HandleHyperLinkMsg(data)
	if data.param and data.paramtype == SpecialChatMsgType.TeamRecruit then
		local os = LuaProtocolManager.getInstance():GetUnmarshalOS(data.param);
		local TeamRecruit = require("Net.Protocols.protolua.ares.logic.msg.teamrecruitparam");
		local teamRecruit = TeamRecruit.New();
		teamRecruit:unmarshal(os);
		data.recruitInfo = teamRecruit;
		data.msg = BeanUtil.GetResStringMsg(10156, {TeamSystem:GetInstance():GetModeName(data.recruitInfo.modeid), data.recruitInfo.num, data.recruitInfo.limit, data.msg})
	elseif data.param and data.paramtype == SpecialChatMsgType.BattleImage then
		local os = LuaProtocolManager.getInstance():GetUnmarshalOS(data.param);
		local BattleImage = require("Net.Protocols.protolua.ares.logic.msg.battleimageparam");
		local battleimage = BattleImage.New();
		battleimage:unmarshal(os);
		local realMsg = StringUtil.CombineStr({"<sprite=\"battle\" anim=\"", battleimage.param, ",", battleimage.param, ",\">", data.msg});
		--local realMsg = "<sprite=\"battle\" anim=\"" .. battleimage.param .. "," .. battleimage.param .. ",\">" .. data.msg;
		data.msg = realMsg;
	end
	return data;
end

--[[
	@private 主界面聊天框接收消息
]]
function M:AllExceptNoticeChannelRceiveMsg(chatWorldData)
	if chatWorldData and not chatWorldData:IsNotice() then
		if not self.chatMsgManagers[ChatChannel.AllExceptNotice] then
			if info then info("something went wrong, chatMsgManagers[ChatChannel.AllExceptNotice] has been destroyed somewhere"); end
			self.chatMsgManagers[ChatChannel.AllExceptNotice] = ChatMsgManager.New(4, ChatWorldData);
		end
		self.chatMsgManagers[ChatChannel.AllExceptNotice]:Receive(chatWorldData);
	end
end

--[[
	@private 世界频道接收消息
]]
function M:ChatMsgReceive(data)
	if not data then
		if info then info("data missing!!!"); end
		return;
	end
	-- 聊天模块处理消息
	-- info("收到消息 ： " .. TableUtil.TableToString(data));
	self:ToChat(data);
end

--[[
	@private 私聊频道接收消息
]]
function M:PrivateMsgReceive(data)
	if not data then
		if info then info("data missing!!!"); end
		return;
	end

	if not data.chatmsg.channeltype or not data.chatmsg.receiveroleid then
		if info then info("data channeltype or receiverroleid missing!!!"); end
		return;
	end

	if not data.chatmsg then
		if info then info("chat msg does not contain a chatmsg!!!"); end
	end

	local currentRoleId = UserManager:GetInstance():GetID();
	local msg = ChatWorldData.New();
	msg:Parse(data.chatmsg);
	if data.chatmsg.roleid == currentRoleId or (self:GetCurrentChannelId() == data.chatmsg.roleid and DialogManager:GetInstance():IsShow(DialogType.Chat)) then
		msg:SetUnread(false);
	else
		msg:SetUnread(true);
		self:AddToUnreadList(data.chatmsg.roleid);
		self:SaveUnreadList();
	end
	if not self.hasLoadRecentChatList then -- 如果没加载最近聊天的话把消息加在离线消息里，只有主界面的聊天框接收消息
		self:AddOnlineMsgToOfflineMsg(data);
		local chatWorldData = ChatWorldData.New();
		chatWorldData:Parse(data.chatmsg);
		self:AllExceptNoticeChannelRceiveMsg(chatWorldData);
		return;
	end
	self:AddToRecentChat(msg);
	self:SaveRecentChatListToLocal();
	if not self.chatMsgManagers[data.chatmsg.channeltype] then
		self.chatMsgManagers = {}
	end
	if not self.chatMsgManagers[data.chatmsg.channeltype][data.chatmsg.roleid] then
		self.chatMsgManagers[data.chatmsg.channeltype][data.chatmsg.roleid] = ChatMsgManager.New(ChatCommon.MsgLimit, ChatWorldData, data.chatmsg.channeltype, data.chatmsg.roleid);
	end
	self:ToChat(data.chatmsg);
	--BroadcastEventManager:GetInstance():Bingo("ChatDialog_ReloadRecentChatTab");
end

--[[
	@private 接收离线消息
]]
function M:OffLineMsgReceive(data)
	for k, v in pairs(data) do
		local offlineMsg = ChatOfflineData.New();
		offlineMsg:Parse(v);
		if not offlineMsg:GetChannelType() or not offlineMsg:GetReceiveRoleId() then
			if info then info("data channeltype or receiverroleid missing!!!"); end
			return;
		end
		offlineMsg:SetUnread(true);
		local currentRoleId = UserManager:GetInstance():GetID();
		if currentRoleId == offlineMsg:GetReceiveRoleId() and not self.chatMsgManagers[offlineMsg:GetChannelType()][offlineMsg:GetRoleID()] then
			self:GetLocalChatMsg(offlineMsg:GetRoleID(), offlineMsg:GetChannelType());
		elseif not self.chatMsgManagers[offlineMsg:GetChannelType()][offlineMsg:GetReceiveRoleId()] then
			self:GetLocalChatMsg(offlineMsg:GetReceiveRoleId(), offlineMsg:GetChannelType());
		end
		self:ToChat(offlineMsg:GetChatMsg(), offlineMsg:GetTimeStamp());
		self:AddToRecentChat(offlineMsg);
	end
	self.addLocalAndOfflineMsgFinished = true;
	if self.needAddToRecent then
		BroadcastEventManager:GetInstance():Bingo("ChatDialog_ReloadRecentChatTab");
		self.needAddToRecent = false;
	end
	self:SaveRecentChatListToLocal();
end

--[[
	@private 加载未读信息列表
]]
function M:LoadLocalUnreadList()
	local localUnreadList = LocalSaveChatManager:GetInstance():GetLocalUnreadList();
	if localUnreadList then
		self.unreadList = localUnreadList;
	end
end

--[[
	@private 加载离线信息列表
]]
function M:LoadLocalOfflineMsg()
	local offlineMsgCache = LocalSaveChatManager:GetInstance():GetLocalOfflineMsg();
	if offlineMsgCache then
		self.offLineMsgCache = offlineMsgCache;
	end
end

--[[
	@private 当在线但没加载离线消息时（离线消息在加载聊天面板之后加载）
	如果收到在线消息，先把在线消息转换成离线消息加到离线消息缓存列表里
]]
function M:AddOnlineMsgToOfflineMsg(data)
	if not self.offLineMsgCache then
		self.offLineMsgCache = {};
	end
	local time = GSystem.GetServerTimeInMilSec();
	local offlineMsg = ChatOfflineData.New();
	offlineMsg.chatmsg = data.chatmsg;
	offlineMsg.sendtime = time;
	table.insert(self.offLineMsgCache, offlineMsg);
	LocalSaveChatManager:GetInstance():SaveOfflineMsgCache();
end

--[[
	@private 使对应角色私聊的未读信息数量+1
]]
function M:AddToUnreadList(roleid)
	if not self:GetUnreadIndexByRoleId(roleid) then
		local unread = {};
		unread.roleid = roleid;
		unread.count = 1
		table.insert(self.unreadList, unread)
	else
		local index = self:GetUnreadIndexByRoleId(roleid);
		self.unreadList[index].count = self.unreadList[index].count + 1;
	end
end

--[[
	@private 保存未读信息列表到本地并刷新提示
]]
function M:SaveUnreadList()
	LocalSaveChatManager:GetInstance():SaveUnreadListToLocal(self.unreadList);
	BroadcastEventManager:GetInstance():Bingo("CommonChatModule_RefreshChatNotice");
	BroadcastEventManager:GetInstance():Bingo("ChatFriendCell_RefreshNotice");
end

--[[
	@private 获取对应roleid角色在UnreadList里面的index
]]
function M:GetUnreadIndexByRoleId(roleid)
	for k, v in pairs(self.unreadList) do
		if v.roleid == roleid then
			return k;
		end
	end
	return nil;
end

function M:GetLocalChatMsg(channelid, channeltype)
	if self.chatMsgManagers[channeltype] and self.chatMsgManagers[channeltype][channelid] then --有了manager就不用再收一遍离线消息了
		return;
	end
	-- 处理本地存储消息
	local saveList = LocalSaveChatManager:GetInstance():GetChatLog(channelid,channeltype);
	if saveList then
		for i, v in ipairs(saveList) do
			if not self.chatMsgManagers[channeltype][channelid] then
				self.chatMsgManagers[channeltype][channelid] = ChatMsgManager.New(ChatCommon.MsgLimit, ChatWorldData, channeltype, channelid);
			end
			self.chatMsgManagers[channeltype][channelid]:Receive(v);
		end
	end
end

function M:GetIfRecentChatDataReady()
	return self.hasLoadRecentChatList;
end

-- ======================================== 最近聊天好友相关 ===========================================
--[[
	@private 保存最近聊天列表到本地
]]
function M:SaveRecentChatListToLocal()
	LocalSaveChatManager:GetInstance():SaveRecentChatListToLocal(self.recentChatList);
end

function M:RemoveRecentChatId(roleid)
	if not self.hasLoadRecentChatList then
		self:LoadLocalRecentChatList();
	end

	local index = self:GetIndexFromRecentChatIdList(roleid);
	if index < 0 then
		if info then info("not found roleid = " .. roleid .. " in self.recentUnknownChatIdList"); end
		return;
	end

	 table.remove(self.recentChatIdList, index);
	local friend = table.remove(self.recentChatList, index);
    self:SaveRecentChatListToLocal();
	return friend;
end

--[[
	@private 在最近陌生人聊天列表（self.recentUnknownChatIdList）里移除对应roleid的陌生人
]]
function M:RemoveRecentUnknownChatId(roleid)
	if not self.hasLoadRecentChatList then
		self:LoadLocalRecentChatList();
	end

	local index = self:GetIndexFromUnknownChatIdList(roleid);
	if index < 0 then
		if info then info("not found roleid = " .. roleid .. " in self.recentUnknownChatIdList, it's a new conversation"); end
		return nil;
	end
	local friendId = table.remove(self.recentUnknownChatIdList, index);

	return friendId;
end

--[[
	@private 在最近陌生人聊天列表（self.recentUnknownChatIdList）里获取对应roleid陌生人的index
]]
function M:GetIndexFromRecentChatIdList(roleid)
	local index = -1;
	for k, v in pairs(self.recentChatIdList) do
		if v == roleid then
			index = k;
		end
	end
	return index;
end

--[[
	@private 在最近陌生人聊天列表（self.recentUnknownChatIdList）里获取对应roleid陌生人的index
]]
function M:GetIndexFromUnknownChatIdList(roleid)
	local index = -1;
	for k, v in pairs(self.recentUnknownChatIdList) do
		if v == roleid then
			index = k;
		end
	end
	return index;
end

--[[
	@private 检查最近聊天列表大小，如果大于十个，即删除最后一个
]]
function M:CheckRecentChatSize()
	if TableUtil.TableLength(self.recentChatIdList) < 11 then
		return;
	end
	local friend = table.remove(self.recentChatList, 1);
	local roleid = table.remove(self.recentChatIdList, 1);
	local index = self:GetIndexFromUnknownChatIdList(roleid);
	if index < 0 then
		if info then info("not found roleid = ".. roleid .. " in self.recentUnknownChatIdList!!!!"); end
		return;
	end
	table.remove(self.recentUnknownChatIdList, index);

end

-- ============================= 战斗内相关聊天逻辑 =========================================================

function M:GetMuteInBattle()
	return self.muteInBattle;
end

function M:SetMuteInBattle(mute)
	self.muteInBattle = mute;
end

-- ==============================  处理协议相关  =============================================================

function M:SendChatMessage(chatBaseData)
	local realContent = self:RemoveInvalidTag(chatBaseData.msg);
	if not realContent or StringBuilder.utf8len(realContent) <= 0 then
		if info then info("发送的字符串里仅包含特殊字符串或为空！原始字符串为" .. chatBaseData.msg) end
		return;
	end

	chatBaseData.msg = realContent;
	if chatBaseData.channeltype == ChatChannel.World or chatBaseData.channeltype == ChatChannel.Team or chatBaseData.channeltype == ChatChannel.AllPlayer or chatBaseData.channeltype == ChatChannel.Allies or chatBaseData.channeltype == ChatChannel.InBattle or chatBaseData.channeltype == ChatChannel.AfterBattle then
		local p = require("Net.Protocols.protolua.ares.logic.msg.csendchatmsg").Create();
		p.channeltype = chatBaseData.channeltype;
		p.msg = chatBaseData.msg;
		p.param = chatBaseData.param or CsIO.Octets.New();
		p.paramtype = chatBaseData.paramtype or 0;
		LuaProtocolManager:getInstance():send(p);
	elseif chatBaseData.channeltype == ChatChannel.Friend then
		local p = require("Net.Protocols.protolua.ares.logic.friend.csentchatmsg2role").Create();
		p.roleid = chatBaseData.channelid;
		p.msg = chatBaseData.msg;
		LuaProtocolManager:getInstance():send(p);
	end
end

function M:ReceiveChatMessage(data)
	self:ChatMsgReceive(data);
end

function M:ReceivePrivateChatMessage(data)
	self:PrivateMsgReceive(data);
end

function M:ReceiveOffLineMsg(data)
	if self.hasLoadRecentChatList then
		self:OffLineMsgReceive(data);
	else
		local count = TableUtil.TableLength(data);
		if not self.offLineMsgCache then
			self.offLineMsgCache = {};
		end
		for k, v in pairs(data) do
			table.insert(self.offLineMsgCache, v);
		end
		LocalSaveChatManager:GetInstance():SaveOfflineMsgCache();
	end
	for k, v in pairs(data) do
		local offlineMsg = ChatOfflineData.New();
		offlineMsg:Parse(v);
		self:AddToUnreadList(offlineMsg:GetRoleID());
	end
	self:SaveUnreadList();
end

function M:SendChangeWorldChannel(channelId)
	local p = require("Net.Protocols.protolua.ares.logic.msg.cchangeworldchannel").Create();
	p.channelid = channelId;
	LuaProtocolManager:getInstance():send(p);
end

function M:ReceiveChangeWorldChannel(channelId)
	self.currentChannelId = channelId;
	self.worldChannelId = channelId;
	BroadcastEventManager:GetInstance():Bingo("ChatDialog_RefreshChannel");
	local data = {};
	data.isNotice = true;
	data.msg = BeanUtil.GetResStringMsg(10116, {channelId})
	data.channelid = channelId;
	data.channeltype = ChatChannel.World;
	self:ChatMsgReceive(data);
end

function M:SendRecentChaters()
	if TableUtil.TableLength(self.recentUnknownChatIdList) <= 0 then
		return;
	end
	local p = require("Net.Protocols.protolua.ares.logic.friend.crecentchaters").Create();
	p.roleids = self.recentUnknownChatIdList;
	LuaProtocolManager:getInstance():send(p);
end

function M:ReceiveRecentChaters(data)
	for k, v in pairs(data) do
		local index = self:GetRecentChatIndex(v.baseinfo.roleid);
		if index > 0 then
			self.recentChatList[index]:Parse(v);
		end
	end
	BroadcastEventManager:GetInstance():Bingo("ChatDialog_RefreshRecentChatTab");
end

function M:ModifyRecentFriendInfo(roleid, data)
	for k, v in pairs(self.recentChatList) do
		if v:GetRoleID() == roleid then
			v:SetRoleName(data.rolename);
			v:SetAvatarId(data.avatarid);
			v:SetLevel(data.rolelevel);
		end
	end


	BroadcastEventManager:GetInstance():Bingo("ChatDialog_RefreshRecentChatTab");
end

--function M:ReceiveAfterBattleChannelActive()
--	self.hasAfterBattle = true;
--end
-- =========================== 清除逻辑 ========================
--退出登录执行逻辑
function M:Clear()


	self.recentChatList = {};
	self.recentChatIdList = {};
	self.recentUnknownChatIdList = {};
	self.unreadList = {};
	self.emojiConfigIds = {};
	self.gifConfigIds = {};
	self.worldChannelId = 0;
	self.isDialogOpen = false;
	self.addLocalAndOfflineMsgFinished = false;
	self.hasLoadRecentChatList = false;
	self.hasAfterBattle = false;
	self.needAddToRecent = false;
	self.muteInBattle = false;

	self.currentChannelId = 0;
	self.worldChannelId = 0;
	self.isDialogOpen = false;
	self.addLocalAndOfflineMsgFinished = false;
	self.hasLoadRecentChatList = false;
	self.hasAfterBattle = false;
	self.needAddToRecent = false;
	self.muteInBattle = false;
	self.emojiConfigIds = {};
	self.gifConfigIds = {};
	for k, v in pairs(self.chatMsgManagers) do
		if v and v.__cname == "ChatMsgManager" then
			v:Destroy();
		elseif type(v) == "table" then
			for k1, v1 in pairs(v) do
				if v1 and v1.__cname == "ChatMsgManager" then
					v1:Destroy();
				end
			end
		end
	end
	TableUtil.Clear(self.chatMsgManagers);
	self.chatMsgManagers = {};
	self.emojiConfigList = {};
	--BroadcastEventManager:GetInstance():Bingo("ChatDialog_ClearAllFriendList");
	self:RemoveListener();
	Singleton.Clear(self);
end


function M.OnSubmit()
	BroadcastEventManager:GetInstance():Bingo("CommonChatSend");
end

function M.OnKeyboardDone()
	BroadcastEventManager:GetInstance():Bingo("CommonChatSend");
end
--
function M.OnEndEdit()
	BroadcastEventManager:GetInstance():Bingo("CommonChatSend");
end

function M.OnClickRecruit(param)
	TeamSystem:GetInstance():RequestJoinTeam(0, param);
end


-- ======================================= 测试用接口 ===================================

-- 测试发送信息接口
function M:TestChatHyperLink()
	local battleParams = BattleImageParam.New();
	battleParams.param = 1;

	local os = CsIO.OctetsStream.New()
	local _os_ = battleParams:marshal(os);
	local data = {};
	data.msg = "一二三三二一";
	data.param = _os_.Data;
	data.paramtype = SpecialChatMsgType.BattleImage;
	local chatWorldData = ChatWorldData.New();
	chatWorldData:SetMessage(data.msg);
	chatWorldData:SetParam(data.paramtype, data.param);
	chatWorldData:SetChannelId(ChatSystem:GetInstance():GetCurrentChannelId());
	chatWorldData:SetChannelType(ChatChannel.World);
	info("发送测试消息 chatWorldData = " .. TableUtil.TableToString(chatWorldData));
	ChatSystem:GetInstance():SendChatMessage(chatWorldData);
end

-- channelType参考 ChatChannel， ChatChannel.Allies战斗内友军频道，ChatChannel.World世界频道
function M:ShowCustomChat(msg, channelType)
	local chatWorldData = ChatWorldData.New();
	chatWorldData:SetMessage(msg);
	if channelType == ChatChannel.Allies or channelType == ChatChannel.AllPlayer then
		chatWorldData.receiveroleid = nil;
	end
	chatWorldData:SetChannelId(ChatSystem:GetInstance():GetCurrentChannelId());
	chatWorldData:SetChannelType(channelType);
	self:ChatMsgReceive(chatWorldData);
end

function M:SendCustomChat(msg, channelType)
	local chatWorldData = ChatWorldData.New();
	chatWorldData:SetMessage(msg);
	chatWorldData:SetChannelId(ChatSystem:GetInstance():GetCurrentChannelId());

	chatWorldData:SetChannelType(channelType);
	self:SendChatMessage(chatWorldData);
end

return M;