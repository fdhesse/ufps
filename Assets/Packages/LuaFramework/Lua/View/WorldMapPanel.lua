local transform = nil
WorldMapPanel = {  };
local this = WorldMapPanel

--启动事件--
function WorldMapPanel.Awake(obj)
  logWarn('WorldMapPanel:Awake')
	transform = obj.transform;

	this.InitPanel();
	--logWarn("Awake lua--->>"..gameObject.name);
end

--初始化面板--
function WorldMapPanel.InitPanel()
  logWarn('WorldMapPanel:InitPanel')
	this.mPVE = transform:FindChild("PVE").gameObject
  this.mPVP = transform:FindChild("PVP").gameObject
end

--单击事件--
function WorldMapPanel.OnDestroy()
  logWarn('WorldMapPanel:OnDestroy')
end

