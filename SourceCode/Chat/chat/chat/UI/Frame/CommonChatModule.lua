--[[
		通用聊天框
		创建范例：
			local commonChatModule = CommonChatModule.New();
			self.chatModule = UIComponent.New(self.skin.chatModule);
			self.chatModule:AddChild(commonChatModule);

		默认情况下左边的是好友按钮点击打开好友面板，右边的是迷你聊天框点击打开聊天框
		如果需要自定义回调的话调用SetCallBack
		例：
			local chatModuleParams = {}
			chatModuleParams.btn1Callback = self.fun1;
			chatModuleParams.btn2Callback = self.fun2;
			chatModuleParams.btn1Params = self;
			chatModuleParams.btn2Params = self;
			commonChatModule:SetCallBack(chatModuleParams);

--]]
local TeamQuickChatFrame = require "UI.Communication.Frame.TeamQuickChatFrame"
local ChatCommon = require "UI.Communication.Common.ChatCommon";
local ChatBaseData = require "UI.Communication.Data.ChatBaseData";
local MainUIChatMsgCell = require "UI.MainUI.Frame.MainUIChatMsgCell";
local ChatScrollComponent = require "UI.Component.ScrollView.ChatScrollComponent";

local CommonChatModule = Class("CommonChatModule", UIComponent);
local M = CommonChatModule;

function M:GetLayoutPath()
	return "UI/Prefab/Module/Module_Comm_Chat";
end

function M:OnOpen()
end

function M:OnClose()
end

function M:OnInit()
end

function M:OnParse(params)
	if params then
		self.params = params;

	end
end

function M:SetCallBack(params)
	self.btn1Callback = params.btn1Callback;
	self.btn2Callback = params.btn2Callback;
	self.btn1Params = params.btn1Params;
	self.btn2Params = params.btn2Params;
end

function M:OnCreate()
	if self:TryGetChildByPath("g_chat/ebtn_friend") then
		self.btnFriend = UIButton.New(self:GetChildByPath("g_chat/ebtn_friend"));
		self.btnFriendGO = self:GetGameObjectByPath("g_chat/ebtn_friend");
	end

	if self:TryGetChildByPath("g_chat/ebtn_chat") then
		self.btnChat02 = UIButton.New(self:GetChildByPath("g_chat/ebtn_chat"));
	end

	if self:TryGetComponentByPath("g_chat/p_bg", "Button") then
		self.chatBtn = UIButton.New(self:GetChildByPath("g_chat/p_bg"));
	end

	if self:TryGetChildByPath("g_chat/ebtn_quick") then
		self.btnQuick = UIButton.New(self:GetChildByPath("g_chat/ebtn_quick"));
		self.btnQuickGO = self:GetGameObjectByPath("g_chat/ebtn_quick");
	end

    if self:TryGetChildByPath("g_input/ebtn_send") then
        self.btnSend = UIButton.New(self:GetChildByPath("g_input/ebtn_send"));
    end

    if self:TryGetChildByPath("g_input/ebtn_close") then
        self.btnClose = UIButton.New(self:GetChildByPath("g_input/ebtn_close"));
    end

	if self:TryGetComponentByPath("g_input", "Animator") then
		self.inputAnimRoot = self:TryGetComponentByPath("g_input", "Animator");
	end

	if self:TryGetComponentByPath("g_input/InputField", "TMP_InputField") then
		self.inputField = self:TryGetComponentByPath("g_input/InputField", "TMP_InputField");
	end

	if self:TryGetChildByPath("g_chat/ept_Module_Chat_Quick") then
		self.teamQuickChatFrame = TeamQuickChatFrame.New();
		self.quickChatFrame = UIComponent.New(self:TryGetChildByPath("g_chat/ept_Module_Chat_Quick") );
		self.quickChatFrame:AddChild(self.teamQuickChatFrame);
		self.teamQuickChatFrame:Hide();
		if self.params and self.params.quickMessageType then
			self.teamQuickChatFrame:SetQuickMessageType(self.params.quickMessageType);
		end
	end

	self.chatScrollTrans = self:TryGetChildByPath("g_chat/p_bg/Scroll_Module_Chat_Rect/Group_ScrollRect");
	if self.chatScrollTrans then
		self.chatNoticeGameObject = self:TryGetGameObjectByPath("g_chat/p_bg/ebtn_chat");
		if self.chatNoticeGameObject then
			self.chatNoticeBtn = UIButton.New(self:GetChildByPath("g_chat/p_bg/ebtn_chat"));
		end
		self.chatNoticeNum = self:TryGetComponentByPath("g_chat/p_bg/ebtn_chat/et_num", "TextMeshProUGUI");
		local params = {};
		params.cell = MainUIChatMsgCell;
		params.size = 4; --界面上只保留四条
		params.lineSpace = 0;
		if self.params and self.params.size then
			params.size = self.params.size
		end
		self.chatScroll = ChatScrollComponent.New(self.chatScrollTrans, params);
		if self.params and self.params.channel then
			self.chatScroll:SetDataSource(ChatSystem:GetInstance():GetChatMsgManagerByChannel(self.params.channel));
		else
			self.chatScroll:SetDataSource(ChatSystem:GetInstance():GetChatMsgManagerByChannel(ChatChannel.AllExceptNotice));
		end

		if self.params and self.params.canScroll then
			self.chatScroll:SetCanScroll(true);
			self.chatScroll:SetForceLock(true);
		else
			self.chatScroll:SetCanScroll(false);
		end

	    self.isInputFieldShow = false;
	end
	--self.isQuickChatShow =

