
local accountName;
local accountId;
local nickName;

LoginPanelModel = {};

local this = LoginPanelModel;

function LoginPanelModel.OnConnect()
  Event.RemoveListener(Protocal.Connect, this.OnConnect); 
  
  print('CheckUserData');
  this.CheckUserData()
end

function LoginPanelModel.CheckUserData()
   GameData:CallServer(Game.KingsMan.MessagePack.ActionEnum.CheckUserData, 9100, 
    function(requestPack)
      requestPack.AccountName = accountName;
      requestPack.AccountID = accountId;
      requestPack.AuthorToken = '3q4z3h7m6y4m6h8d7h4l';
      requestPack.NickName = nickName;
      requestPack.CharacterType = 'C1';   
      print(accountId);
      print(accountName);
      print(nickName);
      
      print('CheckUserData request');
    end,
    function(errorCode, responsePack)
      print('CheckUserData response');
      print(errorCode);
      print(responsePack)
      
      if errorCode == 0 then
        Game.EnterLobby(function()
          ClosePanel('Login');
        end); 
      
        --LobbyModel.BeginMatch(0, true);
        --LobbyModel.UpdateMatch(0);
       --LobbyModel.CancelMatch(0, false);
      end
    end
  );
end


function LoginPanelModel.Login(username, passwd)
  print(username);
  print(passwd);
  if username == nil or username == '' then
    username = 'pysong1'
  end
  if passwd == nil or passwd == '' then
    passwd = '1'
  end
  
  GameData:CallServer(Game.KingsMan.MessagePack.ActionEnum.LoginAccount, 0, 
    function(requestPack)
      requestPack.AccountName = username;
      requestPack.Password = passwd;
      requestPack.Channel = "Channel";
      requestPack.Device = "Device";
      requestPack.Manufacturer = "Manufacturer";
      requestPack.DeviceUUID = "DeviceUUID";           
    end,
    function(errorCode, responsePack)
      print(errorCode);
      print(responsePack)
  
      if errorCode ~= 0 then
        MessageViewModel.Message = '' .. errorCode;
        --MessagePanel.Show();
        ShowPanel('Message');
      else    
        accountName = username;
        accountId = responsePack.AccountID;
        nickName = responsePack.NickName;
        if (nickName == nil) then
          nickName = 'unknown';
        end
        
        
        GameEnv.AccountName = accountName;
        GameEnv.AccountId = accountId;
        GameEnv.NickName = nickName;
        
        Event.AddListener(Protocal.Connect, this.OnConnect); 

        AppConst.SocketPort = responsePack.GatePort;
        AppConst.SocketAddress = responsePack.GateIP;
        ActionMgr.session = responsePack.Session;
        
       -- GameData:LoadServerGroupTable(responsePack.ServerGroupMap);
       -- GameData:LoadUserServerTable(responsePack.UserServerGroups);        
        
       -- GameData:RefreshUsingServerID( responsePack.AccountID );
        
        networkMgr:SendConnect();         
      end      
    end
  );
end

local transform;
local context;
--local inputUsername;
--local inputPasswd;

LoginPanelViewModel = {   
  Monster = {
    {Name = 'Monster1'},
    {Name = 'Monster2'},
  
  },
 -- Time = os.date();
  Time = os.clock();
  UserName = nil,
  Password = nil,

  OnCreate = function(gameob, _context)
    logWarn('OnCreate ViewModel ' .. gameob.name)
    transform = gameob.transform;
    context = _context;
   
    --inputUsername = transform:FindChild("Username"):GetComponent("InputField");
    --inputPasswd = transform:FindChild("Password"):GetComponent("InputField");    
    
  end,
  
  OnLogin = function()
    logWarn('OnLogin ViewModel')
    --[[
    LoginPanelViewModel.UserName = "123";
      
    LoginPanelViewModel.Monster = {
      {Name = 'Monstera'},
      {Name = 'Monsterb'},
      {Name = 'Monsterc'},
    };
    
    LoginPanelViewModel.Monster[1].Name = 'aaa';

    --context:UpdateField('UserName');
    context:UpdateViewModel();
    --context:UpdateCollection('Monster', 1);
    --]]
    ShowPanel('Loading');
    LoginPanelModel.Login(LoginPanelViewModel.UserName, LoginPanelViewModel.Password)
    --logWarn('OnLogin----->>');
    --Game.EnterLobby(function()
      --ClosePanel('Login');
    --end);     
  end,
  
  OnEvent = function(event)
    --logWarn('OnEvent ViewModel')
  end,
};
