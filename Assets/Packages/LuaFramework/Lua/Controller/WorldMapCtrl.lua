
local WorldMapCtrl = {};
local this = WorldMapCtrl;

local message;
local transform;
local gameObject;

--构建函数--
function WorldMapCtrl.New()
	--logWarn("LoginCtrl.New--->>");
	return this;
end

function WorldMapCtrl.Awake()
	--logWarn("LoginCtrl.Awake--->>");
	panelMgr:CreatePanel('WorldMap', this.OnCreate);
end

--启动事件--
function WorldMapCtrl.OnCreate(obj)
	gameObject = obj;
  gameObject:SetActive(true);

	message = gameObject:GetComponent('LuaBehaviour');
  message:AddClickByDir(gameObject,this.clickPVE,'PVE')
  message:AddClickByDir(gameObject,this.clickPVP,'PVP')
  message:AddClickByDir(gameObject,this.clickback,'back')
end

function WorldMapCtrl.clickback(go)
  this.Close()
end

function WorldMapCtrl.clickPVE(go)
  --logWarn('clickPVE')
  local as = WorldMapPanel.mPVE:GetComponent('AudioSource')
  as:Play()
  Game.EnterGame(function()
      this.Close();
  end);  
end

function WorldMapCtrl.clickPVP(go)
  --logWarn('clickPVP')
  local as = WorldMapPanel.mPVP:GetComponent('AudioSource')
  as:Play()
  Game.EnterPvp(function()
      this.Close();
  end);    
end

--关闭事件--
function WorldMapCtrl.Close()
	panelMgr:ClosePanel('WorldMap');
  message = nil;
  gameObject = nil;
end

return WorldMapCtrl;
