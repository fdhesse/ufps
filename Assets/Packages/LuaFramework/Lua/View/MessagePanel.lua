local transform;
local gameObject;

MessagePanel = {};
local this = MessagePanel;

--启动事件--
function MessagePanel.Awake(obj)
	gameObject = obj;
	transform = obj.transform;

	this.InitPanel();
	--logWarn("Awake lua--->>"..gameObject.name);
end

--初始化面板--
function MessagePanel.InitPanel()
	this.btnClose = transform:FindChild("Button").gameObject;
  this.message = transform:FindChild("Message"):GetComponent("Text");
end

--单击事件--
function MessagePanel.OnDestroy()
	--logWarn("OnDestroy---->>>");
  gameObject = nil;
  transform = nil;
  this.btnClose = nil;
end

