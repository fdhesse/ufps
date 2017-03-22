
local MainMenuCtrl = {};
local this = MainMenuCtrl;

local message;
local transform;
local gameObject;

--构建函数--
function MainMenuCtrl.New()
	--logWarn("LoginCtrl.New--->>");
	return this;
end

function MainMenuCtrl.Awake()
	--logWarn("LoginCtrl.Awake--->>");
	panelMgr:CreatePanel('MainMenu', this.OnCreate);
end

--启动事件--
function MainMenuCtrl.OnCreate(obj)
	gameObject = obj;
  gameObject:SetActive(true);

	message = gameObject:GetComponent('LuaBehaviour');
	--message:AddClick(LoginPanel.btnClose, this.OnClick);
	--message:AddClick(MessagePanel.Button, this.OnClick);
  message:AddClick(MainMenuPanel.mBtnMap,this.OnClick)
  message:AddClick(MainMenuPanel.mBtnLiveEvent,this.OnClick)
  message:AddClick(MainMenuPanel.mBtnInventory,this.OnClick)
  message:AddClickByDir(gameObject,this.clickPVE,'Panel/PVE')
  message:AddClickByDir(gameObject,this.clickPVP,'Panel/PVP')
	--logWarn("Start lua--->>"..gameObject.name);
end

function MainMenuCtrl.clickPVE(go)
  logWarn('clickPVE')
end

function MainMenuCtrl.clickPVP(go)
  logWarn('clickPVP')
end

--单击事件--
function MainMenuCtrl.OnClick(go)
  logWarn('clickname:'..go.name)
  --[[local ctrl = CtrlManager.GetCtrl(CtrlNames.Message);
  if ctrl ~= nil then
    ctrl:Awake();
    ctrl.SetText('clickname:'..go.name);
  end]]
  
  if go.name == 'map' then
    Game.EnterPvp(function()
        this.Close();
    end);    
  elseif go.name =='inventory' then
    local ctrl = CtrlManager.GetCtrl('InventoryCtrl')
    if ctrl ~= nil then
      ctrl.Awake()
    end
  else
    Game.EnterGame(function()
        this.Close();
    end);
  end
  --logWarn("Start lua--->>"..gameObject.name);
end

--关闭事件--
function MainMenuCtrl.Close()
	panelMgr:ClosePanel('MainMenu');
  message = nil;
  gameObject = nil;
end

return MainMenuCtrl;
