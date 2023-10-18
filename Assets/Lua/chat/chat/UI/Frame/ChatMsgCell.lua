local UIComponent = require "UI.Base.UIComponent";
local GroupRoleUnit = require "UI.Common.Group.Units.GroupRoleUnit";
ChatMsgCell = Class("ChatMsgCell", UIComponent);
local  M = ChatMsgCell;

function M:GetLayoutPath()
	return "UI/Prefab/Module/Module_Chat_Paopao";
end

function M:OnInit()
	self.size = Vector2.zero;
	self.isNotice = false;
	self.isSelf = false;
    self.randomEmojiTimer = nil;
	self.isInTick = false;
	self.tickTotleDelta = 0;
end

function M:OnCreate()
	self:GenChildPathMap(self.selfTransform);

	-- 通知消息
	self.notice 	= self:GetGameObjectByPath("g_notice");
	self.noticeTxt 	= self:GetComponentByPath("g_notice/et_text", "TextMeshProUGUI");
	self.notiveTxtGO= self:GetGameObjectByPath("g_notice/et_text");

	---- 左侧泡泡
	self.left 		 = self:GetGameObjectByPath("g_other");
	self.leftUnit 	 = GroupRoleUnit.New(self:GetChildByPath("g_other/Group_Roleicon"));
	self.leftName 	 = self:GetComponentByPath("g_other/g_name/et_name", "TextMeshProUGUI");
	self.leftSexGO   = self:GetGameObjectByPath("g_other/g_name/ep_sex");
	self.leftSex     = self:GetChildByPath("g_other/g_name/ep_sex");
	self.leftTxt1    = self:GetComponentByPath("g_other/g_paopao/p_bg/et_text", "TextMeshProUGUI");		-- 图文混排文本
	self.leftTxt1GO	 = self:GetGameObjectByPath("g_other/g_paopao/p_bg/et_text");
	self.leftBoxRT 	 = self:GetComponentByPath("g_other/g_paopao", "RectTransform");
	self.leftBgRT  	 = self:GetComponentByPath("g_other/g_paopao/p_bg", "RectTransform");
    --
	---- 右侧泡泡
	self.right 		  = self:GetGameObjectByPath("g_me");
	self.rightUnit 	  = GroupRoleUnit.New(self:GetChildByPath("g_me/Group_Roleicon"));
	self.rightName 	  = self:GetComponentByPath("g_me/g_name/et_name", "TextMeshProUGUI");
	self.rightSexGO   = self:GetGameObjectByPath("g_me/g_name/ep_sex");
	self.rightSex     = self:GetChildByPath("g_me/g_name/ep_sex");
	self.rightTxt1 	  = self:GetComponentByPath("g_me/g_paopao/p_bg/et_text", "TextMeshProUGUI");			-- 图文混排文本
	self.rightTxt1GO  = self:GetGameObjectByPath("g_me/g_paopao/p_bg/et_text");
	self.rightBoxRT   = self:GetComponentByPath("g_me/g_paopao", "RectTransform");
	self.rightBgRT 	  = self:GetComponentByPath("g_me/g_paopao/p_bg", "RectTransform");
    --
	---- 默认参数
	self.defTxtSize = {};
	self.defTxtSize.width = self.leftTxt1.rectTransform.rect.width;
	self.defTxtSize.height = self.leftTxt1.rectTransform.rect.height;
    --
	self.leftTxtPos = self.leftTxt1.rectTransform.anchoredPosition;
	self.rightTxtPos = self.rightTxt1.rectTransform.anchoredPosition;
	self.defNoticeWidth = 135;
end

function M:SetDiffWidth(width)
	self.defTxtSize.width = self.defTxtSize.width + width;
end

function M:OnClose()
end

function M:OnDestroy()
end

function M:OnParse(args)
	self.message = args;
end

function M:OnHide()
end

function M:OnShow()
end

function M:OnUpdate()

	local message = self.message;
	if not message or TableUtil.TableLength(message) == 0 then return end
	-- 是否是通知
	self.isNotice = message:IsNotice();
	-- 是否是自己
	if UserManager and UserManager:HasInstance() then
		self.isSelf = UserManager:GetInstance():GetID() == message.roleid;
	end

	self:ShowType(self.isNotice, self.isSelf);

	-- 通知消息处理
	if self.isNotice then
		self:SetNotice();
		self:UpdateLayout();

	-- 聊天消息处理
	else
		self:SetRoleInfo();
		local isHyper = false;
		-- TODO 现在没有urlType不用区分语音信息文字信息，这里先写死之后协议添加urlType的时候再加一下
		local urlType = 0;
		local content = message.msg;
		-- 文本消息
		if urlType == 0 then
			self:SetText(content, message.recruitInfo);
			self:UpdateLayout();
		end
	end
