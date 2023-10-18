--[[
	ChatDialog 面板
]]
local ChatMsgCell = require "UI.Communication.Frame.ChatMsgCell";
local ChatCommon = require "UI.Communication.Common.ChatCommon";
local ChatWorldData = require "UI.Communication.Data.ChatWorldData";
local ChatBaseData = require "UI.Communication.Data.ChatBaseData";
local FriendBaseInfo = require "Net.Protocols.protolua.ares.logic.friend.friendbaseinfo"
local FriendApplyInfo = require "Net.Protocols.protolua.ares.logic.friend.friendapplyinfo"
local ChatScrollComponent = require "UI.Component.ScrollView.ChatScrollComponent";
local MsgType = require "Net.Protocols.protolua.ares.logic.msg.msgtype";
local ShowInfo = require "Net.Protocols.protolua.ares.logic.msg.showinfo";
local EmojiModule = require "UI.Communication.Frame.EmojiModule"
local RoleCardModule = require "UI.Communication.Frame.RoleCardModule";
local ChatDialog = Class("ChatDialog", UIDialog);
local CommonKeyboardFrame = require "UI.Communication.Frame.CommonKeyboardFrame";
local M = ChatDialog;

local ChannelUIIndex = {
	World = 1,
	Team = 2,
	Friend = 3,
	AfterBattle = 4,
}

-- 聊天频道

function M:GetLayoutPath()
	return "UI/Prefab/Win/Win_CF";
end
function M:GetSkinClass()
	return require "UI.Communication.Skin.ChatDialogSkin";
end

function M:OnInit()
    self.isEmojiVisible = false;
	self.isKeyVisible = false;
	self.isMoving = false;
	self.isMoreInfoVisible = false;
	self.refreshTime = 0;
	self.hasTeam = false;
	self.hasAfterBattle = false;
	self.isPrivateChat = nil;
	self.cachedPlayerData = nil;
	self.needOpenChannelType = 0;
	self.lastChannel = ChannelUIIndex.World;
	self.focusFriendInfo = nil;
end

function M:OnCreate()
	self.firstCellInfo = nil;
	self.currentFriendInfo = nil;

	self.needResetAllDataSource = false;
	self.addFriendBtn = UIButton.New(self.skin.addFriendBtnTrans);
	self.interactBtn = UIButton.New(self.skin.interactBtnTrans);
	self.closeBtn = UIButton.New(self:GetChildByPath("g_var/ebtn_close"));
	self.emojiBtn = UIButton.New(self:GetChildByPath("g_var/g_input/ebtn_emjoy"));
	self.micBtn = UIButton.New(self:GetChildByPath("g_var/g_input/ebtn_mic"));
	self.voiceBtn = UIButton.New(self:GetChildByPath("g_var/g_input/ebtn_voice"));
	self.sendBtn = UIButton.New(self:GetChildByPath("g_var/g_input/Group_Btn_L_a/ebtn"));
	self.channelBtn = UIButton.New(self:GetChildByPath("g_var/g_world/InputField/ebtn_empty"));
	self.sendBtn:SetText(BeanUtil.GetResStringMsg(10115));
	self.skin.keyboardModuleGameObject:SetActive(false);
	self.commonKeyboardFrame = CommonKeyboardFrame.New();
	self.keyboardModule = UIComponent.New(self.skin.keyboardModule);
	self.keyboardModule:AddChild(self.commonKeyboardFrame);
	--频道
	local params = {};
	params.cell = ChatMsgCell;
	params.size = 10;
	params.lineSpace = -10;
	params.diffWidth = 0;
	params.tipBtn = self.skin.worldTipTrans;
	self.worldChatScroll = ChatScrollComponent.New(self.skin.worldScrollTrans, params);
	self.offsetMax1=self.worldChatScroll:GetRectTransform().offsetMax;
	self.offsetMax2=Vector2.New(self.offsetMax1.x,self.offsetMax1.y-38);
	params.tipBtn = self.skin.teamTipTrans;
	self.teamChatScroll = ChatScrollComponent.New(self.skin.teamScrollTrans, params);
	params.tipBtn = self.skin.afterBattleTipTrans;
	self.afterBattleChatScroll = ChatScrollComponent.New(self.skin.afterBattleScrollTrans, params);

	params.tipBtn = self.skin.friendTipTrans;
	params.diffWidth = -100;
	self.friendChatScroll = ChatScrollComponent.New(self.skin.friendScrollTrans, params);

	self.scrollList = {};
	self.scrollList[ChannelUIIndex.World] = self.worldChatScroll;
	self.scrollList[ChannelUIIndex.Team] = self.teamChatScroll;
	self.scrollList[ChannelUIIndex.Friend] = self.friendChatScroll;
	self.scrollList[ChannelUIIndex.AfterBattle] = self.afterBattleChatScroll;

	self.channelGameObjectList = {};
	self.channelGameObjectList[ChannelUIIndex.World] = self.skin.worldGameObject;
	self.channelGameObjectList[ChannelUIIndex.Team] = self.skin.teamGameObject;
	self.channelGameObjectList[ChannelUIIndex.Friend] = self.skin.friendGameObject;
	self.channelGameObjectList[ChannelUIIndex.AfterBattle] = self.skin.afterBattleGameObject;

	self:ResetAllDataSource();


	self.toggleGroup = UIToggleGroup.New();
	self.toggleGroup:MakeCommonTypeToggle(self.skin.toggleGroup, 3, self.OnToggleChanged, self);
	self.skin.friendVoiceGameObject:SetActive(false);
	self:SwitchChannel(ChatChannel.World);
	self.toggleGroup:SelectedIndex(ChannelUIIndex.World);


	self.recentBtn = UIButton.New(self.skin.recentDropDownBtn);
	self.friendBtn = UIButton.New(self.skin.friendDropDownBtn);

	self.moduleCommonCell = require("UI.Common.Module.ModuleCommCell").New(self.skin.interactTrans);
	self.moduleCommonCell:SetOptions({BeanUtil.GetResStringMsg(10132), BeanUtil.GetResStringMsg(10133), BeanUtil.GetResStringMsg(10134)}, self.OnClickMoreInfo, self, 1);
    self.skin.noneText.text = BeanUtil.GetResStringMsg(10135);
	self.skin.recentTxt.text = BeanUtil.GetResStringMsg(10137);
	self.skin.friendTxt.text = BeanUtil.GetResStringMsg(10138);
	self.skin.friendNumTxt.text = BeanUtil.GetResStringMsg(10131, {tostring(0), tostring(0)});
	self.skin.recentNumTxt.text = BeanUtil.GetResStringMsg(10131, {tostring(0), tostring(0)});

	self.emojiNode = UIComponent.New(self.skin.emojiTrans);
	self.emojiModule = EmojiModule.New();
	self.emojiNode:AddChild(self.emojiModule);
	self.emojiModule:Hide();
	self.emojiBtn:SetRaycastTargetEnable(true);

	self.friendUnitList = {};
	self.recentUnitList = {};
	self.skin.channelTxt[1].text = BeanUtil.GetResStringMsg(10403);
	self.skin.channelTxt[2].text = BeanUtil.GetResStringMsg(10402);
	self.skin.channelTxt[3].text = BeanUtil.GetResStringMsg(10404);
