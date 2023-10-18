
local ChatBaseData = require "UI.Communication.Data.ChatBaseData";
local GifCell = Class("GifCell", UIScrollViewCell);
local M = GifCell;

--资源路径
function M:GetLayoutPath()
	return "UI/Prefab/Module/Module_CF_Input_Gif";
end

--protected 此处可以添加皮肤代码
function M:OnCreate()
	UIScrollViewCell.OnCreate(self);
	self.emojiTrans = self:GetChildByPath("ebtn_emjoy");
	self.emoji		= self:GetComponentByPath("et_text", "TextMeshProUGUI");
	self.emojiBtn 	= UIButton.New(self.emojiTrans);
end

function M:OnHide()
	UIScrollViewCell.OnHide(self);

end

function M:OnDestroy()
	self.emojiTrans = nil;
	self.emojiBtn = nil;
	self.emoji = nil;
end

function M:OnAddListener()
	self.emojiBtn:AddEventListener(ButtonEvent.CLICK, self.OnClickEmoji, self);
end

function M:OnRemoveListener()
	self.emojiBtn:RemoveEventListener(ButtonEvent.CLICK, self.OnClickEmoji);
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
	--ImageUtils.SetUIImage("ui_comm_money_jinbi", self.emojiImg);
	--change #str with "<sprite=\"emoji\" anim=\"0, 4, 10\">"
	self.emoji.text = StringUtil.CombineStr({"<sprite=\"", self.data.atlasname, "\" anim=\" ", self.data.indexstart, ", ", self.data.indexend, ", 20\">"});
end

function M:OnClickEmoji()
	BroadcastEventManager:GetInstance():Bingo("ChatDialog_SendEmoji", self.data.emoji);
end

return M;