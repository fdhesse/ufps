
LobbyModel = {}
local this = LobbyModel;
  
local matchId = 0;
local sequence = -1;

--[[
function LobbyModel.Connect()
    Event.AddListener(Protocal.Connect, this.OnConnect); 

    AppConst.SocketPort = 7000;
    AppConst.SocketAddress = "127.0.0.1";
    --ActionMgr.session = responsePack.Session;
    networkMgr:SendConnect();           
end

function LobbyModel.OnConnect()
  Event.RemoveListener(Protocal.Connect, this.OnConnect); 
  this.CreateRoom();
end

function LobbyModel.CreateRoom()
   GameData:CallServer(Game.KingsMan.MessagePack.ActionEnum.RoomCreate, 7000, 
    function(requestPack)
      requestPack.UserId = 12345;
      requestPack.SlaveUserId = 0;
      requestPack.ServerId = 0;
      requestPack.IsFriendlyBattle = false;
      requestPack.TeamId = 0;
      requestPack.RobotList = {};
      requestPack.PlayerNums = 2;      
      print('CreateRoom request');
    end,
    function(errorCode, responsePack)
      print('CreateRoom response');
      print(errorCode);
      end
    end
  );    
end

function LobbyModel.JoinRoom()
  
  
end
--]]
  
function LobbyModel.StartMatch(missionId, isCreate)
   GameData:CallServer(Game.KingsMan.MessagePack.ActionEnum.MatchStart, 9850, 
    function(requestPack)
      local inf = Game.KingsMan.MessagePack.RTMatchInf;
      inf.UserId = GameEnv.AccountId;  
      inf.ExpLevel = 0;  
      inf.RTCupNum = 0;  
      inf.NickName = GameEnv.NickName;  
      inf.GuildId = 0;  
      inf.GuildName = "";  
      inf.MatchCupBaseNum = 0;  
      inf.RateOfWinning = 0;  
      inf.HeadIcon = "";  
      --inf.TeamId = 0;  
      --inf.CardStrength = 0;  
      --inf.AreaId = 0;  
      inf.GuildIcon = 0;  
      
      requestPack.MatchInf = inf;      
      requestPack.MissionId = missionId;
      requestPack.IsCreate = isCreate;
      print('StartMatch request');
    end,
    function(errorCode, responsePack)
      print('StartMatch response');

      if errorCode == 0 then
        --LobbyViewModel.Users = responsePack.resultEx.MatchedUserList;
        matchId = responsePack.resultEx.TeamId;        
        
        if responsePack.resultEx.MatchedUserList then
          LobbyViewModel.UpdateMatch(responsePack.resultEx);
          LobbyViewModel.SwitchPanel(4);  
        end
        
        LobbyModel.TimeUpdateMatch(missionId);
      else
        MessageBox('StartMatch ' .. errorCode);
      end
    end
  );  
  
end

local timer;
function LobbyModel.TimeUpdateMatch(missionId)
  if timer then timer:Stop() end
  
  --if (timer == nil) then
     timer = Timer.New(
      function()
        --print('LobbyModel')     
        this.UpdateMatch(missionId);  
        LobbyViewModel.Time =  LobbyViewModel.Time-1;
        LobbyViewModel.Pointer = LobbyViewModel.Pointer-6;
        LobbyViewModel.UpdateViewModel();
      end, 
      1, 
      60,
      false,
      function()
        --print('stop timer')
        --this.CancelMatch(missionId);
      end
      )
  --end

  timer:Start();
end

