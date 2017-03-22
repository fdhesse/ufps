local transform = nil
local gameobj = nil
InventoryPanel = {  };
local this = InventoryPanel

--启动事件--
function InventoryPanel.Awake(obj)
  logWarn('InventoryPanel:Awake')
	transform = obj.transform;
  gameobj = obj
	this.InitPanel();
	--logWarn("Awake lua--->>"..gameObject.name);
end

--初始化面板--
function InventoryPanel.InitPanel()
  logWarn('InventoryPanel:InitPanel')
	this.mBtnBack = transform:FindChild("back").gameObject;
  this.mBtnAll = transform:FindChild("Panelup").gameObject.transform:FindChild("all").gameObject;
  this.mScrollView = transform:FindChild("ScrollView").gameObject
  this.mContent = this.mScrollView.transform:FindChild('Viewport').gameObject.transform:FindChild("Content").gameObject
  this.mInventoryInfoPfb = transform:FindChild("InventoryInfoPfb").gameObject
  this.mInventoryMaterialInfoPfb = transform:FindChild("InventoryMaterialInfoPfb").gameObject
end

--单击事件--
function InventoryPanel.OnDestroy()
  logWarn('InventoryPanel:OnDestroy')
end

