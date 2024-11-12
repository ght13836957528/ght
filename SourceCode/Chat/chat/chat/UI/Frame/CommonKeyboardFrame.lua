
local CommonKeyboardFrame = Class("CommonKeyboardFrame", UIComponent);
local M = CommonKeyboardFrame;

KeyboardOperation = {
	ADD = 1,
	DELETE = 2,
	CONFIRM = 3,
}

function M:GetLayoutPath()
	return "UI/Prefab/Module/Module_Comm_Keyboard";
end

function M:OnCreate()
	self.numBtn = {};
	for i = 1, 9 do
		self.numBtn[i] = UIButton.New(self:GetChildByPath("g_var/ebtn_num_0"..i));
	end
	for j = 10, 12 do
		self.numBtn[j] = UIButton.New(self:GetChildByPath("g_var/ebtn_num_"..j));
	end
	local bg = self:GetChildByPath("g_comm/p_bg_01");
	self:SetSize(bg.sizeDelta.x, bg.sizeDelta.y);
end

function M:OnDestroy()
end

function M:OnAddListener()

	self.numBtn[1]:AddEventListener(ButtonEvent.CLICK, self.OnClickNum1, self);
	self.numBtn[2]:AddEventListener(ButtonEvent.CLICK, self.OnClickNum2, self);
	self.numBtn[3]:AddEventListener(ButtonEvent.CLICK, self.OnClickNum3, self);
	self.numBtn[4]:AddEventListener(ButtonEvent.CLICK, self.OnClickDelete, self);
	self.numBtn[5]:AddEventListener(ButtonEvent.CLICK, self.OnClickNum4, self);
	self.numBtn[6]:AddEventListener(ButtonEvent.CLICK, self.OnClickNum5, self);
	self.numBtn[7]:AddEventListener(ButtonEvent.CLICK, self.OnClickNum6, self);
	self.numBtn[8]:AddEventListener(ButtonEvent.CLICK, self.OnClickNum0, self);
	self.numBtn[9]:AddEventListener(ButtonEvent.CLICK, self.OnClickNum7, self);
	self.numBtn[10]:AddEventListener(ButtonEvent.CLICK, self.OnClickNum8, self);
	self.numBtn[11]:AddEventListener(ButtonEvent.CLICK, self.OnClickNum9, self);
	self.numBtn[12]:AddEventListener(ButtonEvent.CLICK, self.OnClickConfirm, self);
end

function M:OnRemoveListener()
	self.numBtn[1]:RemoveEventListener(ButtonEvent.CLICK, self.OnClickNum1);
	self.numBtn[2]:RemoveEventListener(ButtonEvent.CLICK, self.OnClickNum2);
	self.numBtn[3]:RemoveEventListener(ButtonEvent.CLICK, self.OnClickNum3);
	self.numBtn[4]:RemoveEventListener(ButtonEvent.CLICK, self.OnClickDelete);
	self.numBtn[5]:RemoveEventListener(ButtonEvent.CLICK, self.OnClickNum4);
	self.numBtn[6]:RemoveEventListener(ButtonEvent.CLICK, self.OnClickNum5);
	self.numBtn[7]:RemoveEventListener(ButtonEvent.CLICK, self.OnClickNum6);
	self.numBtn[8]:RemoveEventListener(ButtonEvent.CLICK, self.OnClickNum0);
	self.numBtn[9]:RemoveEventListener(ButtonEvent.CLICK, self.OnClickNum7);
	self.numBtn[10]:RemoveEventListener(ButtonEvent.CLICK, self.OnClickNum8);
	self.numBtn[11]:RemoveEventListener(ButtonEvent.CLICK, self.OnClickNum9);
	self.numBtn[12]:RemoveEventListener(ButtonEvent.CLICK, self.OnClickConfirm);
end

function M:OnShow()
	self:RegisterClick();
end

function M:OnHide()
	self:UnregisterClick();
end

function M:RegisterClick()
	UIManager:GetInstance():RegisterComponentClick(self, self.OnClose, self);
end

function M:UnregisterClick()
	UIManager:GetInstance():UnregisterComponentClick(self);
end

function M:OnClickNum1()
	BroadcastEventManager:GetInstance():Bingo("KeyboardClickEvent", {operation = KeyboardOperation.ADD, num = 1})
end

function M:OnClickNum2()
	BroadcastEventManager:GetInstance():Bingo("KeyboardClickEvent", {operation = KeyboardOperation.ADD, num = 2})
end

function M:OnClickNum3()
	BroadcastEventManager:GetInstance():Bingo("KeyboardClickEvent", {operation = KeyboardOperation.ADD, num = 3})
end

function M:OnClickNum4()
	BroadcastEventManager:GetInstance():Bingo("KeyboardClickEvent", {operation = KeyboardOperation.ADD, num = 4})
end

function M:OnClickNum5()
	BroadcastEventManager:GetInstance():Bingo("KeyboardClickEvent", {operation = KeyboardOperation.ADD, num = 5})
end

function M:OnClickNum6()
	BroadcastEventManager:GetInstance():Bingo("KeyboardClickEvent", {operation = KeyboardOperation.ADD, num = 6})
end

function M:OnClickNum7()
	BroadcastEventManager:GetInstance():Bingo("KeyboardClickEvent", {operation = KeyboardOperation.ADD, num = 7})
end

function M:OnClickNum8()
	BroadcastEventManager:GetInstance():Bingo("KeyboardClickEvent", {operation = KeyboardOperation.ADD, num = 8})
end

function M:OnClickNum9()
	BroadcastEventManager:GetInstance():Bingo("KeyboardClickEvent", {operation = KeyboardOperation.ADD, num = 9})
end

function M:OnClickNum0()
	BroadcastEventManager:GetInstance():Bingo("KeyboardClickEvent", {operation = KeyboardOperation.ADD, num = 0})
end

function M:OnClickConfirm()
	BroadcastEventManager:GetInstance():Bingo("KeyboardClickEvent", {operation = KeyboardOperation.CONFIRM})
end

function M:OnClickDelete()
	BroadcastEventManager:GetInstance():Bingo("KeyboardClickEvent", {operation = KeyboardOperation.DELETE})
end

function M:OnClose()
	BroadcastEventManager:GetInstance():Bingo("KeyboardCloseEvent", true);
end


return M;