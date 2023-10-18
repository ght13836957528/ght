local FriendApplyInfo = require "UI.Friend.Data.FriendApplyInfoData"
local FriendBaseInfo = require "UI.Friend.Data.FriendBaseInfoData"
local GroupRoleUnit = require "UI.Common.Group.Units.GroupRoleUnit";
local FriendInfo = require "UI.Friend.Data.FriendInfoData";
local PlayerData = require "Logic.Entity.Data.PlayerData";
local RoleCardModule = Class("RoleCardModule", UIComponent);
local M = RoleCardModule;

function M:GetLayoutPath()
	return "UI/Prefab/Module/Module_Role_Card";
end

function M:OnParse(params)
	if params then
		self.params = params;
	end
end

function M:OnCreate()
    self.roleUnit = GroupRoleUnit.New(self:GetChildByPath("g_var/Group_Roleicon"));
    self.nameText = self:GetComponentByPath("g_var/g_palyer/et_name", "TextMeshProUGUI");
	self.chatBtn = UIButton.New(self:GetChildByPath("g_var/g_social/ebtn_01"));
	self.friendBtn = UIButton.New(self:GetChildByPath("g_var/g_social/ebtn_02"));
	self.spaceBtn = UIButton.New(self:GetChildByPath("g_var/g_social/ebtn_03"));

	-- 自定义按钮
	self.custom4Btn = UIButton.New(self:GetChildByPath("g_var/g_behavior/ebtn_04"));
	self.custom5Btn = UIButton.New(self:GetChildByPath("g_var/g_behavior/ebtn_05"));
	self.custom6Btn = UIButton.New(self:GetChildByPath("g_var/g_behavior/ebtn_06"));
	self.custom7Btn = UIButton.New(self:GetChildByPath("g_var/g_behavior/ebtn_07"));

	self.signGameObject = {};
	self.signText = {};
	self.customSignGos = {};
	for i = 1, 4 do
		self.signGameObject[i] = self:GetGameObjectByPath("g_var/g_sign_0" .. tostring(i));
		self.signText[i] = self:GetComponentByPath(string.format("g_var/g_sign_0" .. tostring(i) .. "/et_text"), "TextMeshProUGUI");
		self.customSignGos[i] = self:GetGameObjectByPath("g_var/g_sign_0"..i.."/ep_bg_02");
	end
	self.infoText = self:GetComponentByPath("g_var/et_text", "TextMeshProUGUI");
	self.infoText.text = "";

	self.bg = self:GetChildByPath("g_comm/p_bg");
	self:SetSize(self.bg.sizeDelta.x, self.bg.sizeDelta.y)

	self.popularityText = self:GetComponentByPath("g_var/g_hot/et_num", "TextMeshProUGUI");
	self.sexTrans = self:GetChildByPath("g_var/g_palyer/ep_sex");
end

function M:OnUpdate()
	-- 刷新自定义按钮的显示和隐藏
	for k = 4, 7 do
		if self.params["btn"..k] then
			self["custom"..k.."Btn"]:Show()
		else
			self["custom"..k.."Btn"]:Hide()
		end
	end

    self.roleUnit:SetLevel(self.params.rolelevel);
    self.roleUnit:SetIcon(self.params.avatarid);
	self.nameText.text = self.params.rolename;

	local selfWidth, selfHeight = self:GetRectSize();
	-- local uiPos = TransformUtil.GetRightPos(self:GetTransform(), nil, selfWidth/2 , selfHeight/2);
	-- self:SetLocalPosition(uiPos);
	TransformUtil.SetClickPos(self:GetTransform(), nil, selfWidth/2, selfHeight/2);
	RoleSpaceManager:GetInstance():RequestOpenSpace(self.params.roleid);

	-- 正在私聊中 隐藏私聊按钮
	if ChatSystem:GetInstance():GetCurrentChannelType() == ChatChannel.Friend and
		ChatSystem:GetInstance():IsDialogOpen() == true then
		self.chatBtn:Hide();
	else
		self.chatBtn:Show();
	end

	-- 如果已经是好友则隐藏好友按钮
	local friendInfo = FriendSystem:GetInstance():GetMyFriendByRoleId(self.params.roleid);
	if not friendInfo then
		self.friendBtn:Show();
	else
		self.friendBtn:Hide();
	end
end


function M:OnAddListener()
	self.custom4Btn:AddEventListener(ButtonEvent.CLICK, self.OnBtn4Click, self)
	self.custom5Btn:AddEventListener(ButtonEvent.CLICK, self.OnBtn5Click, self)
	self.custom6Btn:AddEventListener(ButtonEvent.CLICK, self.OnBtn6Click, self)
	self.custom7Btn:AddEventListener(ButtonEvent.CLICK, self.OnBtn7Click, self)

	self.chatBtn:AddEventListener(ButtonEvent.CLICK, self.OnClickChat, self)
	self.friendBtn:AddEventListener(ButtonEvent.CLICK, self.OnClickFriend, self)
	self.spaceBtn:AddEventListener(ButtonEvent.CLICK, self.OnClickSpace, self)
end

