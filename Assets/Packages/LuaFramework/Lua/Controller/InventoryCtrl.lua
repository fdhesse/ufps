
local InventoryCtrl = {};
local this = InventoryCtrl;

local message;
local transform;
local gameObject;
local InventoryClassTable = {};
local WeaponRarityImgTable = {};
local weaponData;

--构建函数--
function InventoryCtrl.New()
	--logWarn("LoginCtrl.New--->>");
	return this;
end

function InventoryCtrl.Awake()
	--logWarn("LoginCtrl.Awake--->>");
	panelMgr:CreatePanel('Inventory', this.OnCreate);
end

--启动事件--
function InventoryCtrl.OnCreate(obj)
	gameObject = obj;
  gameObject:SetActive(true);

	message = gameObject:GetComponent('LuaBehaviour');
	--message:AddClick(LoginPanel.btnClose, this.OnClick);
	--message:AddClick(MessagePanel.Button, this.OnClick);
  message:AddClick(InventoryPanel.mBtnBack,this.OnClick)
  message:AddClick(InventoryPanel.mBtnAll,this.OnClick)
  message:AddClick(InventoryPanel.mBtnScrollView,this.OnClick)
  message:AddClickByDir(gameObject,this.clickAssault,'Panelup/assault')
  message:AddClickByDir(gameObject,this.clickDefense,'Panelup/defense')
  message:AddClickByDir(gameObject,this.clickAssist,'Panelup/assist')
  message:AddClickByDir(gameObject,this.clickWeapon,'Paneldown/weapon')
  message:AddClickByDir(gameObject,this.clickGear,'Paneldown/gear')
  message:AddClickByDir(gameObject,this.clickProps,'Paneldown/props')
  message:AddClickByDir(gameObject,this.clickMaterial,'Paneldown/material')
	--logWarn("Start lua--->>"..gameObject.name);
  
  local data = GameData:GetUserEntity(Game.KingsMan.MessagePack.EntityType.ET_TINVENTORYSET)
  print(#data.TInventoryEntityList)
  print(#data.InventoryClassToUUIDList)
  for i = 1,#data.InventoryClassToUUIDList do 
    local value = data.InventoryClassToUUIDList[i]
    InventoryClassTable[value.Key] = value.UUIDListValue
  end
  
  this.CreateScrollViewByData(data)
end

function InventoryCtrl.clickAssault(go)
  logWarn('clickAssault')
end

function InventoryCtrl.clickDefense(go)
  logWarn('clickDefense')
end

function InventoryCtrl.clickAssist(go)
  logWarn('clickAssist')
end

function InventoryCtrl.clickWeapon(go)
  logWarn('clickWeapon')
  FindChild(gameObject,'Panelup'):SetActive(true)
  FindChild(gameObject,'PanelupMaterial'):SetActive(false)
  
  local data = GameData:GetUserEntity(Game.KingsMan.MessagePack.EntityType.ET_TINVENTORYSET)
  print(#data.TInventoryEntityList)
  print(#data.InventoryClassToUUIDList)
  for i = 1,#data.InventoryClassToUUIDList do 
    local value = data.InventoryClassToUUIDList[i]
    InventoryClassTable[value.Key] = value.UUIDListValue
  end
  
  this.ClearScrollViewData()
  this.CreateScrollViewByData(data)
end

function InventoryCtrl.clickGear(go)
  logWarn('clickGear')
end

function InventoryCtrl.clickProps(go)
  logWarn('clickProps')
end

function InventoryCtrl.clickMaterial(go)
  logWarn('clickMaterial')
  FindChild(gameObject,'Panelup'):SetActive(false)
  FindChild(gameObject,'PanelupMaterial'):SetActive(true)
  
  local data = GameData:GetUserEntity(Game.KingsMan.MessagePack.EntityType.ET_TINVENTORYSET)
  print(#data.TInventoryEntityList)
  print(#data.InventoryClassToUUIDList)
  for i = 1,#data.InventoryClassToUUIDList do 
    local value = data.InventoryClassToUUIDList[i]
    InventoryClassTable[value.Key] = value.UUIDListValue
  end
  
  this.ClearScrollViewData()
  this.CreateMaterialScrollViewByData(data)
end

function InventoryCtrl.CreateMaterialScrollViewByData(data)
  weaponData = InventoryClassTable['EIC_WeaponBlueprint']
  if weaponData == nil then
    return
  end
  
  local len = #weaponData.UUID
  print(len)
  for i = 1,len do
    local weaponUID = weaponData.UUID[i]
    local weaponUIDData = data.TInventoryEntityList[weaponUID]
    local inventoryid = weaponUIDData.InventoryID
    local cfgdata = InventoryDefTableDB:FindData(inventoryid)
    local rarity = weaponUIDData.Rarity
    print(rarity)
    
    local go = GameObject.Instantiate(InventoryPanel.mInventoryMaterialInfoPfb)
    go.name = 'weaponMaterial_'..i
    go.transform.parent = InventoryPanel.mContent.transform
    go.transform.localScale = Vector3.one
    go.transform.localPosition = Vector3.zero
    message:AddClick(go,this.OnWeaponMaterialClick)
    FindChild(go,'rarityPanel/'..rarity):SetActive(true)
    FindChild(go,'namePanel/'..cfgdata.Image):SetActive(true)
    
    FindChild(go,'nametext'):GetComponent('Text').text = cfgdata.Name
    FindChild(go,'levelvalue'):GetComponent('Text').text = 'X'..weaponUIDData.Count
    
    go:SetActive(true)
  end
  local itemWidth = InventoryPanel.mInventoryInfoPfb:GetComponent('RectTransform').rectWidth
  local contentWidth = InventoryPanel.mContent:GetComponent('RectTransform').rectWidth
  local contentHeight = InventoryPanel.mContent:GetComponent('RectTransform').rectHeight
  logWarn('itemWidth:'..itemWidth)
  if contentWidth < itemWidth * math.ceil(len/2) then
    InventoryPanel.mContent:GetComponent('RectTransform').sizeDelta = Vector2.New(itemWidth * math.ceil(len/2) ,contentHeight)
  end
end

function InventoryCtrl.ClearScrollViewData()
  for i = 1,InventoryPanel.mContent.transform.childCount do
    local go = InventoryPanel.mContent.transform:GetChild(i-1).gameObject
    destroy(go)
  end
end

--每个标签创建一个scrollview，切换时显示或隐藏，不用每次都创建和销毁
function InventoryCtrl.CreateScrollViewByData(data)
  weaponData = InventoryClassTable['EIC_Weapon']
  if weaponData == nil then
    return
  end
  
  local len = #weaponData.UUID
  print(len)
  for i = 1,len do
    local weaponUID = weaponData.UUID[i]
    local weaponUIDData = data.TInventoryEntityList[weaponUID]
    local inventoryid = weaponUIDData.InventoryID
    local cfgdata = InventoryDefTableDB:FindData(inventoryid)
    local rarity = weaponUIDData.Rarity
    print(rarity)
    
    local childuuid = weaponUIDData.ChildUUID
    local prokeyvalue = weaponUIDData.TProKeyValueList
    print(#prokeyvalue)
    
    local go = GameObject.Instantiate(InventoryPanel.mInventoryInfoPfb)
    go.name = 'weapon_'..i
    go.transform.parent = InventoryPanel.mContent.transform
    go.transform.localScale = Vector3.one
    go.transform.localPosition = Vector3.zero
    message:AddClick(go,this.OnWeaponClick)
    local isequip = FindChild(go,'isequipimage')
    if weaponUIDData.ParentUUID == 0 then
      isequip:SetActive(false)
    end
    FindChild(go,'rarityPanel/'..rarity):SetActive(true)
    FindChild(go,'namePanel/'..cfgdata.Name):SetActive(true)
    local dpsvalue = FindChild(go,'dpsvalue')
    dpsvalue:GetComponent('Text').text = 2000 + weaponUIDData.Count * 20 + weaponUIDData.Count * weaponUIDData.Count
    
    FindChild(go,'nametext'):GetComponent('Text').text = cfgdata.Name
    FindChild(go,'levelvalue'):GetComponent('Text').text = weaponUIDData.Count
    
    go:SetActive(true)
  end
  local itemWidth = InventoryPanel.mInventoryInfoPfb:GetComponent('RectTransform').rectWidth
  local contentWidth = InventoryPanel.mContent:GetComponent('RectTransform').rectWidth
  local contentHeight = InventoryPanel.mContent:GetComponent('RectTransform').rectHeight
  logWarn('itemWidth:'..itemWidth)
  if contentWidth < itemWidth * math.ceil(len/2) then
    InventoryPanel.mContent:GetComponent('RectTransform').sizeDelta = Vector2.New(itemWidth * math.ceil(len/2) ,contentHeight)
  end
end

function InventoryCtrl.OnWeaponClick(go)
  logWarn('clickname:'..go.name)
  local arr = split(go.name,'_')
  local index = arr[2]
  local weaponUID = weaponData.UUID[tonumber(index)]
  local data = GameData:GetUserEntity(Game.KingsMan.MessagePack.EntityType.ET_TINVENTORYSET)
  local modelData = data.TInventoryEntityList[weaponUID]
  local inventoryid = modelData.InventoryID
  local cfgdata = InventoryDefTableDB:FindData(inventoryid)
  local ctrl = CtrlManager.GetCtrl('InventoryDetailInfoCtrl')
  if ctrl ~= nil then
    ctrl.Awake()
    ctrl.InitWeaponPfd(modelData,cfgdata)
  end
end

--单击事件--
function InventoryCtrl.OnClick(go)
  logWarn('clickname:'..go.name)
  
  if go.name == 'back' then
    this.Close()
  elseif go.name =='all' then
    
  elseif go.name =='name1' then
    local ctrl = CtrlManager.GetCtrl('InventoryDetailInfoCtrl')
    if ctrl ~= nil then
      ctrl.Awake()
    end
  elseif go.name == 'Button' then
    local ctrl = CtrlManager.GetCtrl('InventoryDetailInfoCtrl')
    if ctrl ~= nil then
      ctrl.Awake()
    end
  else
    --Game.EnterGame(function()
    --    this.Close();
   -- end);
  end
  --logWarn("Start lua--->>"..gameObject.name);
end

--关闭事件--
function InventoryCtrl.Close()
	panelMgr:ClosePanel('Inventory');
  message = nil;
  gameObject = nil;
end

return InventoryCtrl;
