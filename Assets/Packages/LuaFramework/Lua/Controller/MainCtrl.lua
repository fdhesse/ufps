
local MainCtrl = {};
local this = MainCtrl;

local message;
local transform;
local gameObject;

--构建函数--
function MainCtrl.New()
	--logWarn("LoginCtrl.New--->>");
	return this;
end

function MainCtrl.Awake()
	--logWarn("LoginCtrl.Awake--->>");
	panelMgr:CreatePanel('Main', this.OnCreate);
end

--启动事件--
function MainCtrl.OnCreate(obj)
	gameObject = obj;
  gameObject:SetActive(true);

	message = gameObject:GetComponent('LuaBehaviour');
	--message:AddClick(LoginPanel.btnClose, this.OnClick);
	--message:AddClick(MessagePanel.Button, this.OnClick);

	--logWarn("Start lua--->>"..gameObject.name);
  gameObject.transform:SetParent(nil);
end

--单击事件--
function MainCtrl.OnClick(go)
  Game.EnterGame(function()
      this.Close();
  end);
  --logWarn("Start lua--->>"..gameObject.name);
end

--关闭事件--
function MainCtrl.Close()
	panelMgr:ClosePanel('Main');
  message = nil;
  gameObject = nil;
end

return MainCtrl;