end

function M:OnStart()
end

function M:OnShow()
	self:RefreshRedSpot();
	self:RefreshChatNotice();
end

function M:OnHide()
end

function M:OnUpdate()
end

function M:OnClear()
end



function M:OnDestroy()
    if self.chatScroll then
        self.chatScroll:Destroy();
        self.chatScroll = nil;
    end
end

function M:OnAddListener()
	if self.chatBtn then
		self.chatBtn:AddEventListener(ButtonEvent.CLICK, self.OnClickButtonChat, self);
	end

	if self.btnChat02 then
		self.btnChat02:AddEventListener(ButtonEvent.CLICK, self.OnClickButtonChat, self);
	end

	if self.chatNoticeBtn then
		self.chatNoticeBtn:AddEventListener(ButtonEvent.CLICK, self.OnClickButtonChat, self);
	end

	if self.btnFriend then
		self.btnFriend:AddEventListener(ButtonEvent.CLICK, self.OnClickButtonFriend, self);
	end

	if self.btnQuick then
		self.btnQuick:AddEventListener(ButtonEvent.CLICK, self.OnClickButtonQuick, self);
	end

	if self.btnSend then
		self.btnSend:AddEventListener(ButtonEvent.CLICK, self.OnClickButtonSend, self);
	end

	if self.btnClose then
		self.btnClose:AddEventListener(ButtonEvent.CLICK, self.OnClickButtonClose, self);
	end
end

function M:OnRemoveListener()
	if self.chatBtn then
		self.chatBtn:RemoveEventListener(ButtonEvent.CLICK, self.OnClickButtonChat);
	end
	
	if self.btnChat02 then
		self.btnChat02:RemoveEventListener(ButtonEvent.CLICK, self.OnClickButtonChat);
	end

	if self.btnFriend then
		self.btnFriend:RemoveEventListener(ButtonEvent.CLICK, self.OnClickButtonFriend);
	end

	if self.btnQuick then
		self.btnQuick:RemoveEventListener(ButtonEvent.CLICK, self.OnClickButtonQuick, self);
	end

	if self.btnSend then
		self.btnSend:RemoveEventListener(ButtonEvent.CLICK, self.OnClickButtonSend, self);
	end

	if self.btnClose then
		self.btnClose:RemoveEventListener(ButtonEvent.CLICK, self.OnClickButtonClose, self);
	end
end

function M:OnAddDynamicListener()
	BroadcastEventManager:GetInstance():AddListener("RefreshReadPosMsg", self.RefreshRedSpot, self);
	BroadcastEventManager:GetInstance():AddListener("CommonChatModule_RefreshChatNotice", self.RefreshChatNotice, self);
	BroadcastEventManager:GetInstance():AddListener("CommonChatModule_CloseQuickChat", self.CloseQuickChat, self);
	BroadcastEventManager:GetInstance():AddListener("CommonChatModule_AddMessage", self.AddMessage, self);

end

