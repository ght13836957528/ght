ChatCommon = {};
local M = ChatCommon;

local MsgType = require "Net.Protocols.protolua.ares.logic.msg.msgtype";
local ChatMsgParamType = require "Net.Protocols.protolua.ares.logic.msg.chatmsgparamtype"


ChatChannel = {
	None   			= 0,
	Team			= MsgType.CHANNEL_TEAM,
	World 			= MsgType.CHANNEL_WORLD,
	Friend 			= MsgType.CHANNEL_FRIEND,
	Allies			= MsgType.CHANNEL_ALLIESTEAM,
	AllPlayer		= MsgType.CHANNEL_ALLPLAYERS,
	AfterBattle		= MsgType.CHANNEL_AFTERBATTLE,
	InBattle		= 101,
	AllExceptNotice = 201,
}


SpecialChatMsgType = {
	TeamRecruit 	= ChatMsgParamType.TEAM_RECRUIT;
	BattleImage 	= ChatMsgParamType.BATTLE_IMAGE;
}

-- MINI相关
M.MINI_CHANGE_TIME = 0.2;        	-- mini框缓动时间
M.miniExpandHieght = 230;			-- mini框展开高度（读表）

-- 防止配置不同
local LOW_COUNT = 100;			-- 低端
local MID_COUNT = 100;			-- 中端
local HIGH_COUNT = 300;			-- 高端

M.ChannelCount = 4;				-- 频道总数
M.MiniMsgLimit = 10;			-- 左下角消息数上限
M.MsgLimit = LOW_COUNT;			-- 消息数上限
M.WhisperMsgLimit = LOW_COUNT;	-- 私聊消息数上限

M.DelayHandleTime = 0.2;      -- 延迟处理消息时间
M.DelayHandleLimit = 50; 	  --延迟处理消息上限，超过的丢弃掉最老的消息
M.DelayHandleMiniLimit = 20;

M.DelayHandleShow = 0.2; 		-- 延迟处理消息时间（显示）
M.DelayHandleHide = 1;			-- 延迟处理消息时间（隐藏）

function M.Init()

	local quality = SystemSettingManager:GetInstance():GetValue(StmSetOptionId.PictureQuality);
	local limitCount = 0;
	if quality == StmSetOptionEnum.Low then
		limitCount = LOW_COUNT;
	elseif quality == StmSetOptionEnum.Medium then
		limitCount = MID_COUNT;
	elseif quality == StmSetOptionEnum.High then
		limitCount = HIGH_COUNT;
	else
		limitCount = LOW_COUNT;
	end

	M.MsgLimit = limitCount;
	M.WhisperMsgLimit = limitCount;
end
return M