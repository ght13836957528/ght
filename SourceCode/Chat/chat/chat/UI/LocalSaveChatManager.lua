
require "UI.Communication.Common.ChatCommon";
local FriendInfo = require "UI.Friend.Data.FriendInfoData"
require "Logic.System.Communication.ChatMsgPoolManager";
local ChatWorldData = require "UI.Communication.Data.ChatWorldData";
local ChatOfflineData = require "UI.Communication.Data.ChatOfflineData";
--require "UI.Communication.Mod.Photo.PhotoManager";

local Singleton = require "Framework.Base.Singleton";
LocalSaveChatManager = Class("LocalSaveChatManager", Singleton);
local M = LocalSaveChatManager;

--[[
	公共接口
]]
function M:SaveRecentChatListToLocal(recentChatList)
    local currentRoleId = UserManager:GetInstance():GetID();
    local key = "recentChatList" .. "|" .. currentRoleId;
    local t = {}
    local result = json.encode(t);
    LocalStorage.Put(key, result);
    if TableUtil.TableLength(recentChatList) == 0 then
        return;
    end
    for k, v in pairs(recentChatList) do
        local localMsg = self:ConverChatList(v)
        table.insert(t, localMsg);
    end
    result = json.encode(t);
    LocalStorage.Put(key, result);
end

function M:GetLocalRecentChatList()
    local currentRoleId = UserManager:GetInstance():GetID();
    local key = "recentChatList" .. "|" .. currentRoleId;
    local log = LocalStorage.Get(key);
    local list;
    if log then
        list = json.decode(log);
    end

    if list == nil then
        return list;
    end
    -- 还原消息存储格式
    local t = {};
    for i, v in ipairs(list) do
        local item = self:RevertToRecentChat(v);
        table.insert(t, item);
    end
    return t
end

function M:SaveUnreadListToLocal(unreadList)
    local currentRoleId = UserManager:GetInstance():GetID();
    local key = "unreadList" .. "|" .. currentRoleId;
    local t = {}
    local result = json.encode(t);
    LocalStorage.Put(key, result);
    if TableUtil.TableLength(unreadList) == 0 then
        return;
    end
    result = json.encode(unreadList);
    LocalStorage.Put(key, result);
end

function M:GetLocalUnreadList()
    local currentRoleId = UserManager:GetInstance():GetID();
    local key = "unreadList" .. "|" .. currentRoleId;
    local log = LocalStorage.Get(key);
    local list;
    if log then
        list = json.decode(log);
    end
    if list == nil then
        return list;
    end

    return list
end

function M:SaveChatToLocal(channelId, channelType, dataList)
    if not channelId or not channelType or not dataList then
        if info then info("channelId or channelType or dataList is nil!"); end
    end
    local currentRoleId = UserManager:GetInstance():GetID();
    local key = currentRoleId .. "|" .. channelId .. "|" .. channelType;
    local t = {}
    local result = json.encode(t);
    LocalStorage.Put(key, result);
    for k, v in pairs(dataList) do
        local localMsg = self:ConvertMessage(v)
        table.insert(t, localMsg);
    end
    result = json.encode(t);
    LocalStorage.Put(key, result);
end

-- 获取聊天记录（如果聊天列表中不存在这个id且不是群聊，取不到记录）
function M:GetChatLog(channelId,channelType)
    if not channelId or not channelType then return nil end
    local chatList = {};

    local currentRoleId = UserManager:GetInstance():GetID();
    local key = currentRoleId .. "|" .. channelId .. "|" .. channelType;
    local log = LocalStorage.Get(key);
    local t = {};
    local result = json.encode(t);
    LocalStorage.Put(key, result);
    local list;
    if log then
        list = json.decode(log);
    end

    if list == nil then
        return list;
    end

    -- 还原消息存储格式
    local t = {};
    for i, v in ipairs(list) do
        local item = self:RevertToChat(v);
        table.insert(t, item);
    end
    return t
end

function M:SaveOfflineMsgCache()
    local offlineMsgCache = ChatSystem:GetInstance():GetOfflineMsgCache();

    local currentRoleId = UserManager:GetInstance():GetID();
    local key = "offlineMsgCache" .. "|" .. currentRoleId;
    local t = {}
    local result = json.encode(t);
    LocalStorage.Put(key, result);
    if TableUtil.TableLength(offlineMsgCache) == 0 then
        return;
    end
    for k, v in pairs(offlineMsgCache) do
        local localMsg = self:ConverOfflineMsg(v)
        table.insert(t, localMsg);
    end
    result = json.encode(t);
    if not result then
        if info then info("result is nil!!! 很大概率是往json里面传class"); end
    end
    LocalStorage.Put(key, result);
end

function M:GetLocalOfflineMsg()
    local currentRoleId = UserManager:GetInstance():GetID();
    local key = "offlineMsgCache" .. "|" .. currentRoleId;
    local log = LocalStorage.Get(key);
    local list;
    if log then
        list = json.decode(log);
    end
    if list == nil then
        return list;
    end
    local t = {}
    for k, v in pairs(list) do
        local localMsg = self:RevertToOfflineMsg(v)
        table.insert(t, localMsg);
    end

    return t
end

-- 将存储的私聊消息从json还原
function M:RevertToChat(data)
    local chat = ChatWorldData.New();
    chat:Parse(data);
    return chat;
end

function M:RevertToRecentChat(data)
    local friend = FriendInfo.New();
    friend:Parse(data);
    return friend;
end

function M:RevertToOfflineMsg(data)
    local friend = ChatOfflineData.New();
    friend:Parse(data);
    return friend;
end

--[[
	私有接口
]]

function M:ConverChatList(friend)
    local friendInfo = {}
    friendInfo.baseinfo = friend.baseinfo;
    friendInfo.state = friend.state;
    friendInfo.offlinetime = friend.offlinetime;
    friendInfo.intimacy = friend.intimacy;
    return friendInfo;
end

function M:ConvertMessage(message)
    local saveMsg = {}
    if not message then
        if info then info("no ChatMsg was found!"); end
    end
    saveMsg.msg = message.msg;
    saveMsg.channeltype = message.channeltype;
    saveMsg.channelid = message.channelid;
    saveMsg.urlType = message.urlType;
    saveMsg.roleid = message.roleid;
    saveMsg.rolename = message.rolename;
    saveMsg.rolelevel = message.rolelevel;
    saveMsg.avatarid = message.avatarid;
    saveMsg.msgTime = message.time;
    saveMsg.receiveroleid = message.receiveroleid;
    return saveMsg;
end

function M:ConverOfflineMsg(message)
    local saveMsg = {};
    saveMsg.chatmsg = {};
    if not message then
        if info then info("no OfflineChatMsg was found!"); end
    end
    saveMsg.chatmsg.msg = message.chatmsg.msg;
    saveMsg.chatmsg.channeltype = message.chatmsg.channeltype;
    saveMsg.chatmsg.channelid = message.chatmsg.channelid;
    saveMsg.chatmsg.urlType = message.chatmsg.urlType;
    saveMsg.chatmsg.roleid = message.chatmsg.roleid;
    saveMsg.chatmsg.rolename = message.chatmsg.rolename;
    saveMsg.chatmsg.rolelevel = message.chatmsg.rolelevel;
    saveMsg.chatmsg.avatarid = message.chatmsg.avatarid;
    saveMsg.chatmsg.msgTime = message.chatmsg.time;
    saveMsg.chatmsg.receiveroleid = message.chatmsg.receiveroleid;
    saveMsg.sendtime = message.sendtime;
    return saveMsg;
end


return M