end

function M:SetRoleInfo()
	self.curName.text = self.message.rolename;
	if self.message.sex == GenderType.MALE then
		self.curSexGO:SetActive(true);
		ImageUtils.SetUIImage("ui_newcf_sign_boy", self.curSex);
	elseif self.message.sex == GenderType.FEMALE then
		self.curSexGO:SetActive(true);
		ImageUtils.SetUIImage("ui_newcf_sign_girl", self.curSex);
	else
		self.curSexGO:SetActive(false);
	end
	self.curUnit:SetLevel(self.message.rolelevel);
	self.curUnit:SetIcon(self.message.avatarid);
	self.curUnit:SetCallback(self.OnRoleClick, self);
end

--[[
	@private 控制显示的结构类型
--]]
function M:ShowType(isNotice, isSelf)
	if isNotice then
		if not self.notice.activeSelf then
			self.notice:SetActive(true);
		end

		if self.left.activeSelf then
			self.left:SetActive(false);
		end

		if self.right.activeSelf then
			self.right:SetActive(false);
		end
	else
		if self.notice.activeSelf then
			self.notice:SetActive(false);
		end

		if isSelf == self.left.activeSelf then
			self.left:SetActive(not isSelf);
		end

		if isSelf ~= self.right.activeSelf then
			self.right:SetActive(isSelf);
		end

		local pref = isSelf and "right" or "left";
		self.curUnit 	= 	self[pref .. "Unit"];
		self.curName 	= 	self[pref .. "Name"];
		self.curSexGO	= 	self[pref .. "SexGO"];
		self.curSex		= 	self[pref .. "Sex"];
		self.curBtn 	= 	self[pref .. "Btn"];
		self.curTitle 	= 	self[pref .. "Title"];
		self.curTitleGO = 	self[pref .. "TitleGO"];
		self.curTitleRT =   self[pref .. "TitleRT"];
		self.curTitleTxt =  self[pref .. "TitleTxt"];

		self.curVoiceGO = 	self[pref .. "VoiceGO"];
		self.curVoiceBgRT = self[pref .. "VoiceBgRT"];
		self.curVoiceTxt =  self[pref .. "VoiceTxt"];
		self.curVoiceImage =self[pref .. "VoiceImage"];

		self.curImage 	= 	self[pref .. "Image"];

		self.curTxt1	= 	self[pref .. "Txt1"];	-- 图文混排文本
		self.curTxt1GO	=   self[pref .. "Txt1GO"];
		self.curTxt2  	= 	self[pref .. "Txt2"];	-- 普通文本

		self.curBoxRT 	= 	self[pref .. "BoxRT"];
		self.curBgRT 	= 	self[pref .. "BgRT"];
		self.curTxtPos  =  	self[pref .. "TxtPos"];

		self.curRetIcon = 	self[pref .. "RetIcon"];
		self.curRetBtn 	=   self[pref .. "RetBtn"];
		self.curNameRT  =   self[pref .. "NameRT"];

		self.curCustomSign = nil;
		if not isSelf then
			self.curCustomSign = self.leftCustomSign;
		end


		self.curTxtRect = self.curTxt1.rectTransform;

	end
end

--[[
	@private 消息泡泡设置文本
--]]
function M:SetText(content, recruitInfo)
	if not self.curTxt1GO.activeSelf then
		self.curTxt1GO:SetActive(true);
	end
	self.curTxt = self.curTxt1;

	local sizeDelta = self.curTxtRect.sizeDelta;
	sizeDelta.x = self.defTxtSize.width;
	self.curTxtRect.sizeDelta = sizeDelta;
	sizeDelta:Recycle();
	local finalContent = ChatSystem:GetInstance():ReplaceEmojiText(content);
	--local afterReplaceSpaceContent = ChatSystem:GetInstance():ReplaceSpace(finalContent);
	self.curTxt.text = finalContent;
	if recruitInfo ~= nil then
		local linkEvent = self.curTxt.transform:GetComponent("TextMeshProLinkEvent");
		if linkEvent then
			linkEvent:SetKey("ChatSystem");
			linkEvent:SetParam(recruitInfo.teamid);
		else
			info("no linkEvent found")
		end
	end
end

