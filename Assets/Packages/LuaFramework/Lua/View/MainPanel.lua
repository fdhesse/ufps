local transform;
local gameObject;

MainPanel = {};
local this = MainPanel;

--启动事件--
function MainPanel.Awake(obj)
	gameObject = obj;
	transform = obj.transform;

	this.InitPanel();
	--logWarn("Awake lua--->>"..gameObject.name);
end

--初始化面板--
function MainPanel.InitPanel()
	--this.btnClose = transform:FindChild("Button").gameObject;
end

--单击事件--
function MainPanel.OnDestroy()
	--logWarn("OnDestroy---->>>");
  transform = nil;
  gameObject = nil;
  this.btnClose = nil;
end