function M:OnRemoveListener()
	self.custom4Btn:RemoveEventListener(ButtonEvent.CLICK, self.OnBtn4Click)
	self.custom5Btn:RemoveEventListener(ButtonEvent.CLICK, self.OnBtn5Click)
	self.custom6Btn:RemoveEventListener(ButtonEvent.CLICK, self.OnBtn6Click)
	self.custom7Btn:RemoveEventListener(ButtonEvent.CLICK, self.OnBtn7Click)

	self.chatBtn:RemoveEventListener(ButtonEvent.CLICK, self.OnClickChat)
	self.friendBtn:RemoveEventListener(ButtonEvent.CLICK, self.OnClickFriend)
	self.spaceBtn:RemoveEventListener(ButtonEvent.CLICK, self.OnClickSpace)
end

function M:OnAddDynamicListener()
	UIManager:GetInstance():RegisterComponentClick(self, self.OnClickClose, self);
	BroadcastEventManager:GetInstance():AddListener(RoleSpaceRefreshEvent, self.OnRefreshInfo, self);
end

function M:OnRemoveDynamicListener()
	UIManager:GetInstance():UnregisterComponentClick(self);
	BroadcastEventManager:GetInstance():RemoveListener(RoleSpaceRefreshEvent, self.OnRefreshInfo, self);
end

function M:OnClickClose()
	DialogManager:GetInstance():CloseDialog(DialogType.RoleCard);
end

function M:OnClickChat()

	local friendInfo = FriendInfo.New();
	friendInfo.baseinfo = FriendBaseInfo.New();
	friendInfo:SetRoleId(self.params.roleid)
	friendInfo:SetRoleName(self.params.rolename);
	friendInfo:SetLevel(self.params.rolelevel);
	friendInfo:SetAvatarId(self.params.avatarid);
	friendInfo:SetState(FriendState.ONLINE);
	ChatSystem:GetInstance():AddToRecentChat(friendInfo, true);
	ChatSystem:GetInstance():OpenChatDialog(ChatChannel.Friend, friendInfo);
	self:OnClickClose();
end

function M:OnClickFriend()
	local friendInfo = FriendApplyInfo.New();
	friendInfo.baseinfo = FriendBaseInfo.New();
	friendInfo.baseinfo:SetRoleId(self.params.roleid);
	friendInfo.baseinfo:SetRoleName(self.params.rolename);
	FriendSystem:GetInstance():SetCurrentSelectedFriendInfo(friendInfo)
	DialogManager:GetInstance():OpenDialog(DialogType.AddFriend)
	self:OnClickClose();
end

function M:OnClickSpace()
	local playerData = PlayerData.New();
	playerData:Init(self.params.roleid);
	playerData:UpdateAvatarId(self.params.avatarid);
	playerData:UpdateName(self.params.rolename);
	playerData:UpdateLevel(self.params.level);
	BroadcastEventManager:GetInstance():Bingo("ChatDialog_CloseSelf");
    if self.params.isFromChat then
        RoleSpaceManager:GetInstance():OpenDialogFromChat(playerData:GetKey());
    else
        RoleSpaceManager:GetInstance():OpenDialog(playerData:GetKey())
    end
	self:OnClickClose();
end

function M:OnBtn4Click()
	if self.params.btn4 and self.params.btn4.callback then
		self.params.btn4.callback(self.params.btn4.callbackObj);
		self:OnClickClose();
	end
end

function M:OnBtn5Click()
	if self.params.btn5 and self.params.btn5.callback then
		self.params.btn5.callback(self.params.btn5.callbackObj);
		self:OnClickClose();
	end
end

function M:OnBtn6Click()
	if self.params.btn6 and self.params.btn6.callback then
		self.params.btn6.callback(self.params.btn6.callbackObj);
		self:OnClickClose();
	end
end

function M:OnBtn7Click()
	if self.params.btn7 and self.params.btn7.callback then
		self.params.btn7.callback(self.params.btn7.callbackObj);
		self:OnClickClose();
	end
end

function M:OnRefreshInfo()
	local playerInfo = RoleSpaceManager:GetInstance():GetPlayerInfo();

	-- 个性签名
	self.infoText.text = playerInfo:GetPersonalSign();
	local personalSign = playerInfo:GetPersonalSign();
	if personalSign == "" or string.len(personalSign) < 1 then
		if playerInfo:GetPlayerType() ~= RoleType.Owner then
			self.infoText.text = BeanUtil.GetResStringMsg(10241);
		else
			-- self.infoText.text = BeanUtil.GetResStringMsg(10248);
		end
	end

	-- 个性标签
	local tags = playerInfo:GetTags();
	local personalIds = playerInfo:GetPersonalIds();
	local personalTagCount = (#personalIds);
	for i = 1, 4 do
		if tags[i] then
			self.signGameObject[i]:SetActive(true);
			self.signText[i].text = tags[i];
			if i <= personalTagCount then
				self.customSignGos[i]:SetActive(true);
			else
				self.customSignGos[i]:SetActive(false);
			end
		else
			self.signGameObject[i]:SetActive(false);
		end
	end

	-- 人气值	
	self.popularityText.text = playerInfo:GetPopularity();

	-- 性别	
	if playerInfo:GetSex() == GenderType.MALE then
		self.sexTrans.gameObject:SetActive(true);
		ImageUtils.SetUIImage("ui_newcf_sign_boy", self.sexTrans);
	elseif playerInfo:GetSex() == GenderType.FEMALE then
		self.sexTrans.gameObject:SetActive(true);
		ImageUtils.SetUIImage("ui_newcf_sign_girl", self.sexTrans);
	else
		self.sexTrans.gameObject:SetActive(false);
	end
end

function M:OnClear()
end

function M:OnDestroy()
end

return M;