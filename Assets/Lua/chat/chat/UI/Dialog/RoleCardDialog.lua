--[[
	TextTipDialog 面板
--]]
local RoleCardModule = require "UI.Communication.Frame.RoleCardModule";
local RoleCardDialog = Class("RoleCardDialog", UIDialog);
local M = RoleCardDialog;

function M:GetLayoutPath()
	return "UI/Prefab/Win/Win_Empty";
end

function M:OnInit()
end

function M:OnCreate()
	self.roleCard = RoleCardModule.New(nil, self.data);
	self:AddChild(self.roleCard);
end

function M:OnParse(params)
	self.data = params;
end

function M:OnUpdate()
	if self.roleCard then
		self.roleCard:Show(self.data);
	end
end

return M;