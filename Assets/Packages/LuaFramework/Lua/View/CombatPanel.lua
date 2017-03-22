local transform;
local gameObject;

CombatPanel = {};
local this = CombatPanel;

--启动事件--
function CombatPanel.Awake(obj)
	gameObject = obj;
	transform = obj.transform;

	this.InitPanel();
	logWarn("Awake lua--->>"..gameObject.name);
end

--初始化面板--
function CombatPanel.InitPanel()
	this.btnExitCombat = transform:FindChild("ExitCombat").gameObject;  
	this.btnClose = transform:FindChild("Result/Close").gameObject;  
	this.btnAgain = transform:FindChild("Result/Again").gameObject;  
	this.btnQuit = transform:FindChild("Result/Quit").gameObject;  
  this.windowResult = transform:FindChild("Result").gameObject;  
  this.tipWin = transform:FindChild("Result/Win").gameObject;  
  this.tipLose = transform:FindChild("Result/Lose").gameObject;  
  this.windowResult:SetActive(false);
  this.tipWin:SetActive(false);
  this.tipLose:SetActive(false);
end

--单击事件--
function CombatPanel.OnDestroy()
	--logWarn("OnDestroy---->>>");
  transform = nil;
  gameObject = nil;
  this.btnClose = nil;
end