end

function M:OnParse(data)
    if data then
        self.needOpenChannelType = data.channel;
        self.focusFriendInfo = data.info;
    end

end

function M:OnStart()
	--初始化最近聊天及好友列表
	self.friendMyFriendScroll = UIScrollView.New(self.skin.friendMyFriendTrans);
	self.friendRecentScroll = UIScrollView.New(self.skin.friendRecentTrans);
	local param = {};

	param.direction = ScrollPaneCommon.SCROLLPANE_DIR_VERTICAL;
	param.unitSkin = require "UI.Communication.Frame.ChatFriendCell";
	param.itemEvents = {};
	local clickEvent = {};
	clickEvent.onSingleClickCallBack = self.OnFriendClick;
	clickEvent.singleClickCallBackParam = self;
	param.itemEvents[ButtonEvent.CLICK] = clickEvent;
	self.friendRecentScroll:SetParams(param);
	self.friendMyFriendScroll:SetParams(param);
	self.skin.inputField.characterLimit = BeanUtil.GetEnumerNumValue("CHAT_LIMIT_LENGTH");

	--local chatListWidth = self.skin.friendListNode.sizeDelta.x;
	--local chatListHeight = self.skin.friendListNode.parent.parent.parent.sizeDelta.y + self.skin.friendListNode.parent.parent.sizeDelta.y + self.skin.friendListNode.parent.sizeDelta.y + self.skin.friendListNode.parent.parent.parent.parent.sizeDelta.y - 75;
	--self.skin.friendListRect.sizeDelta = Vector2.New(chatListWidth, chatListHeight);
end

function M:OnShow()
	if not self.isMoving then
		self:RefreshChannelId();
		self.isMoving = true;
		UITweenManager:GetInstance():StartTween(0, 0.3, self:GetTransform(), UITweenType.XY, Vector2.New(-1000, 0), Vector3.New(0, 0, 0), "inOutCubic", nil, self.OnFinishOpen, self);
		UIManager:GetInstance():RegisterComponentClick(self, self.OnClickClose, self);
	end
	if self.needResetAllDataSource then
		self:ResetAllDataSource();
		self.needResetAllDataSource = false;
	end

	self.skin.interactGameObject:SetActive(false);
	self.isMoreInfoVisible = false;
	local hasLoadFriendList = FriendSystem:GetInstance():GetIsFriendDataReady();
	if not hasLoadFriendList then
		FriendSystem:GetInstance():RequestFriendPanel();
	else
		self:OnReceiveFriend();
	end
	self:OnClickRecentDropDown();

	self:HandleTeamChange(TeamSystem:GetInstance():HasTeam());
	self:HandleAfterBattleChannelChange(false);


	local count = ChatSystem:GetInstance():GetTotalUnreadCount();

	if self.needOpenChannelType == ChatChannel.Friend then
		self.toggleGroup:SelectedIndex(ChannelUIIndex.Friend);
		self:SwitchChannel(ChatChannel.Friend);

        if self.focusFriendInfo then
			self:CreatePrivateChat(self.focusFriendInfo);
        end

	elseif self.needOpenChannelType == ChatChannel.Team then
		self.toggleGroup:SelectedIndex(ChannelUIIndex.Team);
		self:SwitchChannel(ChatChannel.Team);

		if self.firstCellInfo then
			self:ShowChatByFriendInfo(self.firstCellInfo);
		else
			self:ShowChatByFriendInfo();
		end

	elseif count > 0 then
		self.toggleGroup:SelectedIndex(ChannelUIIndex.Friend);
		self:OnToggleChanged(ChannelUIIndex.Friend);
	else
		self.toggleGroup:SelectedIndex(ChannelUIIndex.World);

		self:SwitchChannel(ChatChannel.World);
	end

	self:RefreshRecentChatTab();
	self:RefreshOnlineOfflineFriendCount();
end

function M:OnFinishOpen()
	self.isMoving = false;
end

function M:OnAddListener()
	self.emojiBtn:AddEventListener(ButtonEvent.CLICK, self.OnClickEmoji, self);
	self.sendBtn:AddEventListener(ButtonEvent.CLICK, self.OnClickSend, self);
	self.closeBtn:AddEventListener(ButtonEvent.CLICK, self.OnClickClose, self);
	self.channelBtn:AddEventListener(ButtonEvent.CLICK, self.OnClickChannel, self);
	self.recentBtn:AddEventListener(ButtonEvent.CLICK, self.OnClickRecentDropDown, self);
	self.friendBtn:AddEventListener(ButtonEvent.CLICK, self.OnClickFriendDropDown, self);
	self.addFriendBtn:AddEventListener(ButtonEvent.CLICK, self.OnClickAddFriend, self);
	self.interactBtn:AddEventListener(ButtonEvent.CLICK, self.OnClickInteract, self);
	BroadcastEventManager:GetInstance():AddListener("ChatDialog_ClearAllFriendList", self.ClearAllFriend, self, self);
	BroadcastEventManager:GetInstance():AddListener("ChatDialog_ResetAllDataSource", self.ResetAllDataSource, self, self);
	BroadcastEventManager:GetInstance():AddListener("ChatDialog_ResetChannel", self.ResetChannel, self);
	BroadcastEventManager:GetInstance():AddListener("ChatDialog_ReceiveFriendUpdate", self.OnReceiveFriend, self, self);
	BroadcastEventManager:GetInstance():AddListener("ChatDialog_DeleteFriend", self.OnDeleteFriend, self, self);
	BroadcastEventManager:GetInstance():AddListener("ChatDialog_DeleteRecent", self.OnDeleteRecent, self);
	BroadcastEventManager:GetInstance():AddListener("ChatDialog_ReloadRecentChatTab", self.ReloadRecentChatTab, self, self);
	BroadcastEventManager:GetInstance():AddListener("ChatDialog_ReloadFriendList", self.ReloadFriendList, self);
	BroadcastEventManager:GetInstance():AddListener("ChatDialog_RefreshRecentChatTab", self.RefreshRecentChatTab, self, self);
