local RoleCardDialogConfig = {
	--必要
	DialogType = DialogType.RoleCard;
	DialogPath = "UI.Communication.Dialog.RoleCardDialog";
	DialogLayer = UILayer.TIP_LAYER;
	DialogLifeType = DialogLifeType.DestroyImmediate;
	DialogLifeTime = -1;
	--非必要
	ClickOutClose = false;
};

return RoleCardDialogConfig;