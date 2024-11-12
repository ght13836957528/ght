local ChatDialogConfig = {
	--必要
	DialogType = DialogType.Chat;
	DialogPath = "UI.Communication.Dialog.ChatDialog";
	DialogLayer = UILayer.CHAT_LAYER;
	DialogLifeType = DialogLifeType.DontDestroy;
	DialogLifeTime = 300;
	--非必要
	ClickOutClose = false;
};

return ChatDialogConfig;