end

function M:OnRemoveListener()
	self.emojiBtn:RemoveEventListener(ButtonEvent.CLICK, self.OnClickEmoji);
	self.sendBtn:RemoveEventListener(ButtonEvent.CLICK, self.OnClickSend);
	self.closeBtn:RemoveEventListener(ButtonEvent.CLICK, self.OnClickClose);
	self.channelBtn:RemoveEventListener(ButtonEvent.CLICK, self.OnClickChannel);
	self.recentBtn:RemoveEventListener(ButtonEvent.CLICK, self.OnClickRecentDropDown);
	self.friendBtn:RemoveEventListener(ButtonEvent.CLICK, self.OnClickFriendDropDown);
	self.addFriendBtn:RemoveEventListener(ButtonEvent.CLICK, self.OnClickAddFriend);
	self.interactBtn:RemoveEventListener(ButtonEvent.CLICK, self.OnClickInteract);
	BroadcastEventManager:GetInstance():RemoveListener("ChatDialog_ClearAllFriendList", self.ClearAllFriend, self, self);
	BroadcastEventManager:GetInstance():RemoveListener("ChatDialog_ResetAllDataSource", self.ResetAllDataSource, self, self);
	BroadcastEventManager:GetInstance():RemoveListener("ChatDialog_ResetChannelType", self.ResetChannel, self);
	BroadcastEventManager:GetInstance():RemoveListener("ChatDialog_ReceiveFriendUpdate", self.OnReceiveFriend, self);
	BroadcastEventManager:GetInstance():RemoveListener("ChatDialog_DeleteFriend", self.OnDeleteFriend, self);
	BroadcastEventManager:GetInstance():RemoveListener("ChatDialog_DeleteRecent", self.OnDeleteRecent, self);
	BroadcastEventManager:GetInstance():RemoveListener("ChatDialog_ReloadRecentChatTab", self.ReloadRecentChatTab, self, self);
	BroadcastEventManager:GetInstance():RemoveListener("ChatDialog_ReloadFriendList", self.ReloadFriendList, self);
	BroadcastEventManager:GetInstance():RemoveListener("ChatDialog_RefreshRecentChatTab", self.RefreshRecentChatTab, self);
end


function M:OnAddDynamicListener()
	BroadcastEventManager:GetInstance():AddListener("ChatDialog_AddEmoji", self.OnAddEmoji, self, self);
	BroadcastEventManager:GetInstance():AddListener("ChatDialog_SendEmoji", self.OnSendEmoji, self, self);
	BroadcastEventManager:GetInstance():AddListener("ChatDialog_AddMessage", self.OnAddMessage, self, self);
	BroadcastEventManager:GetInstance():AddListener("ChatDialog_ReveiceMsg", self.ReceiveMsg, self, self);
	BroadcastEventManager:GetInstance():AddListener("ChatDialog_RefreshChannel", self.RefreshChannelId, self, self);
	BroadcastEventManager:GetInstance():AddListener("ChatDialog_CloseSelf", self.CloseSelf, self, self);
	BroadcastEventManager:GetInstance():AddListener("ChatDialog_CloseSelfAndOpenSpace", self.CloseSelfAndOpenSpace, self, self);
	BroadcastEventManager:GetInstance():AddListener("KeyboardClickEvent", self.ChannelOperation, self, self);
	BroadcastEventManager:GetInstance():AddListener("KeyboardCloseEvent", self.OnClickChannel, self, self);
	BroadcastEventManager:GetInstance():AddListener("ChatDialog_EmojiClose", self.OnClickEmoji, self, self);
	BroadcastEventManager:GetInstance():AddListener("CommonFriendInfoChange", self.RefreshFriendInfo, self, self);
	BroadcastEventManager:GetInstance():AddListener("ChatDialog_CreatePrivateChat", self.CreatePrivateChat, self, self);
	BroadcastEventManager:GetInstance():AddListener("ChatDialog_AddNewFriend", self.AddNewFriend, self, self);
	BroadcastEventManager:GetInstance():AddListener("TeamEvent.OwnerTeamChange", self.HandleTeamChange, self);
	BroadcastEventManager:GetInstance():AddListener("ChatDialog_ShowSelection", self.ShowSelection, self);
end

function M:OnRemoveDynamicListener()
	BroadcastEventManager:GetInstance():RemoveListener("ChatDialog_AddEmoji", self.OnAddEmoji, self);
	BroadcastEventManager:GetInstance():RemoveListener("ChatDialog_SendEmoji", self.OnSendEmoji, self, self);
	BroadcastEventManager:GetInstance():RemoveListener("ChatDialog_AddMessage", self.OnAddMessage, self, self);
	BroadcastEventManager:GetInstance():RemoveListener("ChatDialog_ReveiceMsg", self.ReceiveMsg, self);
	BroadcastEventManager:GetInstance():RemoveListener("ChatDialog_RefreshChannel", self.RefreshChannelId, self);
	BroadcastEventManager:GetInstance():RemoveListener("ChatDialog_CloseSelf", self.CloseSelf, self, self);
	BroadcastEventManager:GetInstance():RemoveListener("ChatDialog_CloseSelfAndOpenSpace", self.CloseSelfAndOpenSpace, self, self);
	BroadcastEventManager:GetInstance():RemoveListener("KeyboardClickEvent", self.ChannelOperation, self);
	BroadcastEventManager:GetInstance():RemoveListener("KeyboardCloseEvent", self.OnClickChannel, self);
	BroadcastEventManager:GetInstance():RemoveListener("ChatDialog_EmojiClose", self.OnClickEmoji, self);
	BroadcastEventManager:GetInstance():RemoveListener("CommonFriendInfoChange", self.RefreshFriendInfo, self);
	BroadcastEventManager:GetInstance():RemoveListener("ChatDialog_CreatePrivateChat", self.CreatePrivateChat, self);
	BroadcastEventManager:GetInstance():RemoveListener("ChatDialog_AddNewFriend", self.AddNewFriend, self);
	BroadcastEventManager:GetInstance():RemoveListener("TeamEvent.OwnerTeamChange", self.HandleTeamChange, self);
	BroadcastEventManager:GetInstance():RemoveListener("ChatDialog_ShowSelection", self.ShowSelection, self);
