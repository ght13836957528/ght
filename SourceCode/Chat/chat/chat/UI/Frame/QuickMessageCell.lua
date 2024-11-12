local QuickMessageData = require "UI.Communication.Data.QuickMessageData";

local QuickMessageCell = Class("QuickMessageCell", UIScrollViewCell);
local M = QuickMessageCell;

--资源路径
function M:GetLayoutPath()
	return "UI/Prefab/Module/Module_CF_Input_Message";
end

--protected 此处可以添加皮肤代码
function M:OnCreate()
	UIScrollViewCell.OnCreate(self);
	self.btn = UIButton.New(self:GetChildByPath("ebtn_info"));
	self.quickText = self:GetComponentByPath("ebtn_info/et_text", "TextMeshProUGUI");
	self.lastWordGameObject = self:GetGameObjectByPath("ebtn_info/ep_lastword");
	self.lastWordText = self:GetComponentByPath("ebtn_info/ep_lastword/t_text", "TextMeshProUGUI");

	self.lastWordGameObject:SetActive(false);
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
	self.btn:RemoveEventListener(ButtonEvent.CLICK, self.OnClickMsg);
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
	self.quickText.text = self.data:GetShowMsg();
end

function M:OnClickMsg()
	BroadcastEventManager:GetInstance():Bingo("ChatDialog_AddMessage", self.data:GetRealMsg());
end

return M;