local transform = nil
InventoryDetailInfoPanel = {  };
local this = InventoryDetailInfoPanel

--启动事件--
function InventoryDetailInfoPanel.Awake(obj)
  logWarn('InventoryDetailInfoPanel:Awake')
	transform = obj.transform;

	this.InitPanel();
	--logWarn("Awake lua--->>"..gameObject.name);
end

--初始化面板--
function InventoryDetailInfoPanel.InitPanel()
  logWarn('InventoryDetailInfoPanel:InitPanel')
	this.mBtnBack = transform:FindChild("bg").gameObject.transform:FindChild("back").gameObject;
  this.mScrollView = transform:FindChild('bg/ScrollView').gameObject
  this.mWeaponDetailInfoPrefab = transform:FindChild('WeaponDetailInfoPrefab').gameObject
  this.mContent = transform:FindChild('bg/ScrollView/Viewport/Content').gameObject
  this.mModuleList = transform:FindChild('modulelist').gameObject
  this.mBg = transform:FindChild('bg').gameObject
  this.mWeaponmodulePfb = transform:FindChild('weaponmodulePfb').gameObject
  this.mModuleListContent = transform:FindChild('modulelist/ScrollView/Viewport/Content').gameObject
  this.mModuleBtnBack = transform:FindChild('modulelist/moduleback').gameObject
  this.mInventoryInfoPfb = transform:FindChild('bg/InventoryInfoPfb').gameObject
end

--单击事件--
function InventoryDetailInfoPanel.OnDestroy()
  logWarn('InventoryDetailInfoPanel:OnDestroy')
end