end

function M:ResetChannel()
	self.toggleGroup:SelectedIndex(ChannelUIIndex.World);
	self:OnToggleChanged(ChannelUIIndex.World);
end

function M:OnFriendClick(cell)
	self:ShowChatByFriendInfo(cell.data);
end

function M:ChangeChatInputEnable(enable)
	self.skin.inputField.interactable = enable;
	self.emojiBtn:SetInteractable(enable);
	self.micBtn:SetInteractable(enable);
	self.voiceBtn:SetInteractable(enable);
	self.sendBtn:SetInteractable(enable);
	if enable then
		ImageUtils.DeGray(self.skin.emojiBtnImage);
		ImageUtils.DeGray(self.skin.micBtnImage);
		ImageUtils.DeGray(self.skin.voiceBtnImage);
		ImageUtils.DeGray(self.skin.sendBtnImage);
	else
		ImageUtils.Gray(self.skin.emojiBtnImage);
		ImageUtils.Gray(self.skin.micBtnImage);
		ImageUtils.Gray(self.skin.voiceBtnImage);
		ImageUtils.Gray(self.skin.sendBtnImage);
	end
end

function M:ShowSelection(data)
	if not data then
		self:ClearUnitSelection();
	else
		self:SelectUnit(data);
	end
end

function M:ClearUnitSelection()
	for k, v in pairs(self.friendUnitList) do
		v:SetSelected(false);
	end

	for k, v in pairs(self.recentUnitList) do
		v:SetSelected(false);
	end
end

function M:SelectUnit(data)
	for k, v in pairs(self.friendUnitList) do
		if data.baseinfo.roleid == v:GetData().baseinfo.roleid then
			v:SetSelected(true);
		else
			v:SetSelected(false);
		end
	end

	for k, v in pairs(self.recentUnitList) do
		if data.baseinfo.roleid == v:GetData().baseinfo.roleid then
			v:SetSelected(true);
		else
			v:SetSelected(false);
		end
	end
end

function M:ShowChatByFriendInfo(data)
	if not data then
		self.skin.friendScrollPaoGameObject:SetActive(false);
		self.skin.friendBarGameObject:SetActive(false);
		self.skin.noneGameObject:SetActive(true);
		FriendSystem:GetInstance():SetCurrentSelectedFriendInfo();
		if self.currentChannel == ChannelUIIndex.Friend then
			self:ChangeChatInputEnable(false);
		end

		self:ClearUnitSelection();

		return;
	end

	if self.currentChannel == ChannelUIIndex.Friend then
		self:ChangeChatInputEnable(true);
	end
    FriendSystem:GetInstance():SetCurrentSelectedFriendInfo(data);
	self.currentFriendInfo = data;
	self.skin.friendScrollPaoGameObject:SetActive(true);
	self.skin.friendBarGameObject:SetActive(true);
	self.skin.noneGameObject:SetActive(false);
	ChatSystem:GetInstance():SetCurrentChannelId(data.baseinfo.roleid);
	local chatManager = ChatSystem:GetInstance():GetChatMsgManagerByChannel(ChatChannel.Friend, data.baseinfo.roleid);
	if chatManager then
		self.friendChatScroll:SetDataSource(chatManager);
	end

	local friend = FriendSystem:GetInstance():GetMyFriendByRoleId(data.baseinfo.roleid);
	if friend then
		self.skin.friendNameTxt.text = friend.baseinfo.rolename;
		self.skin.addFriendBtnGameObject:SetActive(false);
	else
		self.skin.friendNameTxt.text = data.baseinfo.rolename;
		self.skin.addFriendBtnGameObject:SetActive(true);
	end
	ChatSystem:GetInstance():ClearUnreadCountByRoleId(data.baseinfo.roleid);

	self:SelectUnit(data);

	BroadcastEventManager:GetInstance():Bingo("ChatFriendCell_RefreshNotice");
	BroadcastEventManager:GetInstance():Bingo("CommonChatModule_RefreshChatNotice");
end

function M:OnAddEmoji(emoji)
	local str = self.skin.inputField.text .. emoji;
	if StringBuilder.utf8len(str) <= 30 then
		self.skin.inputField.text = str;
	end
end

function M:OnSendEmoji(emoji)
	self.isEmojiVisible = false;
	self.emojiModule:Hide();
	self.emojiBtn:SetRaycastTargetEnable(true);
	self:OnClickSend(emoji);
end

function M:OnAddMessage(message)
	local data = {};
	data.msg = message;
	data.channelid = ChatSystem:GetInstance():GetCurrentChannelId();
	data.channeltype = ChatSystem:GetInstance():GetCurrentChannelType();
	ChatSystem:GetInstance():SendChatMessage(data)
	self:OnClickEmoji();
end

function M:OnClickRecentDropDown()
	self.skin.recentScrollGameObject:SetActive(true);
	self.skin.friendScrollGameObject:SetActive(false);
	self.skin.recentArrowTrans.localEulerAngles = Vector3.New(0, 0, -90);
	self.skin.friendArrowTrans.localEulerAngles = Vector3.New(0, 0, 0);
    self.skin.friendDropDown.anchorMax = Vector2.New(0.5, 0);
	self.skin.friendDropDown.anchorMin = Vector2.New(0.5, 0);
	self.skin.friendDropDown.pivot = Vector2.New(0.5, 0);
	TransformOpt.SetAnchoredPos(self.skin.friendDropDown, 0, 0);
	local recentChatList = ChatSystem:GetInstance():GetRecentChatList();
	if TableUtil.TableLength(recentChatList) > 0 then
		self.skin.friendScrollPaoGameObject:SetActive(true);
		self.skin.friendBarGameObject:SetActive(true);
		self.skin.noneGameObject:SetActive(false);
		--self:ChangeChatInputEnable(true);
		self.skin.noneText.text = BeanUtil.GetResStringMsg(10315);
		self:ShowChatByFriendInfo(self.focusFriendInfo);
	else
		self.skin.friendScrollPaoGameObject:SetActive(false);
		self.skin.friendBarGameObject:SetActive(false);
		self.skin.noneGameObject:SetActive(true);
		--self:ChangeChatInputEnable(false);
		self.skin.noneText.text = BeanUtil.GetResStringMsg(10141);
		self:ShowChatByFriendInfo();
	end