function M:OnRemoveDynamicListener()
	BroadcastEventManager:GetInstance():RemoveListener("RefreshReadPosMsg", self.RefreshRedSpot, self);
	BroadcastEventManager:GetInstance():RemoveListener("CommonChatModule_RefreshChatNotice", self.RefreshChatNotice, self);
	BroadcastEventManager:GetInstance():RemoveListener("CommonChatModule_CloseQuickChat", self.CloseQuickChat, self);
	BroadcastEventManager:GetInstance():RemoveListener("CommonChatModule_AddMessage", self.AddMessage, self);

end

function M:OnClickButtonQuick()
	if self.teamQuickChatFrame then
		self.teamQuickChatFrame:Show();
		self.btnQuick:SetRaycastTargetEnable(false);
	end
end

function M:CloseQuickChat()
	if self.teamQuickChatFrame then
		self.teamQuickChatFrame:Hide();
		self.btnQuick:SetRaycastTargetEnable(true);
	end
end

function M:OnClickButtonSend(immediateSendStr)
	if immediateSendStr and not StringBuilder.IsNilOrEmpty(immediateSendStr) then
		local data = ChatBaseData.New();
		data.msg = immediateSendStr;
		data.channelid = ChatSystem:GetInstance():GetCurrentChannelId();
		data.channeltype = ChatSystem:GetInstance():GetCurrentChannelType();
		ChatSystem:GetInstance():SendChatMessage(data);
		return;
	end
	if StringBuilder.IsNilOrEmpty(self.inputField.text) then
		return;
	end
	if StringBuilder.utf8len(self.inputField.text) > 30 then
		local str = StringBuilder.SubStringUTF8(self.inputField.text, 1, 30);
		self.inputField.text = str;
	end

	local data = ChatBaseData.New();
	data.msg = self.inputField.text;
	data.channelid = ChatSystem:GetInstance():GetCurrentChannelId();
	if self.params and self.params.channel then
		data.channeltype = self.params.channel;
		ChatSystem:GetInstance():SendChatMessage(data)
	end

	self.inputField.text = "";
end

function M:AddMessage(msg)
	local data = ChatBaseData.New();
	data.msg = msg;
	data.channelid = ChatSystem:GetInstance():GetCurrentChannelId();
	if self.params and self.params.channel then
		data.channeltype = self.params.channel;
		ChatSystem:GetInstance():SendChatMessage(data);
	end
end

function M:OnClickButtonClose()
	self:PlayShowInputAnim();
end

function M:OnClose()
end

function M:OnClickButtonChat()
	if self.btn2Callback then
		self.btn2Callback(self.btn2Params);
		return;
	end
	if self.params and self.params.focusChannel then
		ChatSystem:GetInstance():OpenChatDialog(self.params.focusChannel);
	else
		ChatSystem:GetInstance():OpenChatDialog();
	end
end

function M:OnClickButtonFriend()
	if self.btn1Callback then
		self.btn1Callback(self.btn1Params);
		return;
	end
	FriendSystem:GetInstance():RequestFriendPanel();
	DialogManager:GetInstance():OpenDialog(DialogType.Friend);
end

function M:OnOpenDetail()

end

function M:RefreshRedSpot()
	if not self.btnFriend then
		return;
	end
	if CommMsgMgr:GetInstance():IsHasRedPos({SRefreshRedPosMsg.FRIEND_APPLY, SRefreshRedPosMsg.FRIEND_CHATE}) then
		self.btnFriend:ShowNotice(true)
		self.btnFriend:RefreshNoticeNum(CommMsgMgr:GetInstance():GetRedPosNumTxt(SRefreshRedPosMsg.FRIEND_APPLY));
	else
		self.btnFriend:ShowNotice(false)
	end
end

function M:PlayShowInputAnim()
	if not self.inputAnimRoot then
		return;
	end

	if not self.isInputFieldShow then
		self.inputAnimRoot:Play("ui_anim_battleready_chat01", -1, 0);
        self.isInputFieldShow = true;
	else
		self.inputAnimRoot:Play("ui_anim_battleready_chat02", -1, 0);
        self.isInputFieldShow = false;
	end
end


function M:RefreshChatNotice()
	if not self.chatNoticeGameObject or not self.chatNoticeNum then
		return
	end
	local count = ChatSystem:GetInstance():GetTotalUnreadCount();
	if count > 0 then
	    self.chatNoticeGameObject:SetActive(true);
		self.chatNoticeNum.text = count;
	else
	    self.chatNoticeGameObject:SetActive(false);
	end
end

return M;