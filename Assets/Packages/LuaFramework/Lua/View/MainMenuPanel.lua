local transform = nil
MainMenuPanel = {  };
local this = MainMenuPanel

--启动事件--
function MainMenuPanel.Awake(obj)
  logWarn('MainMenuPanel:Awake')
	transform = obj.transform;

	this.InitPanel();
	--logWarn("Awake lua--->>"..gameObject.name);
end

--初始化面板--
function MainMenuPanel.InitPanel()
  logWarn('MainMenuPanel:InitPanel')
	this.mBtnMap = transform:FindChild("map").gameObject;
  this.mBtnLiveEvent = transform:FindChild('liveevent').gameObject
  this.mBtnInventory = transform:FindChild('inventory').gameObject
end

--单击事件--
function MainMenuPanel.OnDestroy()
  logWarn('MainMenuPanel:OnDestroy')
end