end

function M:OnClickFriendDropDown()
	self.skin.friendScrollPaoGameObject:SetActive(false);
	self.skin.friendBarGameObject:SetActive(false);
	self.skin.noneGameObject:SetActive(true);
	self:ChangeChatInputEnable(false);
	self.skin.recentScrollGameObject:SetActive(false);
	self.skin.friendScrollGameObject:SetActive(true);
	self.skin.recentArrowTrans.localEulerAngles = Vector3.New(0, 0, 0);
	self.skin.friendArrowTrans.localEulerAngles = Vector3.New(0, 0, -90);
	self.skin.friendDropDown.anchorMax = Vector2.New(0.5, 1);
	self.skin.friendDropDown.anchorMin = Vector2.New(0.5, 1);
	self.skin.friendDropDown.pivot = Vector2.New(0.5, 1);
	TransformOpt.SetAnchoredPos(self.skin.friendDropDown, 0, -60);

	self:ClearUnitSelection();

	local friendList = FriendSystem:GetInstance():GetFriendListExceptBlackList();
	self:ShowChatByFriendInfo();
	if TableUtil.TableLength(friendList) > 0 then
		self.skin.noneText.text = BeanUtil.GetResStringMsg(10135);
	else
		self.skin.noneText.text = BeanUtil.GetResStringMsg(10141);
	end
end

function M:OnClickAddFriend()
	if not self.currentFriendInfo then
		if info then info(" self.currentFriendInfo not exist!!!!!!"); end
		return;
	end
	local friendInfo = FriendApplyInfo.New();
	friendInfo.baseinfo = FriendBaseInfo.New();
	friendInfo.baseinfo.roleid = self.currentFriendInfo.baseinfo.roleid;
	friendInfo.baseinfo.rolename = self.currentFriendInfo.baseinfo.rolename;
	FriendSystem:GetInstance():SetCurrentSelectedFriendInfo(friendInfo);
	DialogManager:GetInstance():OpenDialog(DialogType.AddFriend);
end

function M:OnClickMoreInfo(index)
	if index == 1 then
		local playerData = PlayerData.New();
		playerData:Init(self.currentFriendInfo.baseinfo.roleid);
		playerData:UpdateAvatarId(self.currentFriendInfo.baseinfo.avatarid);
		playerData:UpdateName(self.currentFriendInfo.baseinfo.rolename);
		playerData:UpdateLevel(self.currentFriendInfo.baseinfo.level);
		self:OnClickInteract();
		self:CloseSelfAndOpenSpace(playerData);
	elseif index == 2 then
		local result = TeamSystem:GetInstance():RequestInviteJoinTeam(self.currentFriendInfo.baseinfo.roleid)
		self:OnClickInteract();
	elseif index == 3 then
		if FriendSystem:GetInstance():IsInBlackList(self.currentFriendInfo.baseinfo.roleid) then
			FriendSystem:GetInstance():ClearBlackListByRoleId(self.currentFriendInfo.baseinfo.roleid);
			FriendSystem:GetInstance():SendRemoveBlackList(self.currentFriendInfo.baseinfo.roleid);
		else
			if FriendSystem:GetInstance():GetBlackListFriendByRoleId(self.currentFriendInfo.baseinfo.roleid) then
				CommMsgMgr:GetInstance():Show(180085, {content = self.currentFriendInfo.baseinfo.rolename});
			else
				local confirmToBlackList = function(self)
					FriendSystem:GetInstance():SendAddBlackList(self.currentFriendInfo.baseinfo);
					self:ShowChatByFriendInfo();
				end
				CommMsgMgr:GetInstance():Show(180187, {content = self.currentFriendInfo.baseinfo.rolename}, self, nil, confirmToBlackList);
			end
		end
		self:OnClickInteract();
	end
end

function M:OnClickInteract()
	if self.isMoreInfoVisible then
		self.skin.interactGameObject:SetActive(false);
		self.isMoreInfoVisible = false;
		UIManager:GetInstance():UnregisterComponentClick(self.moduleCommonCell, self.OnClickInteract, self);
	else

		if not FriendSystem:GetInstance():IsInBlackList(self.currentFriendInfo.baseinfo.roleid) then
			self.moduleCommonCell:SetOptions({BeanUtil.GetResStringMsg(10132), BeanUtil.GetResStringMsg(10133), BeanUtil.GetResStringMsg(10134)}, self.OnClickMoreInfo, self, 1);
		else
			self.moduleCommonCell:SetOptions({BeanUtil.GetResStringMsg(10132), BeanUtil.GetResStringMsg(10133), BeanUtil.GetResStringMsg(10328)}, self.OnClickMoreInfo, self, 1);
		end
		self.skin.interactGameObject:SetActive(true);
		self.isMoreInfoVisible = true;
		UIManager:GetInstance():RegisterComponentClick(self.moduleCommonCell, self.OnClickInteract, self);
	end
end

function M:ReceiveMsg(data)
    --local currentChannelType = ChatSystem:GetInstance():GetCurrentChannelType();
    --self.protoChatManagers[currentChannelType]:Receive(data)
end

function M:RefreshChannelId()
	if ChatSystem:GetInstance():GetCurrentChannelType() == ChatChannel.World then
		self.skin.channelInput.text = ChatSystem:GetInstance():GetCurrentChannelId();
	end
end

function M:ResetChannelId()
	self.skin.channelInput.text = "";
end

--[[
	private 点击频道按钮回调
]]

