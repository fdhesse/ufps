require "3rd/pbc/protobuf"

local pb = require "3rd/pb"

local LoginCtrl = {};
local this = LoginCtrl;

local message;
local transform;
local gameObject;

local accountName;
local accountId;
local nickName;

--构建函数--
function LoginCtrl.New()
	logWarn("LoginCtrl.New--->>");
	return this;
end

function LoginCtrl.Awake()
	logWarn("LoginCtrl.Awake--->>");
	panelMgr:CreatePanel('Login', this.OnCreate);
end

--启动事件--
function LoginCtrl.OnCreate(obj)
	gameObject = obj;

	message = gameObject:GetComponent('LuaBehaviour');
	message:AddClick(LoginPanel.btnConfirm, this.OnLogin);
	message:AddClick(LoginPanel.btnCancel, this.OnTestPvp);

  gameObject:SetActive(false);
  gameObject:SetActive(true);

	--logWarn("Start lua--->>"..gameObject.name);
end

  
local function bin2hex(s)
    local s=string.gsub(s,"(.)",function (x) return string.format("%02X",string.byte(x)) end)
    return s
end

function LoginCtrl.OnConnect()
  Event.RemoveListener(Protocal.Connect, this.OnConnect); 
  
  print('CheckUserData');
  this.CheckUserData()
end

function LoginCtrl.CheckUserData()
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
          this.Close();
        end); 
      end
    end
  );
end

--单击事件--
function LoginCtrl.OnLogin(go)
  --LoginPanel.btnClose:SetActive(false);
--[[    
  local buffer = readfile("Proto/Common/CommonDefs");
  protobuf.register(buffer)
  
  buffer = readfile("Proto/Login11xxPack");
  protobuf.register(buffer)

  local data = {
      AccountName = "Alice";
      Password = "12345";
      Channel = "Channel";
      Device = "Device";
      Manufacturer = "Manufacturer";
      DeviceUUID = "DeviceUUID";        
  };
  
  local request = protobuf.encode("Game.KingsMan.MessagePack.Request1100Pack", data)
----]]
    
  local username = LoginPanel.inputUsername.text;
  local passwd = LoginPanel.inputPasswd.text;
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
        local ctrl = CtrlManager.GetCtrl(CtrlNames.Message);
        if ctrl ~= nil then
            ctrl:Awake();
            ctrl.SetText('' .. errorCode);
        end
      else    
        accountName = username;
        accountId = responsePack.AccountID;
        nickName = responsePack.NickName;
        if (nickName == nil) then
          nickName = '';
        end
        
        Event.AddListener(Protocal.Connect, this.OnConnect); 

        AppConst.SocketPort = responsePack.GatePort;
        AppConst.SocketAddress = responsePack.GateIP;
        ActionMgr.session = responsePack.Session;
        networkMgr:SendConnect();         
      end      
    end
  );
  
  --logWarn("Start lua--->>"..gameObject.name);
end

--关闭事件--
function LoginCtrl.Close()
	panelMgr:ClosePanel('Login');
  message = nil;
  gameObject = nil;
end

--单击事件--
function LoginCtrl.OnTestPvp(go)
  print('OnTestPvp');
  Game.EnterPvp(function()
    this.Close();
  end);   
end

return LoginCtrl;