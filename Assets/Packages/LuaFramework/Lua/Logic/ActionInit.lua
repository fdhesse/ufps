local pb = require "3rd/pb"

pb.require "ErrorEnum"
pb.require "ActionEnum"
pb.require "ServerEnum"

pb.require "RequestHead"
pb.require "ResponseHead"

pb.require "ExtensionPack"
pb.require "Login11xxPack"

pb.require "UserData"
pb.require "UserData50xxPack"
pb.require "Room95xx"
pb.require "RTMatch94xx"

-- Login
ActionMgr:RegisterAction(Game.KingsMan.MessagePack.ActionEnum.LoginAccount, Game.KingsMan.MessagePack.ServerEnum.Login,
	0, --Game.KingsMan.MessagePack.ActionFlag.AF_SYNCUD | Game.KingsMan.MessagePack.ActionFlag.AF_SYNCNOTICE,
	Game.KingsMan.MessagePack.Request1100Pack, Game.KingsMan.MessagePack.Response1100Pack);
  
  
ActionMgr:RegisterAction(Game.KingsMan.MessagePack.ActionEnum.CheckUserData, Game.KingsMan.MessagePack.ServerEnum.UserBusiness,
	0, --Game.KingsMan.MessagePack.ActionFlag.AF_SYNCUD | Game.KingsMan.MessagePack.ActionFlag.AF_SYNCNOTICE,
	Game.KingsMan.MessagePack.Request5000Pack, Game.KingsMan.MessagePack.Response5000Pack);  

  
ActionMgr:RegisterAction(Game.KingsMan.MessagePack.ActionEnum.ChangeHeadIcon, Game.KingsMan.MessagePack.ServerEnum.UserBusiness,
	0, --Game.KingsMan.MessagePack.ActionFlag.AF_SYNCUD | Game.KingsMan.MessagePack.ActionFlag.AF_SYNCNOTICE,
	Game.KingsMan.MessagePack.Request5007Pack, Game.KingsMan.MessagePack.Response5007Pack);  

--match
ActionMgr:RegisterAction(Game.KingsMan.MessagePack.ActionEnum.MatchStart, Game.KingsMan.MessagePack.ServerEnum.UserBusiness,
	0, --Game.KingsMan.MessagePack.ActionFlag.AF_SYNCUD | Game.KingsMan.MessagePack.ActionFlag.AF_SYNCNOTICE,
	Game.KingsMan.MessagePack.Request9420Pack, Game.KingsMan.MessagePack.Response9420Pack);  

ActionMgr:RegisterAction(Game.KingsMan.MessagePack.ActionEnum.MatchUpdate, Game.KingsMan.MessagePack.ServerEnum.UserBusiness,
	0, --Game.KingsMan.MessagePack.ActionFlag.AF_SYNCUD | Game.KingsMan.MessagePack.ActionFlag.AF_SYNCNOTICE,
	Game.KingsMan.MessagePack.Request9421Pack, Game.KingsMan.MessagePack.Response9421Pack);  

ActionMgr:RegisterAction(Game.KingsMan.MessagePack.ActionEnum.MatchCancel, Game.KingsMan.MessagePack.ServerEnum.UserBusiness,
	0, --Game.KingsMan.MessagePack.ActionFlag.AF_SYNCUD | Game.KingsMan.MessagePack.ActionFlag.AF_SYNCNOTICE,
	Game.KingsMan.MessagePack.Request9422Pack, Game.KingsMan.MessagePack.Response9422Pack);  

ActionMgr:RegisterAction(Game.KingsMan.MessagePack.ActionEnum.MatchBegin, Game.KingsMan.MessagePack.ServerEnum.UserBusiness,
	0, --Game.KingsMan.MessagePack.ActionFlag.AF_SYNCUD | Game.KingsMan.MessagePack.ActionFlag.AF_SYNCNOTICE,
	Game.KingsMan.MessagePack.Request9423Pack, Game.KingsMan.MessagePack.Response9423Pack);  

ActionMgr:RegisterAction(Game.KingsMan.MessagePack.ActionEnum.MatchHeartBeat, Game.KingsMan.MessagePack.ServerEnum.UserBusiness,
	0, --Game.KingsMan.MessagePack.ActionFlag.AF_SYNCUD | Game.KingsMan.MessagePack.ActionFlag.AF_SYNCNOTICE,
	Game.KingsMan.MessagePack.Request9424Pack, Game.KingsMan.MessagePack.Response9424Pack);  

ActionMgr:RegisterAction(Game.KingsMan.MessagePack.ActionEnum.MatchQuery, Game.KingsMan.MessagePack.ServerEnum.UserBusiness,
	0, --Game.KingsMan.MessagePack.ActionFlag.AF_SYNCUD | Game.KingsMan.MessagePack.ActionFlag.AF_SYNCNOTICE,
	Game.KingsMan.MessagePack.Request9425Pack, Game.KingsMan.MessagePack.Response9425Pack);  

ActionMgr:RegisterAction(Game.KingsMan.MessagePack.ActionEnum.MatchEnd, Game.KingsMan.MessagePack.ServerEnum.UserBusiness,
	0, --Game.KingsMan.MessagePack.ActionFlag.AF_SYNCUD | Game.KingsMan.MessagePack.ActionFlag.AF_SYNCNOTICE,
	Game.KingsMan.MessagePack.Request9426Pack, Game.KingsMan.MessagePack.Response9426Pack);  

--room
ActionMgr:RegisterAction(Game.KingsMan.MessagePack.ActionEnum.RoomCreate, Game.KingsMan.MessagePack.ServerEnum.Room,
	0, --Game.KingsMan.MessagePack.ActionFlag.AF_SYNCUD | Game.KingsMan.MessagePack.ActionFlag.AF_SYNCNOTICE,
	Game.KingsMan.MessagePack.Request9500Pack, Game.KingsMan.MessagePack.Response9500Pack);  

