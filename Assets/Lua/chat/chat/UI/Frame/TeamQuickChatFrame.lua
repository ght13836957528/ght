local TeamQuickChatFrame = Class("TeamQuickChatFrame", UIComponent);
local  M = TeamQuickChatFrame;

local ChatToggleIndex = {
	Quick = 1,
	Chat = 2
}

function M:GetLayoutPath()
	return "UI/Prefab/Module/Module_Chat_Quick"
end

function M:OnInit()
	self.quickMessageType = nil;
end

function M:OnDestroy()
	self.quickMessageType = nil;
end

function M:OnCreate()
	self.itemScroll = UIScrollView.New(self:GetChildByPath("Scroll_Module_Chat_Quick/Group_ScrollRect"));
	local param = {};
	param.direction = ScrollPaneCommon.SCROLLPANE_DIR_VERTICAL;
	param.unitSkin = require "UI.Communication.Frame.TeamQuickChatCell";
	param.itemEvents = {}
	self.itemScroll:SetParams(param);

	local messageType = self.quickMessageType or QuickMessageType.PREBATTLE;
	local data = QuickMessageSystem:GetInstance():GetMessageByType(messageType);
	self.itemScroll:ClearUnit();
	for i = 1, #data do
		local unit = ScrollUnit.New(data[i]);
		self.itemScroll:AddUnit(unit)
	end
end

function M:OnShow()
end

function M:OnHide()
end

function M:OnAddDynamicListener()
	UIManager:GetInstance():RegisterComponentClick(self, self.OnClickClose, self);
end

function M:OnRemoveDynamicListener()
	UIManager:GetInstance():UnregisterComponentClick(self);
end

function M:OnClickClose()
	BroadcastEventManager:GetInstance():Bingo("CommonChatModule_CloseQuickChat");
end

function M:SetQuickMessageType(type)
	local data = QuickMessageSystem:GetInstance():GetMessageByType(type);
	if not data then
		if info then info("quick message type not found, type = " .. tostring(type)); end
		return;
	end

	self.quickMessageType = type;
end

return M