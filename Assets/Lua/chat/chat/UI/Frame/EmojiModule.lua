--[[
	EmojiModule 面板
]]
local QuickMessageData = require "UI.Communication.Data.QuickMessageData";

local EmojiModule = Class("EmojiModule", UIComponent);
local M = EmojiModule;

function M:InitParams(selfTransform)
	UIComponent.InitParams(self, selfTransform);
end

function M:ParseArgs(args)
	UIComponent.ParseArgs(self, args);
end

function M:GetLayoutPath()
	return "UI/Prefab/Module/Module_CF_Input";
end

function M:GetSkinClass()
	return require "UI.Communication.Skin.EmojiModuleSkin";
end

function M:OnCreate()
	self.emojiScroll = UIScrollView.New(self.skin.emojiTrans);
	self.gifScroll = UIScrollView.New(self.skin.gifTrans);
	self.messageScroll = UIScrollView.New(self.skin.messageTrans);
	self.toggleGroup = UIToggleGroup.New();
	self.toggleGroup:MakeCommonTypeToggle(self.skin.toggleGroup, 3, self.OnToggleChanged, self);
	self.toggleGroup:SelectedIndex(1);
	self:OnToggleChanged(1);
	local bg = self:GetChildByPath("g_var");
	self:SetSize(bg.sizeDelta.x, bg.sizeDelta.y);
end

function M:OnToggleChanged(index)
	if index == 1 then
		self.skin.emojiGameObject:SetActive(true);
		self.skin.gifGameObject:SetActive(false);
		self.skin.messageGameObject:SetActive(false);
	elseif index == 2 then
		self.skin.emojiGameObject:SetActive(false);
		self.skin.gifGameObject:SetActive(true);
		self.skin.messageGameObject:SetActive(false);
	elseif index == 3 then
		self.skin.emojiGameObject:SetActive(false);
		self.skin.gifGameObject:SetActive(false);
		self.skin.messageGameObject:SetActive(true);
	end
	for i = 1, 3 do
		if i == index then
			self.skin.backGround[i].enabled = false;
		else
			self.skin.backGround[i].enabled = true;
		end
	end
end

--可用于数据初始化在OnCreate之后执行
function M:OnStart()
	self:InitScrollView();
	self:RefreshScrollUnit();
end

-- @private 初始化上下两个滑动列表
function M:InitScrollView()
	local param = {};
	param.direction = ScrollPaneCommon.SCROLLPANE_DIR_HORIZEONTAL;
	param.pageModel = ScrollPaneCommon.SCROLLPANE_MODE_ANYPAGE;
	param.unitSkin = require "UI.Communication.Frame.EmojiSmallCell";
	param.pageChangeCallBack = self.PageChangeCallBack;
	param.pageChangeData = self;
	param.itemEvents = {};
	local clickEvent = {};
	param.itemEvents[ButtonEvent.CLICK] = clickEvent;
	self.emojiScroll:SetParams(param);

	param.unitSkin = require "UI.Communication.Frame.GifCell";
	self.gifScroll:SetParams(param);

	param.unitSkin = require "UI.Communication.Frame.QuickMessageCell";
	self.messageScroll:SetParams(param);
end

function M:RefreshScrollUnit()
	self.emojiScroll:ClearUnit();
	local emojiIds = ChatSystem:GetInstance():GetEmojiIds();
	for _, v in pairs(emojiIds) do
		local config = ChatSystem:GetInstance():GetEmojiById(v);
		if config then
			local unit = ScrollUnit.New(config);
			self.emojiScroll:AddUnit(unit);
		end
	end
	local gifIds = ChatSystem:GetInstance():GetGifIds();
	for _, v in pairs(gifIds) do
		local config = ChatSystem:GetInstance():GetEmojiById(v);
		if config then
			local unit = ScrollUnit.New(config);
			self.gifScroll:AddUnit(unit);
		end
	end

	local configs = QuickMessageSystem:GetInstance():GetMessageByType(QuickMessageType.CHAT);
	for _, v in pairs(configs) do
		local unit = ScrollUnit.New(v);
		self.messageScroll:AddUnit(unit);
	end

	self:RefreshPageCount();
end

function M:RefreshPageCount()
	local totalPageCount = self.emojiScroll:GetTotalPageCount();
	if totalPageCount > 1 then
		self.skin.stateGameObject:SetActive(true);
	else
		self.skin.stateGameObject:SetActive(false);
	end

	for k, v in pairs(self.skin.statePicGameObjects) do
		if k > totalPageCount then
			self.skin.statePicGameObjects[k]:SetActive(false);
		end
	end
end

function M:PageChangeCallBack(index)
	if index > TableUtil.TableLength(self.skin.statePicGameObjects) then
		if info then info("state bar item not enough!!!"); end
	end
	for k, v in pairs(self.skin.statePicChooseGameObjects) do
		if k == index then
			self.skin.statePicChooseGameObjects[k]:SetActive(true)
		else
			self.skin.statePicChooseGameObjects[k]:SetActive(false)
		end
	end
end

function M:OnAddDynamicListener()
	UIManager:GetInstance():RegisterComponentClick(self, self.OnCloseSelf, self);
end

function M:OnRemoveDynamicListener()
	UIManager:GetInstance():UnregisterComponentClick(self);
end

function M:OnCloseSelf()
	BroadcastEventManager:GetInstance():Bingo("ChatDialog_EmojiClose", true);
end

return M;