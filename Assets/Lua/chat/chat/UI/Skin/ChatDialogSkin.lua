--[[
	ChatDialog 皮肤
]]
local ChatDialogSkin = Class("ChatDialogSkin", UISkin);
local M = ChatDialogSkin;

function M:InitTrans(transform)
end

function M:OnCreate(transform)
	UISkin.OnCreate(self, transform);
	self.worldGameObject = self:GetGameObjectByPath("g_var/g_world");
	self.worldTipTrans = self:GetChildByPath("g_var/g_world/ebtn_notice");
	self.worldScrollTrans = self:GetChildByPath("g_var/g_world/Scroll_Module_Chat_Paopao/Group_ScrollRect");
	self.teamGameObject = self:GetGameObjectByPath("g_var/g_team");
	self.teamTipTrans = self:GetChildByPath("g_var/g_team/ebtn_notice");
	self.teamScrollTrans = self:GetChildByPath("g_var/g_team/Scroll_Module_Chat_Paopao/Group_ScrollRect");
	self.afterBattleGameObject = self:GetGameObjectByPath("g_var/g_afterbattle");
	self.afterBattleTipTrans = self:GetChildByPath("g_var/g_afterbattle/ebtn_notice");
	self.afterBattleScrollTrans = self:GetChildByPath("g_var/g_afterbattle/Scroll_Module_Chat_Paopao/Group_ScrollRect");

	self.keyboardModule = self:GetChildByPath("g_var/g_world/ept_Module_Comm_Keyboard")
	self.keyboardModuleGameObject = self:GetGameObjectByPath("g_var/g_world/ept_Module_Comm_Keyboard");
	self.inputField = self:GetComponentByPath("g_var/g_input/InputField", "TMP_InputField");
	self.channelInputGameObject = self:GetGameObjectByPath("g_var/g_world/InputField");
	self.channelInput = self:GetComponentByPath("g_var/g_world/InputField/et_num", "TextMeshProUGUI");
	self.toggleGroup = self:GetChildByPath("g_var/g_tab");
	self.emojiTrans = self:GetChildByPath("g_var/ept_Module_CF_Input");

	self.friendGameObject = self:GetGameObjectByPath("g_var/g_friend");
    self.friendBarGameObject = self:GetGameObjectByPath("g_var/g_friend/g_friend");
    self.friendNameTxt = self:GetComponentByPath("g_var/g_friend/g_friend/et_name", "TextMeshProUGUI");
    self.addFriendBtnGameObject = self:GetGameObjectByPath("g_var/g_friend/g_friend/ebtn_add");
	self.addFriendBtnTrans = self:GetChildByPath("g_var/g_friend/g_friend/ebtn_add");
	self.interactBtnTrans = self:GetChildByPath("g_var/g_friend/g_friend/ebtn_list");
	self.interactTrans = self:GetChildByPath("g_var/g_friend/Module_Comm_Btn");
	self.interactGameObject = self:GetGameObjectByPath("g_var/g_friend/Module_Comm_Btn");
	self.friendScrollTrans = self:GetChildByPath("g_var/g_friend/Scroll_Module_Chat_Paopao/Group_ScrollRect");
	self.friendScrollPaoGameObject = self:GetGameObjectByPath("g_var/g_friend/Scroll_Module_Chat_Paopao");
	self.friendTipTrans = self:GetChildByPath("g_var/g_friend/ebtn_notice");
	self.friendVoiceGameObject = self:GetGameObjectByPath("g_var/g_friend/g_voice");

	self.friendRecentTrans = self:GetChildByPath("g_var/g_friend/g_list/g_scroll/Scroll_Module_CF_Friend_List_01/Group_ScrollRect");
	self.friendMyFriendTrans = self:GetChildByPath("g_var/g_friend/g_list/g_scroll/Scroll_Module_CF_Friend_List_02/Group_ScrollRect");
	self.recentScrollGameObject = self:GetGameObjectByPath("g_var/g_friend/g_list/g_scroll/Scroll_Module_CF_Friend_List_01");
	self.friendScrollGameObject = self:GetGameObjectByPath("g_var/g_friend/g_list/g_scroll/Scroll_Module_CF_Friend_List_02");
	self.recentChatScroll = self:GetChildByPath("g_var/g_friend/g_list/g_scroll");

	self.recentDropDownBtn = self:GetChildByPath("g_var/g_friend/g_list/g_scroll/Dropdown_01/ebtn_empty");
	self.friendDropDownBtn = self:GetChildByPath("g_var/g_friend/g_list/g_scroll/Dropdown_02/ebtn_empty");

	self.recentDropDown = self:GetChildByPath("g_var/g_friend/g_list/g_scroll/Dropdown_01");
	self.friendDropDown = self:GetChildByPath("g_var/g_friend/g_list/g_scroll/Dropdown_02");

	self.recentArrowTrans = self:GetChildByPath("g_var/g_friend/g_list/g_scroll/Dropdown_01/ebtn_empty/ep_arrow");
	self.recentNumTxt = self:GetComponentByPath("g_var/g_friend/g_list/g_scroll/Dropdown_01/ebtn_empty/et_num", "TextMeshProUGUI");
    self.recentTxt = self:GetComponentByPath("g_var/g_friend/g_list/g_scroll/Dropdown_01/ebtn_empty/et_text", "TextMeshProUGUI");

	self.friendArrowTrans = self:GetChildByPath("g_var/g_friend/g_list/g_scroll/Dropdown_02/ebtn_empty/ep_arrow");
	self.friendNumTxt = self:GetComponentByPath("g_var/g_friend/g_list/g_scroll/Dropdown_02/ebtn_empty/et_num", "TextMeshProUGUI");
    self.friendTxt = self:GetComponentByPath("g_var/g_friend/g_list/g_scroll/Dropdown_02/ebtn_empty/et_text", "TextMeshProUGUI");

	self.noneText = self:GetComponentByPath("g_var/g_friend/Group_None/g_var/et_text", "TextMeshProUGUI");
	self.noneGameObject = self:GetGameObjectByPath("g_var/g_friend/Group_None");

	self.friendListNode = self:GetChildByPath("g_var/g_friend/g_list");
	self.friendListRect = self:GetComponentByPath("g_var/g_friend/g_list", "RectTransform");

	self.emojiBtnImage = self:GetComponentByPath("g_var/g_input/ebtn_emjoy", "Image");
	self.voiceBtnImage = self:GetComponentByPath("g_var/g_input/ebtn_voice", "Image");
	self.micBtnImage = self:GetComponentByPath("g_var/g_input/ebtn_mic", "Image");
	self.sendBtnImage =  self:GetComponentByPath("g_var/g_input/Group_Btn_L_a/ebtn", "Image");

	self.channelCheckMark = {};
	self.channelTabGO = {};
	self.channelTxt = {};
	for i = 1, 4 do
		self.channelCheckMark[i] = self:GetGameObjectByPath("g_var/g_tab/ebtn_0" .. tostring(i).."/Background/Checkmark");
		self.channelTabGO[i] = self:GetGameObjectByPath("g_var/g_tab/ebtn_0" .. tostring(i));
		self.channelTxt[i] = self:GetComponentByPath("g_var/g_tab/ebtn_0" .. tostring(i).."/et_text", "TextMeshProUGUI");
	end
end

return M;