function LobbyModel.UpdateMatch(missionId)
   GameData:CallServer(Game.KingsMan.MessagePack.ActionEnum.MatchUpdate, 9850, 
    function(requestPack)
      requestPack.UserId = GameEnv.AccountId;      
      requestPack.MissionId = missionId;
      print('UpdateMatch request');
    end,
    function(errorCode, responsePack)
      print('UpdateMatch response');
      print(errorCode)
      
      if errorCode == 0 then
        --timer:Stop();
        local result = responsePack.resultEx;
        --if (sequence ~= result.Sequence) then
          sequence = result.Sequence;
          matchId = result.TeamId;
          
          LobbyViewModel.UpdateMatch(result);
          --LobbyViewModel.UpdateViewModel();
          LobbyViewModel.SwitchPanel(4);  
          print(responsePack.resultEx.State);
          if responsePack.resultEx.State == 1 then
            local teamId = 0;
            local len = #result.MatchedUserList;
            for i=1, len, 1 do
              if (result.MatchedUserList[i].UserId == GameEnv.AccountId) then 
                teamId = result.MatchedUserList[i].TeamId
                break
              end            
            end
      
            timer:Stop();
            Game.EnterPve(matchId, teamId, sequence, result.RoomServerUrl, function()
              LobbyViewModel.SwitchPanel(1);
              ClosePanel('MainMenu');              
              ClosePanel('LobbySceneUI');
             
            end);  
          
        --end
      end
      elseif errorCode == Game.KingsMan.MessagePack.ErrorEnum.MatchFailedMatching then
        --skip this error
      else
        timer:Stop();
        MessageBox('UpdateMatch ' .. errorCode);        
      end
    end
  );    
end

function LobbyModel.CancelMatch(missionId)
   if timer then timer:Stop(); end
   
   GameData:CallServer(Game.KingsMan.MessagePack.ActionEnum.MatchCancel, 9850, 
    function(requestPack)
      requestPack.UserId = GameEnv.AccountId;  
      requestPack.MissionId = missionId;
      requestPack.MatchId = matchId;

      print('CancelMatch request');
    end,
    function(errorCode, responsePack)
      print('CancelMatch response');
      
      LobbyViewModel.SwitchPanel(1); 
      if errorCode == 0 then
        --LobbyViewModel.SwitchPanel(1);    
      elseif matchId ~= 0 then
        --only display error when we got the matchId
        MessageBox('CancelMatch ' .. errorCode);        
      end
    end
  );    
end

function LobbyModel.BeginMatch(missionId)
   --if timer then timer:Stop(); end
   
   GameData:CallServer(Game.KingsMan.MessagePack.ActionEnum.MatchBegin, 9850, 
    function(requestPack)
      requestPack.UserId = GameEnv.AccountId;  
      requestPack.MissionId = missionId;
      requestPack.MatchId = matchId;

      print('BeginMatch request');
    end,
    function(errorCode, responsePack)
      print('BeginMatch response');
      
      --LobbyViewModel.SwitchPanel(1); 
      if errorCode == 0 then
        --LobbyViewModel.SwitchPanel(1);    
      else
        MessageBox('BeginMatch ' .. errorCode);        
      end
    end
  );    
end

function LobbyModel.HeartBeat()
   GameData:CallServer(Game.KingsMan.MessagePack.ActionEnum.MatchHeartBeat, 9850, 
    function(requestPack)
      requestPack.UserId = GameEnv.AccountId;      
      requestPack.MissionId = LobbyViewModel.MissionId;
      requestPack.MatchId = matchId;
      print('HeartBeat request');
    end,
    function(errorCode, responsePack)
      print('HeartBeat response');    
    end
  );    
end

function LobbyModel.CheckTeam()
   ShowPanel('LobbySceneUI');
   GameData:CallServer(Game.KingsMan.MessagePack.ActionEnum.MatchQuery, 9850, 
    function(requestPack)
      requestPack.UserId = GameEnv.AccountId;      
      requestPack.MissionId = LobbyViewModel.MissionId;
      print('MatchQuery request');
    end,
    function(errorCode, responsePack)
      print('MatchQuery response');    
      if (errorCode == 0) then
        matchId = responsePack.resultEx.TeamId;        
        
        if responsePack.resultEx.MatchedUserList then
          LobbyViewModel.UpdateMatch(responsePack.resultEx);
          LobbyViewModel.SwitchPanel(4);  
        end
        
        this.TimeUpdateMatch(LobbyViewModel.MissionId);
      end
    end
  );  
end