function M:ChannelOperation(data)
	if not data or not data.operation then
		if info then info("keyboard data or operation missing!!!"); return; end
	end
	local currentChannelText = self.skin.channelInput.text;
	if data.operation == KeyboardOperation.ADD then
		if not data.num then
			if info then info("keyboard num missing!!!"); return; end
		end
		if tonumber(currentChannelText) == 0 then
			currentChannelText = tostring(data.num);
		else
			currentChannelText = currentChannelText..tostring(data.num);
		end
		if tonumber(currentChannelText) > 999 then
			currentChannelText = "999";
		end
		self.skin.channelInput.text = currentChannelText;
	elseif  data.operation == KeyboardOperation.DELETE then
		currentChannelText = string.sub(currentChannelText, 1, -2);
		if StringBuilder.IsNilOrEmpty(currentChannelText) or tonumber(currentChannelText) == 0 then
			currentChannelText = "";
		end
		self.skin.channelInput.text = currentChannelText;
	elseif data.operation == KeyboardOperation.CONFIRM then
		if StringBuilder.IsNilOrEmpty(currentChannelText) or tonumber(currentChannelText) == 0 then
			CommMsgMgr:GetInstance():Show(180054);
			return;
		end
		ChatSystem:GetInstance():SendChangeWorldChannel(tonumber(currentChannelText));
		self:RefreshChannelId();
		self:OnClickChannel();
	else
		if info then info("keyboard operation error!!!"); end
	end
end

function M:OnClickEmoji(forceClose)
	if forceClose then
		self.isEmojiVisible = false;
		self.emojiModule:Hide();
		self.emojiBtn:SetRaycastTargetEnable(true);
		return;
	end
    if not self.isEmojiVisible then
		self.emojiModule:Show();
		self.isEmojiVisible = true;
		self.emojiBtn:SetRaycastTargetEnable(false);
    else
		self.emojiModule:Hide();
		self.isEmojiVisible = false;
		self.emojiBtn:SetRaycastTargetEnable(true);
    end
end

function M:OnClickSend(immediateSendStr)
	if immediateSendStr and not StringBuilder.IsNilOrEmpty(immediateSendStr) then
		local data = ChatBaseData.New();
		data.msg = immediateSendStr;
		data.channelid = ChatSystem:GetInstance():GetCurrentChannelId();
		data.channeltype = ChatSystem:GetInstance():GetCurrentChannelType();
		ChatSystem:GetInstance():SendChatMessage(data);
		return;
	end
	if StringBuilder.IsNilOrEmpty(self.skin.inputField.text) then
		return;
	end
	if StringBuilder.utf8len(self.skin.inputField.text) > 30 then
		local str = StringBuilder.SubStringUTF8(self.skin.inputField.text, 1, 30);
		self.skin.inputField.text = str;
	end

	local data = ChatBaseData.New();
	data.msg = self.skin.inputField.text;
	data.channelid = ChatSystem:GetInstance():GetCurrentChannelId();
	data.channeltype = ChatSystem:GetInstance():GetCurrentChannelType();

	ChatSystem:GetInstance():SendChatMessage(data)
	self.skin.inputField.text = "";
end

function M:OnClickChannel(forceClose)
	if forceClose then
		self.isKeyVisible = false;
		self.skin.keyboardModuleGameObject:SetActive(false);
		self.commonKeyboardFrame:Hide();
		self:RefreshChannelId();
		return;
	end
	if self.isKeyVisible then
		self.isKeyVisible = false;
		self.skin.keyboardModuleGameObject:SetActive(false);
		self.commonKeyboardFrame:Hide();
		self:RefreshChannelId();
	else
		self.isKeyVisible = true;
		self.skin.keyboardModuleGameObject:SetActive(true);
		self.commonKeyboardFrame:Show();
		self:ResetChannelId();
	end
end

function M:OnReceiveFriend()
	local friends = FriendSystem:GetInstance():GetFriendListExceptBlackList();
	if not friends or TableUtil.TableLength(friends) <= 0 then
		return;
	end
	self.friendMyFriendScroll:ClearUnit();
	self.friendUnitList = {};
	for k, v in pairs(friends) do
        if not FriendSystem:GetInstance():IsInBlackList(v.baseinfo.roleid) then
            local unit = ScrollUnit.New(v);
            self.friendMyFriendScroll:AddUnit(unit);
            table.insert(self.friendUnitList, unit);
        end
	end
	self:RefreshOnlineOfflineFriendCount()

	local count = ChatSystem:GetInstance():GetTotalUnreadCount();
	if count > 0 then
		self.toggleGroup:SelectedIndex(ChannelUIIndex.Friend);
		self:OnToggleChanged(ChannelUIIndex.Friend);
	end
	TickerManager:GetInstance():AddTicker(self);
end

function M:AddNewFriend(data)
    for k, v in pairs(data) do
        if not FriendSystem:GetInstance():IsInBlackList(v.baseinfo.roleid) then
            local unit = ScrollUnit.New(v);
            self.friendMyFriendScroll:AddUnit(unit);
            table.insert(self.friendUnitList, unit);
        end
    end
	self:RefreshOnlineOfflineFriendCount();
end

function M:OnDeleteFriend(roleid)

	local pos = -1
	local unitListPos = -1;
	for k, v in pairs(self.friendUnitList) do
		if v:GetData() and v:GetData().baseinfo.roleid == roleid then
			pos = v:GetIndex()
			unitListPos = k;
		end
	end

	if pos == -1 then
		if info then info("此人并不是好友 roleid = ".. roleid); end
		return
	end

	if unitListPos == -1 then
		if info then info("此人并不是好友 roleid = ".. roleid); end
		return
	end

	self.friendMyFriendScroll:RemoveUnit(pos);
	table.remove(self.friendUnitList, unitListPos);
	self:RefreshOnlineOfflineFriendCount();
end

function M:OnDeleteRecent(roleid)
	local ready = ChatSystem:GetInstance():GetIfRecentChatDataReady();
	if not ready then
		ChatSystem:GetInstance():LoadLocalRecentChatList();
		ChatSystem:GetInstance():LoadOfflineMsgCache();
		return;
	end

	local pos = -1
	local unitListPos = -1;
	for k, v in pairs(self.recentUnitList) do
		if v:GetData() and v:GetData().baseinfo.roleid == roleid then
			pos = v:GetIndex()
			unitListPos = k;
		end
	end

	if pos == -1 then
		--if info then info("没有跟此人的私聊unit roleid = ".. roleid); end
		return
	end

	if unitListPos == -1 then
		--if info then info("没有跟此人的私聊unit roleid = ".. roleid); end
		return
	end

	self.friendRecentScroll:RemoveUnit(pos);
	table.remove(self.recentUnitList, unitListPos);
	self:RefreshOnlineOfflineFriendCount();