function M:SetNotice()
	if not self.message then return end
	local message = self.message;

	local isHyper = false;
	local msgType = message:GetMsgType();
	local content = message:GetContent();

	local color = "ffffffff";
	local pic = nil;
	local recorder = BeanConfigManager:GetInstance():GetTableByName("ares.logic.chat.cchatconfig"):GetRecorder(msgType);
	if recorder then
		color = recorder.color;
		pic = recorder.pic;
	end

	if not self.notiveTxtGO.activeSelf then
		self.notiveTxtGO:SetActive(true);
	end
	self.curTxt = isHyper and self.noticeTxt1 or self.noticeTxt;
	self.curTxtRect = self.curTxt.rectTransform;
	local sizeDelta = self.curTxtRect.sizeDelta;
	sizeDelta.x = self.defNoticeWidth;
	self.curTxtRect.sizeDelta = sizeDelta;
	sizeDelta:Recycle();
	self.curTxt.text = content and content or "";
end

function M:StartRenderTextTimer()
	if not self.renderTextTimer then
		self.renderTextTimer = require "Framework.Tick.Timer".New(1, M.RenderTextTimer, self, false, true);
	end
	self.renderTextTimer:Start();
end

function M:RenderTextTimer()
	local renderedWidth = self.curTxt.renderedWidth;
	local maxWidth = self.defTxtSize.width;
	local width = ((renderedWidth >= maxWidth) and maxWidth) or renderedWidth;
	local sizeDelta = Vector2.New(width + 5, self.curTxt.preferredHeight + 5);
	self.curTxtRect.sizeDelta = sizeDelta;-- 加是为了防止单个字母或数字的时候无法显示
	self.curTxtRect.anchoredPosition = self.curTxtPos;


	-- 消息泡泡大小调整
	sizeDelta.x = sizeDelta.x + 55;
	sizeDelta.y = sizeDelta.y + 28;
	self.curBgRT.sizeDelta = sizeDelta;
	--self.curBoxRT.sizeDelta = sizeDelta;
	local ap = self.curBoxRT.anchoredPosition;
	self:SetSize(0, math.abs(ap.y) + sizeDelta.y + 30);
	if self.renderTextTimer then
		self.renderTextTimer:Stop();
		self.renderTextTimer:Destroy();
		self.renderTextTimer = nil;
	end
end

--[[
	@private 更新布局
--]] 
function M:UpdateLayout()

	if self.message:IsNotice() then
		local preferredWidth = self.curTxt.preferredWidth;
		local maxWidth = self.defNoticeWidth;

		local width = preferredWidth

		if preferredWidth >= maxWidth then
			width = maxWidth;
		end

		local height = self.curTxt.preferredHeight + 10;
		-- 加是为了防止单个字母或数字的时候无法显示
		local sizeDelta = Vector2.New(width + 2, height);
		self.curTxtRect.sizeDelta = sizeDelta;
		sizeDelta:Recycle();
		width = self.curTxt.preferredWidth;
		self:SetSize(width + 2, height + 5)		
	else
		-- TODO 暂时没有语音信息的需求只处理文字信息，urlType先写死0
		--local urlType = self.message:GetUrlType();
		local urlType = 0;
		-- 文本消息
		if urlType == 0 then
			-- 文本RectTransform大小调整
			local preferredWidth = self.curTxt.preferredWidth;
			local maxWidth = self.defTxtSize.width;
			local width = ((preferredWidth >= maxWidth) and maxWidth) or preferredWidth;
			local sizeDelta = Vector2.New(width + 2, self.curTxt.preferredHeight + 5);
			self.curTxt.rectTransform.sizeDelta = sizeDelta;-- 加是为了防止单个字母或数字的时候无法显示
			self.curTxt.rectTransform.anchoredPosition = self.curTxtPos;


			-- 消息泡泡大小调整
			sizeDelta.x = sizeDelta.x + 55;
			sizeDelta.y = sizeDelta.y + 28;
			self.curBgRT.sizeDelta = sizeDelta;
			--self.curBoxRT.sizeDelta = sizeDelta;
			local ap = self.curBoxRT.anchoredPosition;
			self:SetSize(0, math.abs(ap.y) + sizeDelta.y + 30);

			-- 一帧之后重新获取字体渲染长度
			--self:StartRenderTextTimer();

			--local width = ((preferredWidth >= maxWidth) and maxWidth) or preferredWidth;
			--local sizeDelta = Vector2.New(width + 2, self.curTxt.preferredHeight + 5);
			--self.curTxtRect.sizeDelta = sizeDelta;-- 加是为了防止单个字母或数字的时候无法显示
			--self.curTxtRect.anchoredPosition = self.curTxtPos;
            --
			--info("222 self.curTxt.preferredWidth = " .. self.curTxt.preferredWidth);
			--info("222 self.curTxt.preferredHeight = " .. self.curTxt.preferredHeight)
			--sizeDelta.x = sizeDelta.x + 30;
			--sizeDelta.y = sizeDelta.y + 20;
			--self.curBgRT.sizeDelta = sizeDelta;
			----self.curBoxRT.sizeDelta = sizeDelta;
			--local ap = self.curBoxRT.anchoredPosition;
			--self:SetSize(0, math.abs(ap.y) + sizeDelta.y + 30);


			sizeDelta:Recycle();
			ap:Recycle();
		end			
	end