function LobbyModel.EndMatch()
   GameData:CallServer(Game.KingsMan.MessagePack.ActionEnum.MatchEnd, 9850, 
    function(requestPack)
      requestPack.UserId = GameEnv.AccountId;      
      requestPack.MissionId = LobbyViewModel.MissionId;
      requestPack.MatchId = sequence;
      print('EndMatch request');
    end,
    function(errorCode, responsePack)
      print('EndMatch response');    
      if (errorCode == 0) then
      end
    end
  );  
end


function LobbyModel.TestEntityChange(entity)
  print('TestEntityChange ' .. entity.HeadIcon); 
end

GameData:RegisterEntityChangedCB(Game.KingsMan.MessagePack.EntityType.ET_PLAYER, LobbyModel.TestEntityChange);

local i = 1;

function LobbyModel.Test()
  local ent = GameData:GetUserEntity(Game.KingsMan.MessagePack.EntityType.ET_PLAYER);
  print(ent.HeadIcon);
  print(ent.ID);
  
  GameData:CallServer(Game.KingsMan.MessagePack.ActionEnum.ChangeHeadIcon, 9100, 
    function(requestPack)
      requestPack.UserID = GameEnv.AccountId;  
      requestPack.InventoryId = 'testimg' .. i;
      i = i+1;
      print('Test request');
    end,
    function(errorCode, responsePack)
      print('Test response');
      print(errorCode);
      
      if errorCode == 0 then
        local newent = GameData:GetUserEntity(Game.KingsMan.MessagePack.EntityType.ET_PLAYER);
        print(newent.HeadIcon);
      end
    end
  );    
end


local context;

LobbyViewModel = {    
  ActivePanel = 5;
  WarningPanel = false;
  MissionId = 0,
  Users = {},

  Time = 60;
  Pointer = 0;

  UpdateMatch = function(result)
    local len = #result.MatchedUserList;
    LobbyViewModel.Users = {};
    for i=1, len, 1 do
      table.insert(LobbyViewModel.Users, result.MatchedUserList[i])
    end
  end, 
  
  UpdateViewModel = function()
    context:UpdateViewModel();        
  end,
  
  SwitchPanel = function(panelId)
    LobbyViewModel.ActivePanel = panelId;
    context:UpdateViewModel();    
  end,
      
  OnCreate = function(gameob, _context)
    logWarn('OnCreate ViewModel ' .. gameob.name)
    --transform = gameob.transform;
    context = _context;
  end,  
  
  OnTeam = function()    
    --LobbyModel.Test()
    LobbyModel.StartMatch(LobbyViewModel.MissionId, true);    
  end,
  
  OnMatch = function()
    LobbyViewModel.SwitchPanel(2);    
    -- LobbyModel.MatchTime();
    LobbyModel.StartMatch(LobbyViewModel.MissionId, false);   
  end,
  
  OnCancelMatch = function()
    LobbyModel.CancelMatch(LobbyViewModel.MissionId); 
  end,
  
  OnLeave = function()
    LobbyViewModel.WarningPanel = true;
    context:UpdateField('WarningPanel');
  end,
  
  OnCancelLeave = function()  
    LobbyViewModel.WarningPanel = false;
    context:UpdateField('WarningPanel');
  end,
  
  OnConfirmLeave = function()
    LobbyViewModel.WarningPanel = false;
    context:UpdateField('WarningPanel');
    LobbyModel.CancelMatch(LobbyViewModel.MissionId);
  end,
  
  OnMission = function()    
  end,
      
  OnStartMission = function()    
    ShowPanel('LobbyLoading');
    LobbyModel.BeginMatch(LobbyViewModel.MissionId);
    LobbyLoading.TimeUpdateRotate();
  end,  
  
  
  OnMap = function()    
    
    LobbyViewModel.SwitchPanel(1);    
  end,  
 
  OnToMap = function()    
    
    LobbyViewModel.SwitchPanel(5);    
  end,  
  OnClose = function()    
    
    ClosePanel('LobbySceneUI')
  end,    
};