end

function M:ReloadFriendList()
	local friends = FriendSystem:GetInstance():GetFriendListExceptBlackList();
	self.friendMyFriendScroll:ClearUnit();
	for k, v in pairs(friends) do
		if not FriendSystem:GetInstance():IsInBlackList(v.baseinfo.roleid) then
			local unit = ScrollUnit.New(v);
			self.friendMyFriendScroll:AddUnit(unit);
			table.insert(self.friendUnitList, unit);
		end
	end
	self:RefreshOnlineOfflineFriendCount()
end

function M:HandleTeamChange(hasTeam)
	self.hasTeam = hasTeam;
	if self.hasTeam == true then
		self.skin.channelTabGO[2]:SetActive(true);
	else
		self.skin.channelTabGO[2]:SetActive(false);
	end
	--self.toggleGroup:SetToggleBtnEnableByIndex(ChannelUIIndex.Team, hasTeam);
end

function M:HandleAfterBattleChannelChange(hasAfterBattle)
	self.hasAfterBattle = hasAfterBattle;
	--self.toggleGroup:SetToggleBtnEnableByIndex(ChannelUIIndex.AfterBattle, hasAfterBattle);
end

function M:RefreshOnlineOfflineFriendCount()
	local friends = FriendSystem:GetInstance():GetFriendListExceptBlackList();

	local onlineFriendCount = 0;
	local friendCount = TableUtil.TableLength(friends);
	for k, v in pairs(friends) do
		if FriendSystem:GetInstance():IsInBlackList(v.baseinfo.roleid) then
			friendCount = friendCount - 1;
		else
			if v.state ~= FriendState.OFFLINE then
				onlineFriendCount = onlineFriendCount + 1;
			end
		end
	end
	self.skin.friendNumTxt.text = BeanUtil.GetResStringMsg(10131, {tostring(onlineFriendCount), tostring(friendCount)});

	friends = ChatSystem:GetInstance():GetRecentChatList();
	if not friends or TableUtil.TableLength(friends) <= 0 then
		self.skin.recentNumTxt.text = BeanUtil.GetResStringMsg(10131, {"0", "0"});
		return;
	end

	local count = TableUtil.TableLength(friends);
	friendCount = TableUtil.TableLength(friends);
	onlineFriendCount = 0;
	for i = count, 1, -1 do
		if FriendSystem:GetInstance():GetMyFriendByRoleId(friends[i].baseinfo.roleid) then
			friends[i] = FriendSystem:GetInstance():GetMyFriendByRoleId(friends[i].baseinfo.roleid)
		end
		if i == count then
			self.firstCellInfo = friends[i];
		end

		if friends[i].state ~= FriendState.OFFLINE then
			onlineFriendCount = onlineFriendCount + 1;
		end
	end

	if count > TableUtil.TableLength(self.recentUnitList) then
		--local recentUnitListCount = TableUtil.TableLength(self.recentUnitList);
		--if info then info(" ============= 数据异常， 重新加载 RecentChatTab ============= recentUnitListCount = " .. recentUnitListCount .. " realCount = " .. count); end
		self:ReloadRecentChatTab();
	end

	self.skin.recentNumTxt.text = BeanUtil.GetResStringMsg(10131, {tostring(onlineFriendCount), tostring(friendCount)});
end

function M:OnToggleChanged(index)
	local result = self:CheckSwitchable(index);
	if result == false then
		self.toggleGroup:SelectedIndex(self.lastChannel);
		self:OnToggleChanged(self.lastChannel);
		return;
	end
	self.currentChannel = index;
    if index == ChannelUIIndex.World then
        self:SwitchChannel(ChatChannel.World);
		self:ChangeChatInputEnable(true);
    elseif index == ChannelUIIndex.Team then
		self:ChangeChatInputEnable(true);
		self:SwitchChannel(ChatChannel.Team);
    elseif index == ChannelUIIndex.Friend then
		if FriendSystem:GetInstance():GetCurrentSelectedFriendInfo() then
			self:ChangeChatInputEnable(true);
		else
			self:ChangeChatInputEnable(false);
		end
		self:SwitchChannel(ChatChannel.Friend);
		--self:OnClickRecentDropDown();
    elseif index == ChannelUIIndex.AfterBattle then
		self:ChangeChatInputEnable(true);
		self:SwitchChannel(ChatChannel.AfterBattle);
	end
	self.lastChannel = index;
end

function M:CheckSwitchable(index)
	if index == ChannelUIIndex.Team then
		if self.hasTeam == true then
			return true;
		else
			CommMsgMgr:GetInstance():Show(180121);
			return false;
		end
	elseif index == ChannelUIIndex.AfterBattle then
		if self.hasAfterBattle == true then
			return true;
		else
			CommMsgMgr:GetInstance():Show(180122);
			return false;
		end
	end
	return true;
end

function M:ConvertToUIChannel(channel)
	if channel == ChatChannel.World then
		return ChannelUIIndex.World;
	elseif channel == ChatChannel.Team then
		return ChannelUIIndex.Team;
	elseif channel == ChatChannel.Friend then
		return ChannelUIIndex.Friend;
	elseif channel == ChatChannel.AfterBattle then
		return ChannelUIIndex.AfterBattle;
	else
		return 0
	end
end

function M:SwitchChannel(channel)
	--local currentChannelType = ChatSystem:GetInstance():GetCurrentChannelType();
	--if currentChannelType == channel then
	--	return;
	--end
	for i = 1, 4 do
		local flag = i == self:ConvertToUIChannel(channel);
		self.skin.channelCheckMark[i]:SetActive(flag);
	end

	ChatSystem:GetInstance():SwitchChannel(channel);
	local uiChannel = self:ConvertToUIChannel(channel);
	self.currentScroll = self.scrollList[uiChannel];
	for i = 1, ChatCommon.ChannelCount do
		if self.channelGameObjectList[i] then
			local visible = i == uiChannel;
			self.channelGameObjectList[i]:SetActive(visible);
		end
	end
	if channel == ChatChannel.World then
		self.skin.channelInputGameObject:SetActive(true);
	elseif channel == ChatChannel.Friend then
		self.skin.channelInputGameObject:SetActive(false);
	end
