--require "Common/define"

CtrlNames = {
	Prompt = "PromptCtrl",
	Message = "MessageCtrl",
  Login = "LoginCtrl",
  Main = "MainCtrl",
  MainMenu = "MainMenuCtrl",
  Inventory = "InventoryCtrl",
  InventoryDetailInfo = "InventoryDetailInfoCtrl",
  WorldMap = "WorldMapCtrl",
  Combat = "CombatCtrl",
}

PanelNames = {
	"PromptPanel",	
	"MessagePanel",
  "LoginPanel",
  "MainPanel",
  "MainMenuPanel",
  "InventoryPanel",
  "InventoryDetailInfoPanel",
  "WorldMapPanel",
  "CombatPanel",
}

--require "Controller/LoginCtrl";

CtrlManager = {};
local this = CtrlManager;
local ctrlList = {};	--控制器列表--

function CtrlManager.Init()
	logWarn("CtrlManager.Init----->>>");
	for i = 1, #PanelNames do
		require ("View/"..tostring(PanelNames[i]))
	end  
	for k, v in pairs(CtrlNames) do
    --logWarn(k .. "_" .. v);
		local ctrl = require ("Controller/".. v)
    ctrlList[v] = ctrl.New()
	end    
	--ctrlList[CtrlNames.Prompt] = PromptCtrl.New();
	--ctrlList[CtrlNames.Message] = MessageCtrl.New();
  --ctrlList[CtrlNames.Login] = LoginCtrl.New();
	return this;
end

--添加控制器--
function CtrlManager.AddCtrl(ctrlName, ctrlObj)
	ctrlList[ctrlName] = ctrlObj;
end

--获取控制器--
function CtrlManager.GetCtrl(ctrlName)
	return ctrlList[ctrlName];
end

--移除控制器--
function CtrlManager.RemoveCtrl(ctrlName)
	ctrlList[ctrlName] = nil;
end

--关闭控制器--
function CtrlManager.Close()
	logWarn('CtrlManager.Close---->>>');
end