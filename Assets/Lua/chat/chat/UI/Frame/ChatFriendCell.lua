local GroupRoleUnit = require "UI.Common.Group.Units.GroupRoleUnit";
local ChatFriendCell = Class("ChatFriendCell", UIScrollViewCell);
local  M = ChatFriendCell;


--[[
	创建：
	local item = CommonFriendCell.New();

	添加点击监听：
	item:SetClickSelfCallBack(callBack, listener, ...);

	设置图片
	item:SetIcon(iconPath, forceRefresh)

	设置技能名称
	item:SetName(name)
--]]

local NameColor = {
	SELECTED = "<color=#8A5F03AA>",
	UNSELECTED = "<color=#00548FFF>",
	OFFLINE = "<color=#888888FF>",
	POSTFIX = "</color>"
}

function M:GetLayoutPath()
	return "UI/Prefab/Module/Module_CF_Friend_List";
	--return "UI/Temp/Module_CF_Friend_List";
end

function M:OnCreate()
	UIScrollViewCell.OnCreate(self);

	self.name = "";

	self.roleUnit = GroupRoleUnit.New(self:GetChildByPath("Group_Roleicon"));
	self.nameText = self:GetComponentByPath("g_name/et_name", "TextMeshProUGUI");
	self.sexGO = self:GetGameObjectByPath("g_name/ep_sex");
	self.sexTrans = self:GetChildByPath("g_name/ep_sex");
	self.stateText = self:GetComponentByPath("et_text", "TextMeshProUGUI");
	self.stateGameObject = self:GetGameObjectByPath("ep_outline");
	self.scrollBtn = UIButton.New(self:GetChildByPath("ebtn_empty"));
	self.chooseObject = self:GetGameObjectByPath("ep_choose");
	self.unreadObject = self:GetGameObjectByPath("g_state");
	self.unreadTxt = self:GetComponentByPath("g_state/p_bg/et_num", "TextMeshProUGUI");
	self.unreadTxtObject = self:GetGameObjectByPath("g_state/p_bg/et_num");
end

function M:GetScrollButton()
	return self.scrollBtn;
end

function M:OnAddListener()
	UIScrollViewCell.OnAddListener(self);
	self.roleUnit:SetCallback(self.OnClickSelf, self);
end

function M:OnRemoveListener()
	UIScrollViewCell.OnRemoveListener(self);
end

function M:OnAddDynamicListener()
	BroadcastEventManager:GetInstance():AddListener("ChatFriendCell_RefreshNotice", self.RefreshNotice, self);
end


function M:OnRemoveDynamicListener()
	BroadcastEventManager:GetInstance():RemoveListener("ChatFriendCell_RefreshNotice", self.RefreshNotice, self);
end

function M:OnClickSelf()
	self:OnSingleClickCallBack();
end

-- 如果不作为scrollUnit加到界面中时，调用SetData接口主动设置技能信息
function M:SetData(data)
	if not data then
		return;
	end
	self.data = data;
	if self:IsLoaded() then
		self:ReloadLayout()
	end
end

function M:RefreshUnit(args)
	if not args then
		return
	end
	self.data = args;
	self:ReloadLayout()
	UIScrollViewCell.RefreshUnit(self, args);
end

function M:ReloadLayout()
	if not self:IsLoaded() then
		return
	end

	if not self.data or not self.data.baseinfo then
		if info then info("data or baseinfo not found!"); end
		return;
	end

	self:SetName(self.data.baseinfo.rolename)
	self:SetSex(self.data.baseinfo.sex)
	self.roleUnit:SetIcon(self.data.baseinfo.avatarid)
	self.roleUnit:SetLevel(self.data.baseinfo.level)
	local unit = self:GetUnit();
	if unit then
		self:RefreshSelected(unit:GetSelected());
	end

	self:RefreshNotice();
end

function M:SetSex(sex)
	if sex == GenderType.MALE then
		self.sexGO:SetActive(true);
		ImageUtils.SetUIImage("ui_newcf_sign_boy", self.sexTrans);
	elseif sex == GenderType.FEMALE then
		self.sexGO:SetActive(true);
		ImageUtils.SetUIImage("ui_newcf_sign_girl", self.sexTrans);
	else
		self.sexGO:SetActive(false);
	end
