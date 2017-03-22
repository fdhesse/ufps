
local InventoryDetailInfoCtrl = {};
local this = InventoryDetailInfoCtrl;

local message;
local transform;
local gameObject;
local currModuleSelect = 1;
local currModuleListSelect = 0;

--构建函数--
function InventoryDetailInfoCtrl.New()
	--logWarn("LoginCtrl.New--->>");
	return this;
end

function InventoryDetailInfoCtrl.Awake()
	--logWarn("LoginCtrl.Awake--->>");
	panelMgr:CreatePanel('InventoryDetailInfo', this.OnCreate);
end

--启动事件--
function InventoryDetailInfoCtrl.OnCreate(obj)
	gameObject = obj;
  gameObject:SetActive(true);

	message = gameObject:GetComponent('LuaBehaviour');
	--message:AddClick(LoginPanel.btnClose, this.OnClick);
	--message:AddClick(MessagePanel.Button, this.OnClick);
  message:AddClick(InventoryDetailInfoPanel.mBtnBack,this.OnClick)
  message:AddClick(InventoryDetailInfoPanel.mModuleBtnBack,this.OnClick)
  message:AddClickByDir(gameObject,this.clickSell,'bg/sell')
  message:AddClickByDir(gameObject,this.clickDecompose,'bg/decompose')
	--logWarn("Start lua--->>"..gameObject.name);
  this.CreateScrollViewByData(nil)
  --this.InitWeaponPfd(nil)
end

function InventoryDetailInfoCtrl.InitWeaponPfd(modelData,cfgData)
  local go = InventoryDetailInfoPanel.mInventoryInfoPfb
  --local weaponIconGo = go.transform:FindChild('nameimage').gameObject
  local moduleGo0 = go.transform:FindChild('module0').gameObject
  moduleGo0:SetActive(false)
  local moduleGo1 = go.transform:FindChild('module1').gameObject
  moduleGo1:SetActive(false)
  local moduleGo2 = go.transform:FindChild('module2').gameObject
  moduleGo2:SetActive(false)
  
  local isequipGo = FindChild(go,'isequipimage')
  isequipGo:SetActive(false)
  
  --local rarityimageGo = FindChild(go,'rarityimage')
  --local rarityimageGoImage = rarityimageGo:GetComponent('Image')
  --rarityimageGoImage.sprite = 'quality_1'
  local rarity = modelData.Rarity
  local isequip = FindChild(go,'isequipimage')
  if modelData.ParentUUID == 0 then
   isequip:SetActive(false)
  end
  print(rarity)
  FindChild(go,'rarityPanel/'..rarity):SetActive(true)  
  FindChild(go,'namePanel/'..cfgData.Name):SetActive(true)
  local dpsvalue = FindChild(go,'dpsvalue')
  dpsvalue:GetComponent('Text').text = 2000 + modelData.Count * 20 + modelData.Count * modelData.Count
    
  FindChild(go,'nametext'):GetComponent('Text').text = cfgData.Name
  FindChild(go,'levelvalue'):GetComponent('Text').text = modelData.Count
  local prokeyvalue = modelData.TProKeyValueList
  for i = 1,#prokeyvalue do
    local keyvalue = prokeyvalue[i]
    local proname = 'bg/ScrollView/Viewport/Content/WeaponDetailInfoPrefab1/'..keyvalue.Key..'Panel/provalueText'
    print(proname)
    FindChild(gameObject,proname):GetComponent('Text').text = keyvalue.Value
  end
  
  FindChild(gameObject,'bg/leftbg/Text'):GetComponent('Text').text = cfgData.Description
end

function InventoryDetailInfoCtrl.clickSell(go)
  logWarn('clickSell')
end

function InventoryDetailInfoCtrl.clickDecompose(go)
  logWarn('clickDecompose')
end

function InventoryDetailInfoCtrl.CreateScrollViewByData(data)
  local go = GameObject.Instantiate(InventoryDetailInfoPanel.mWeaponDetailInfoPrefab)
  go.name = 'WeaponDetailInfoPrefab1'
  go.transform.parent = InventoryDetailInfoPanel.mContent.transform
  go.transform.localScale = Vector3.one
  go.transform.localPosition = Vector3.zero
  go:SetActive(true)
  
  local module1 = go.transform:FindChild('modulePanel1/moduleBtn1').gameObject
  message:AddClick(module1,this.ClickModule)
  local module2 = go.transform:FindChild('modulePanel2/moduleBtn2').gameObject
  message:AddClick(module2,this.ClickModule)
  local module3 = go.transform:FindChild('modulePanel3/moduleBtn3').gameObject
  message:AddClick(module3,this.ClickModule)
  
  local itemWidth = InventoryDetailInfoPanel.mWeaponDetailInfoPrefab:GetComponent('RectTransform').rectWidth
  local itemHeight = InventoryDetailInfoPanel.mWeaponDetailInfoPrefab:GetComponent('RectTransform').rectHeight
  InventoryDetailInfoPanel.mContent:GetComponent('RectTransform').sizeDelta = Vector2.New(itemWidth,itemHeight)
