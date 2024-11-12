local QuickMessageData = require "UI.Communication.Data.QuickMessageData";

local BattleImageParam = require("Net.Protocols.protolua.ares.logic.msg.battleimageparam");
local TeamQuickMessageCell = Class("TeamQuickMessageCell", UIScrollViewCell);
local M = TeamQuickMessageCell;

--资源路径
function M:GetLayoutPath()
	return "UI/Prefab/Module/Module_Chat_Quick_Cell";
end

--protected 此处可以添加皮肤代码
function M:OnCreate()
	UIScrollViewCell.OnCreate(self);
	self.showTxt = self:GetComponentByPath("et_text", "TextMeshProUGUI");
	self.btn = UIButton.New(self:GetChildByPath("ebtn_empty"));
end

function M:OnShow()
end

function M:OnHide()
	UIScrollViewCell.OnHide(self);

end

function M:OnDestroy()
end

function M:OnAddListener()
	self.btn:AddEventListener(ButtonEvent.CLICK, self.OnClickMsg, self);
end

function M:OnRemoveListener()
	self.btn:RemoveEventListener(ButtonEvent.CLICK, self.OnClickMsg, self);
end

-- 如果不作为scrollUnit加到界面中时，调用SetData接口主动设置技能信息
function M:SetData(data)
	self.data = data;
	if self:IsLoaded() then
		self:RefreshCell();
	end
end

function M:RefreshUnit(args)
	if not args then
		return
	end
	self.data = args;
	self:RefreshCell();
	UIScrollViewCell.RefreshUnit(self, args);
end

function M:RefreshCell()
	if not self.data then
		return;
	end
	self.showTxt.text = self.data:GetTag();
end

function M:OnClickMsg()
	BroadcastEventManager:GetInstance():Bingo("CommonChatModule_AddMessage", self.data:GetRealMsg());
	BroadcastEventManager:GetInstance():Bingo("CommonChatModule_CloseQuickChat");
end

return M;