end

function M:ReloadRecentChatTab()
	ChatSystem:GetInstance():LoadLocalRecentChatList();
	local friends = ChatSystem:GetInstance():GetRecentChatList();
	if not friends or TableUtil.TableLength(friends) <= 0 then

        self.friendRecentScroll:ClearUnit();
		self.skin.recentNumTxt.text = BeanUtil.GetResStringMsg(10131, {tostring(0), tostring(0)});
		return;
	end
	--先全清，按照数据顺序来
	self.friendRecentScroll:ClearUnit();
	local count = TableUtil.TableLength(friends);
	self.recentUnitList = {};
	for i = count, 1, -1 do
		if FriendSystem:GetInstance():GetMyFriendByRoleId(friends[i].baseinfo.roleid) then
			friends[i] = FriendSystem:GetInstance():GetMyFriendByRoleId(friends[i].baseinfo.roleid)
		end
		local unit = ScrollUnit.New(friends[i]);
		self.friendRecentScroll:AddUnit(unit);
		table.insert(self.recentUnitList, unit);
		if tonumber(i) == tonumber(count) then
			self.firstCellInfo = friends[i];
		end
	end
	self:RefreshOnlineOfflineFriendCount();
end

function M:RefreshRecentChatTab()
    local friends = ChatSystem:GetInstance():GetRecentChatList();
    if not friends or TableUtil.TableLength(friends) <= 0 then
	    return;
    end
	local count = TableUtil.TableLength(friends);
	for i = count, 1, -1 do
		self.friendRecentScroll:ModifyUnit(count - i + 1, friends[i]);
		if i == count then
			self.firstCellInfo = friends[i];
		end
	end
end

function M:RefreshFriendInfo()
	local friendList = FriendSystem:GetInstance():GetFriendListExceptBlackList();
	for index, data in pairs(friendList) do
		local unit = self.friendMyFriendScroll:GetUnitByIndex(index);
		if not unit then
			self:OnReceiveFriend();
			return;
		else
			self.friendMyFriendScroll:ModifyUnit(index, data);
		end
	end
	self:RefreshRecentChatTab();
	self:RefreshOnlineOfflineFriendCount();

end

function M:CreatePrivateChat(data)
	if not data then
		return
	end
	self.toggleGroup:SelectedIndex(ChannelUIIndex.Friend);
	self:OnToggleChanged(ChannelUIIndex.Friend);
	self:ShowChatByFriendInfo(data);
end

function M:ResetAllDataSource()
	-- 默认当前频道
	if not ChatSystem:HasInstance() then
		self.needResetAllDataSource = true;
		return;
	end
	local chatManager = ChatSystem:GetInstance():GetChatMsgManagerByChannel(ChatChannel.World);
	if chatManager then
		self.worldChatScroll:SetDataSource(chatManager);
	end
	chatManager = ChatSystem:GetInstance():GetChatMsgManagerByChannel(ChatChannel.Team);
	if chatManager then
		self.teamChatScroll:SetDataSource(chatManager);
	end
	chatManager = ChatSystem:GetInstance():GetChatMsgManagerByChannel(ChatChannel.AfterBattle);
	if chatManager then
		self.afterBattleChatScroll:SetDataSource(chatManager);
	end
end

function M:Tick(delta)
	if self.refreshTime > 600 then
		self.refreshTime = 0;
		ChatSystem:GetInstance():SendRecentChaters();
	end
	self.refreshTime = self.refreshTime + delta;
end

function M:Refresh()
end

function M:OnHide()
	--FriendSystem:GetInstance():SetCurrentSelectedFriendInfo();
	--self.toggleGroup:SelectedIndex(ChannelUIIndex.World);
	--self:OnToggleChanged(ChannelUIIndex.World);
	TickerManager:GetInstance():RemoveTicker(self);
	UIManager:GetInstance():UnregisterComponentClick(self);
	UIManager:GetInstance():UnregisterComponentClick(self.moduleCommonCell, self.OnClickInteract, self);
end

function M:OnDestroy()
end

function M:OnClear()
	self.isEmojiVisible = nil;
	self.isKeyVisible = nil;
	self.isMoving = nil;
	self.isMoreInfoVisible = nil;
	self.firstCellInfo = nil;
	self.currentFriendInfo = nil;
	self.isPrivateChat = nil;
	self.skin.keyboardModuleGameObject:SetActive(false);
	self.commonKeyboardFrame:Destroy();
	self.commonKeyboardFrame = nil;
	self.refreshTime = 0;
	self.cachedPlayerData = nil;
	self.friendUnitList = {};
	self.recentUnitList = {};
	self.needOpenChannelType = 0;
	self.focusFriendInfo = nil;
	if self.worldChatScroll then
		self.worldChatScroll:Destroy();
	end
	self.worldChatScroll = nil;

	if self.teamChatScroll then
		self.teamChatScroll:Destroy();
	end
	self.teamChatScroll = nil;

	if self.afterBattleChatScroll then
		self.afterBattleChatScroll:Destroy();
	end
	self.afterBattleChatScroll = nil;

	if self.friendChatScroll then
		self.friendChatScroll:Destroy();
	end
	self.friendChatScroll = nil;

	TableUtil.Clear(self.scrollList);
	self.scrollList = {};

end

function M:CloseSelfAndOpenSpace(playerData)
	self.cachedPlayerData = playerData;
	self:CloseSelf();
end

function M:CloseSelf()
	self:OnClickClose();
end

function M:OnClickClose()
	if not self.isMoving then
		self.isMoving = true;
		UITweenManager:GetInstance():StartTween(0, 0.3, self:GetTransform(), UITweenType.XY, Vector2.New(0, 0), Vector3.New(-1000, 0, 0), "inOutCubic", nil, self.CloseDialog, self);
		UIManager:GetInstance():UnregisterComponentClick(self);
	end
end

function M:CloseDialog()
	self.isMoving = false
	ChatSystem:GetInstance():CloseChatDialog();

	if self.cachedPlayerData then
		RoleSpaceManager:GetInstance():OpenDialogFromChat(self.cachedPlayerData:GetKey())
		self.cachedPlayerData = nil;
	end
end

function M:ClearAllFriend()
	self.friendRecentScroll:ClearUnit();
	self.friendMyFriendScroll:ClearUnit();
end

return M;