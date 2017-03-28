require "3rd/pblua/login_pb"
require "3rd/pbc/protobuf"

local lpeg = require "lpeg"

local json = require "cjson"
local util = require "3rd/cjson/util"

local sproto = require "3rd/sproto/sproto"
local core = require "sproto.core"
local print_r = require "3rd/sproto/print_r"

require "Common/functions"
require "Logic/LuaClass"
--require "Logic/CtrlManager"
require "Logic/ActionManager"
require "Logic/ActionInit"
require "Logic/GameData"
require "Logic/Gui"
require "Player/Player"


GameEnv = {
  AccountName = nil,
  AccountId = nil,
  NickName = nil,  
};

--管理器--
-- conflict with Proto namespace
if (Game == nil ) then Game = {}; end

local this = Game;


local game; 
local transform;
local gameObject;
local WWW = UnityEngine.WWW;

--初始化完成，发送链接服务器信息--
function Game.OnInitOK()
    AppConst.SocketPort = 5000;
   --AppConst.SocketAddress = "127.0.0.1";
    AppConst.SocketAddress = "106.14.67.25";
    networkMgr:SendConnect();

    GameData:Init();
    ShowPanel('Login');
    
    GameAPI.RegisterEvent("LoadLevel", this.LoadLevel);
    GameAPI.RegisterEvent("LoadLobby", this.LoadLobby);
    GameAPI.RegisterEvent("EndGame", this.OnEndGame);
    
    logWarn('LuaFramework InitOK--->>>');
end

function Game.LoadLevel()  
  ShowPanel('LobbyLoading');

  --todo: load level according GameApi.RequestLevel
  resMgr:LoadLevel("pvescenes", "TWD_coop_warehouse", nil, function() 
  GameAPI.ExecuteEvent("LevelLoaded");
  ClosePanel('LobbyLoading');
end);  
 
end

function Game.LoadLobby()  
  this.EnterLobby(function() 
  end)
  ClosePanel('Loading');
end

function Game.OnEndGame()  
  logWarn('OnEndGame ' .. tostring(GameAPI.Win));
  --local ctrl = CtrlManager.GetCtrl(CtrlNames.Combat);
  --ctrl.ShowResult(true, GameAPI.Win);
  --CombatPanelViewModel.Win = GameAPI.Win;
  CombatPanelViewModel.SetWin(GameAPI.Win);
  ShowPanel('Combat');
end

function Game.EnterGame(onEnter)
  --resMgr:LoadLevel("scenes", "Camp", nil, function() 
  this.PvpMode = false;  
  resMgr:LoadLevel("pvescenes", "TWD_coop_warehouse", nil, function() 
      --resMgr:LoadPrefab("player", {"Player Camera", "Player Remy"}, function(objs)            
         --local camera = UnityEngine.Object.Instantiate(objs[0]); 
         --local player = UnityEngine.Object.Instantiate(objs[1]); 
    
          PreloadPanel('Combat');
    
          onEnter();           
         --resMgr:UnloadAssetBundle("player", false);
         resMgr:UnloadAssetBundle("pvescenes", false);
         
         --MessageCtrl.Close();
        
         Util.ClearMemory();
      --end)
  end);  

end

function Game.EnterPve(matchId, teamId, roomUrl, onEnter)
  --resMgr:LoadLevel("pvescenes", "TWD_coop_warehouse", nil, function() 
      
    local info = MultiplayerInfo.New();
    local index = string.find(roomUrl, ':');
    local ip = string.sub(roomUrl, 1, index-1);
    local port = tonumber(string.sub(roomUrl, index+1, #roomUrl));
      
    info.ip = ip;
    info.port = port;
    info.matchId = matchId;
    --info.teamNum = teamNum;
    info.teamId = teamId;
    --print(ip);
    
    GameAPI.StartMultiplayer(info);      
    PreloadPanel('Combat');
     
    onEnter();    
    --resMgr:UnloadAssetBundle("pvpscenes", false);
  --end);  
end

--[[
function Game.EnterPvp(onEnter)
  this.PvpMode = true;
  --resMgr:LoadLevel("pvescenes", "TWD_coop_warehouse", nil, function() 
      
    local info = MultiplayerInfo.New();
    local index = string.find(roomUrl, ':');
    local ip = string.sub(roomUrl, 1, index-1);
    local port = tonumber(string.sub(roomUrl, index+1, #roomUrl));
      
    info.ip = ip;
    info.port = port;
    info.matchId = matchId;
    print(ip);    
    
    GameAPI.StartMultiplayer(info);
    PreloadPanel('Combat');
     
    onEnter();
    
    --resMgr:UnloadAssetBundle("pvpscenes", false);
  --end);  
end
--]]

function Game.EnterLobby(onEnter)
  resMgr:LoadLevel("lobbyscenes", "lobby", nil, function() 
      
    ShowPanel('MainMenu');
    ClosePanel('Loading');
    onEnter();
    
    resMgr:UnloadAssetBundle("lobbyscenes", false);
  end);  
end

--销毁--
function Game.OnDestroy()
	--logWarn('OnDestroy--->>>');
end