end

function InventoryDetailInfoCtrl.ClickModule(go)
  logWarn('clickname:'..go.name)
  this.SetMode(2)
  if go.name == 'moduleBtn1' then
    this.CreateModuleList(go,16)
  elseif go.name == 'moduleBtn2' then
    this.CreateModuleList(go,10)
  else
    
  end
end

function InventoryDetailInfoCtrl.CreateModuleList(go,len)
  --InventoryDetailInfoPanel.mModuleListContent.transform:DetachChildren()
  for i = 1,len do
    local addgo = GameObject.Instantiate(InventoryDetailInfoPanel.mWeaponmodulePfb)
    addgo.name = 'modulelist_'..i
    addgo.transform.parent = InventoryDetailInfoPanel.mModuleListContent.transform
    addgo.transform.localScale = Vector3.one
    addgo.transform.localPosition = Vector3.zero
    addgo:SetActive(true)
    message:AddClick(addgo,this.ClickModuleOp)
  end
  local currgo = InventoryDetailInfoPanel.mModuleListContent.transform:GetChild(currModuleListSelect).gameObject
  local selectbg = currgo.transform:FindChild('selectbg').gameObject
  selectbg:SetActive(true)
  local itemHeight = InventoryDetailInfoPanel.mWeaponmodulePfb:GetComponent('RectTransform').rectHeight
  local contentWidth = InventoryDetailInfoPanel.mModuleListContent:GetComponent('RectTransform').rectWidth
  local contentHeight = InventoryDetailInfoPanel.mModuleListContent:GetComponent('RectTransform').rectHeight
  if contentHeight < itemHeight * math.ceil(len/2) then
    InventoryDetailInfoPanel.mModuleListContent:GetComponent('RectTransform').sizeDelta = Vector2.New(contentWidth ,itemHeight * math.ceil(len/2))
  end
end

function InventoryDetailInfoCtrl.ClickModuleOp(go)
  logWarn('clickname:'..go.name)
  local arr = split(go.name,'_')
  local lastgo = InventoryDetailInfoPanel.mModuleListContent.transform:GetChild(currModuleListSelect).gameObject
  local selectbg = lastgo.transform:FindChild('selectbg').gameObject
  selectbg:SetActive(false)
  
  local currgo = InventoryDetailInfoPanel.mModuleListContent.transform:GetChild(arr[2]-1).gameObject
  selectbg = currgo.transform:FindChild('selectbg').gameObject
  selectbg:SetActive(true)
  currModuleListSelect = arr[2]-1
end

function InventoryDetailInfoCtrl.SetMode(mode)
  if mode == 1 then
    InventoryDetailInfoPanel.mBg:SetActive(true)
    InventoryDetailInfoPanel.mModuleList:SetActive(false)
    local childCount = InventoryDetailInfoPanel.mModuleListContent.transform.childCount
    logWarn('childCount:'..childCount)
    for i = childCount,1, -1 do
      local childgo = InventoryDetailInfoPanel.mModuleListContent.transform:GetChild(i-1).gameObject
      message:RemoveClick(childgo)
      GameObject.Destroy(childgo)
    end
    logWarn('childend:'..InventoryDetailInfoPanel.mModuleListContent.transform.childCount)
  else
    InventoryDetailInfoPanel.mBg:SetActive(false)
    InventoryDetailInfoPanel.mModuleList:SetActive(true)
  end
end

--单击事件--
function InventoryDetailInfoCtrl.OnClick(go)
  logWarn('clickname:'..go.name)
  
  if go.name == 'back' then
    this.Close()
  elseif go.name == 'moduleback' then
    this.SetMode(1)
  else
    Game.EnterGame(function()
        this.Close();
    end);
  end
  --logWarn("Start lua--->>"..gameObject.name);
end

--关闭事件--
function InventoryDetailInfoCtrl.Close()
	panelMgr:ClosePanel('InventoryDetailInfo');
  message = nil;
  gameObject = nil;
end

return InventoryDetailInfoCtrl;
