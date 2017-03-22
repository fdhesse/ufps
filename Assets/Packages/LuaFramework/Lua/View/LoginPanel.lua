local transform;
local gameObject;

LoginPanel = {};
local this = LoginPanel;

--启动事件--
function LoginPanel.Awake(obj)
	gameObject = obj;
	transform = obj.transform;

	this.InitPanel();
	--logWarn("Awake lua--->>"..gameObject.name);
end

--初始化面板--
function LoginPanel.InitPanel()
	this.btnConfirm = transform:FindChild("Confirm").gameObject;
	this.btnCancel = transform:FindChild("Cancel").gameObject;
	this.inputUsername = transform:FindChild("Username"):GetComponent("InputField");
	this.inputPasswd = transform:FindChild("Password"):GetComponent("InputField");
  
end

--单击事件--
function LoginPanel.OnDestroy()
	--logWarn("OnDestroy---->>>");
  transform = nil;
  gameObject = nil;
  this.btnClose = nil;
end