end

function M:RefreshNotice()
    if not self.data then
        return;
    end
	local count = ChatSystem:GetInstance():GetUnreadCountByRoleId(self.data.baseinfo.roleid);
	if count > 0 then
		self.unreadObject:SetActive(true);
		self.unreadTxt.text = count;
		self.unreadTxtObject:SetActive(true);
	else
		self.unreadObject:SetActive(false);
	end
end

function M:SetName(name)
	self.name = name;
end

function M:SetIcon(iconNum)
	if not iconNum then
		if info then info("no iconNum was sent!!!"); end
		return;
	end
	local config = BeanConfigManager:GetInstance():GetRecord("ares.logic.role.croleiconconfig", iconNum)
	if not config or not config.name then
		if info then info("no icon was found in croleiconconfig, id = "..iconNum) end
		return
	end
	local iconPath = config.name;
	ImageUtils.SetUIImage(iconPath, self.roleIconImg);
end

function M:SetLevel(level)
	self.levelText.text = level;
end

function M:RefreshSelected(isSelected)
	if not self.data.state then
		if info then info("self.data.state is missing! self.data = " .. TableUtil.TableToString(self.data)); end
		return;
	end
	self:SetState(self.data.state, self.data.offlinetime, isSelected);
end

function M:SetState(state, offlinetime, isSelected)
	if state == FriendState.ONLINE then
		self.stateGameObject:SetActive(false);
        self.roleUnit:SetGray(false);
		self.stateText.text = "<color=#39A3EDFF>" .. BeanUtil.GetResStringMsg(10061);
	elseif state == FriendState.GAMEMATCH then
		self.stateGameObject:SetActive(false);
        self.roleUnit:SetGray(false);
		self.stateText.text = "<color=#EB2828FF>" .. BeanUtil.GetResStringMsg(10063);
	elseif state == FriendState.INGAME then
		self.stateGameObject:SetActive(false);
        self.roleUnit:SetGray(false);
		self.stateText.text = "<color=#EB2828FF>" .. BeanUtil.GetResStringMsg(10062);
	elseif state == FriendState.OFFLINE then
		self.stateGameObject:SetActive(true);
        self.roleUnit:SetGray(true);
		local serverTime = GetServerTime();
		local time = serverTime - offlinetime / 1000;
		if time > 31536000 then
			self.stateText.text = "<color=#888888AA>" .. BeanUtil.GetResStringMsg(10069);
		elseif time <= 31536000  and time > 86400 then
			self.stateText.text = "<color=#888888AA>" .. BeanUtil.GetResStringMsg(10068, {math.floor(time / 86400)});
		elseif time <= 86400  and time > 3600 then
			self.stateText.text = "<color=#888888AA>" .. BeanUtil.GetResStringMsg(10067, {math.floor(time / 3600)});
		elseif time <= 3600  and time > 60 then
			self.stateText.text = "<color=#888888AA>" .. BeanUtil.GetResStringMsg(10066, {math.floor(time / 60)});
		elseif time <= 60  then
			self.stateText.text = "<color=#888888AA>" .. BeanUtil.GetResStringMsg(10065);
		end
	end

	self:RefreshState(isSelected);
end

function M:RefreshState(isSelected)
	if isSelected then
		self.chooseObject:SetActive(true);
		self.stateGameObject:SetActive(false);
		self.nameText.text = NameColor.SELECTED .. self.name .. NameColor.POSTFIX;
		return;
	end

	self.chooseObject:SetActive(false);

	if self.data.state == FriendState.OFFLINE then
		self.stateGameObject:SetActive(true);
		self.nameText.text = NameColor.OFFLINE .. self.name .. NameColor.POSTFIX;
	else
		self.stateGameObject:SetActive(false);
		self.nameText.text = NameColor.UNSELECTED .. self.name .. NameColor.POSTFIX;
	end
end


function M:OnClear()
	self.name = "";
end

return M;