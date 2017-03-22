
local MessageCtrl = {};
local this = MessageCtrl;

local message;
local transform;
local gameObject;

--构建函数--
function MessageCtrl.New()
	--logWarn("MessageCtrl.New--->>");
	return this;
end

function MessageCtrl.Awake()
	--logWarn("MessageCtrl.Awake--->>");
	panelMgr:CreatePanel('Message', this.OnCreate);
end

function MessageCtrl.Start()
end

--启动事件--
function MessageCtrl.OnCreate(obj)
	gameObject = obj;
  
	message = gameObject:GetComponent('LuaBehaviour');
	message:AddClick(MessagePanel.btnClose, this.OnClick);
  
	--logWarn("Start lua--->>"..gameObject.name);
  gameObject:SetActive(false);
  gameObject:SetActive(true);  
end

--单击事件--
function MessageCtrl.OnClick(go)
  this.Close();
end

--关闭事件--
function MessageCtrl.Close()
	panelMgr:ClosePanel('Message');
  gameObject = nil;
end

function MessageCtrl.SetText(text)
  --print(text);
	MessagePanel.message.text = text;
end

return MessageCtrl;