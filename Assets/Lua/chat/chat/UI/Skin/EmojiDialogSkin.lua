--[[
	ChatDialog 皮肤
]]
local EmojiDialogSkin = Class("EmojiDialogSkin", UISkin);
local M = EmojiDialogSkin;

function M:InitTrans(transform)
end

function M:OnCreate(transform)
	UISkin.OnCreate(self, transform);
	--以下直接写控件,GenChild在上层已写,获取路径格式a/b替代a.b
	self.emojiTrans= self:GetChildByPath("g_var/Scroll_Module_CF_Input_Emjoy/Group_ScrollRect");
	self.emojiGameObject = self:GetGameObjectByPath("g_var/Scroll_Module_CF_Input_Emjoy");
	self.gifTrans= self:GetChildByPath("g_var/Scroll_Module_CF_Input_Gif/Group_ScrollRect");
	self.gifGameObject= self:GetGameObjectByPath("g_var/Scroll_Module_CF_Input_Gif");
	self.messageTrans= self:GetChildByPath("g_var/Scroll_Module_CF_Input_Message/Group_ScrollRect");
	self.messageGameObject= self:GetGameObjectByPath("g_var/Scroll_Module_CF_Input_Message");
	self.toggleGroup = self:GetChildByPath("g_var/g_tab");

	self.stateGameObject = self:GetGameObjectByPath("g_var/g_state");
	self.statePicGameObjects = {}
	self.statePicChooseGameObjects = {}
	for i = 1, 4 do
		self.statePicGameObjects[i] = self:GetGameObjectByPath("g_var/g_state/p_pic_0" .. i);
		self.statePicChooseGameObjects[i] = self:GetGameObjectByPath("g_var/g_state/p_pic_0" .. i .. "/ep_choose");
	end

end

return M;