end

--[[
	@private 注册UI事件监听
--]]
function M:RegisterListener()
	self.leftBtn:AddEventListener(ButtonEvent.CLICK, M.OnPlayClick, self);
	self.rightBtn:AddEventListener(ButtonEvent.CLICK, M.OnPlayClick, self);
	self.leftRetBtn:AddEventListener(ButtonEvent.CLICK, M.ShowOldPlayerRetTips);
	self.rightRetBtn:AddEventListener(ButtonEvent.CLICK, M.ShowOldPlayerRetTips);
end

--[[
	@private 注销UI事件监听
--]]
function M:UnregisterListener()
	if self.leftBtn then
		self.leftBtn:RemoveEventListener(ButtonEvent.CLICK, M.OnPlayClick);
	end
	if self.rightBtn then
		self.rightBtn:RemoveEventListener(ButtonEvent.CLICK, M.OnPlayClick);
	end	
	if self.leftRetBtn then
		self.leftRetBtn:RemoveEventListener(ButtonEvent.CLICK, M.ShowOldPlayerRetTips);
	end
	if self.rightRetBtn then
		self.rightRetBtn:RemoveEventListener(ButtonEvent.CLICK, M.ShowOldPlayerRetTips);
	end
end

-- 点击头像呼出交互面板
function M:OnRoleClick()
	local message = self.message;
	if not message or message:GetRoleID() == 0 then return; end
	if UserManager and UserManager:HasInstance() and UserManager:GetInstance():GetID() == message:GetRoleID() then return; end

	local passdata = {};
	passdata.rolelevel = message.rolelevel;
	passdata.avatarid = message.avatarid;
	passdata.rolename = message.rolename;
	passdata.roleid = message.roleid;

	passdata.btn4 = {};
	passdata.btn4.callback = self.OnShieldClick;
	passdata.btn4.callbackObj = self;

	passdata.btn5 = {};
	passdata.btn5.callback = self.OnReportClick;
	passdata.btn5.callbackObj = self;

	passdata.isFromChat = true;

	DialogManager:GetInstance():OpenDialog(DialogType.RoleCard, passdata);
end

-- 屏蔽按钮点击
function M:OnShieldClick()

	if FriendSystem:GetInstance():GetBlackListFriendByRoleId(self.message.roleid) then
		CommMsgMgr:GetInstance():Show(180085, {content = self.message.rolename});
		return;
	else
		local confirmToBlackList = function(self)
			FriendSystem:GetInstance():SendAddBlackList(self.message);
		end
		CommMsgMgr:GetInstance():Show(180187, {content = self.message.rolename}, self, nil, confirmToBlackList);
	end
end

-- 举报按钮点击
function M:OnReportClick()
	CommMsgMgr:GetInstance():Show(180002);
end

--[[
	@private 设置大小
--]]
function M:SetSize(x, y)
	if self.size == nil then
		self.size = Vector2.New(x, y);
	else
		self.size.x = x;
		self.size.y = y;
	end

	self.rectTransform.sizeDelta = self.size;
end

function M:GetHeight()
	if self.size then
		return self.size.y;
	elseif self.rectTransform then
		self.size = self.rectTransform.sizeDelta;
	else
		self.size = Vector2.zero;
	end

	return self.size.y;
end

function M:GetWidth()
	if self.size then
		return self.size.x;
	elseif self.rectTransform then
		self.size = self.rectTransform.sizeDelta;
	else
		self.size = Vector2.zero;
	end

	return self.size.x;
end

function M:GetPosX()
	if self:GetAnchoredPosition() then
		return self:GetAnchoredPosition().x;
	elseif self.rectTransform then
		self.anchoredPosition = self.rectTransform.anchoredPosition;
		return self.anchoredPosition.x;
	else
		return 0;
	end
end

function M:GetPosY()
	if self:GetAnchoredPosition() then
		return self:GetAnchoredPosition().y;
	elseif self.rectTransform then
		self.anchoredPosition = self.rectTransform.anchoredPosition;
		return self.anchoredPosition.y;
	else
		return 0;
	end
end

return M