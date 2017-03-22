
local CombatCtrl = {};
local this = CombatCtrl;

local message;
local transform;
local gameObject;

--构建函数--
function CombatCtrl.New()
	--logWarn("CombatCtrl.New--->>");
	return this;
end

function CombatCtrl.Awake()
	--logWarn("LoginCtrl.Awake--->>");
	panelMgr:CreatePanel('Combat', this.OnCreate);
end

--启动事件--
function CombatCtrl.OnCreate(obj)
	gameObject = obj;
  --gameObject:SetActive(true);

	message = gameObject:GetComponent('LuaBehaviour');
	message:AddClick(CombatPanel.btnExitCombat, this.OnExitCombat);
	message:AddClick(CombatPanel.btnClose, function() this.ShowResult(false) end);
	message:AddClick(CombatPanel.btnAgain, this.OnRetryCombat);
	message:AddClick(CombatPanel.btnQuit, this.OnExitCombat);
end

function CombatCtrl.OnExitCombat(go)
  --logWarn("OnExitCombat");
  --GameAPI.ExecuteBoolEvent("OnGameOver", false);
  --Game.EnterLobby(function()
      --this.Close();
  --end);

  this.Close();
  GameAPI.ExecuteEvent("ExitCombat");
  GameAPI.ExecuteEvent("LoadLobby");
end

function CombatCtrl.OnRetryCombat(go)
  this.Close();
  GameAPI.ExecuteEvent("ExitCombat");
  GameAPI.ExecuteEvent("LoadLobby");
end

function CombatCtrl.ShowResult(show, win)
  --logWarn("ShowResult");
  CombatPanel.windowResult:SetActive(show)  
  if win then
    CombatPanel.tipWin:SetActive(true);
  else
    CombatPanel.tipLose:SetActive(true);    
  end
end

--关闭事件--
function CombatCtrl.Close()
	panelMgr:ClosePanel('Combat');
  message = nil;
  gameObject = nil;
end

return CombatCtrl;
