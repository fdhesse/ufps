local pb = require "3rd/pb"

pb.require "Model/Player";
--pb.require "Model/TInventoryEntity";
pb.require "Model/TInventorySet";
--pb.require "Model/UUIDList";
pb.require "Cfg/InventoryDefTable";

require "Config/InventoryDefTableDB";

kRequestExtensionFieldNumber 					= 20005;
kResponseExtensionFieldNumber 					= 20005;

GameData =
{
-- public:
	Document										= nil;
	isVoice                     = true;        --记录音乐的开关状态
  isMusic                     = true;        --记录音效的开关状态
  SoundVolume = '';
  MusicVolume = '';
  AccountName                 = '';
  AccountPassWord             ='';
-- private:
    mUserEntitys                                    = {};
    --mLocalPath                                      = string.format('%sTactics/GameData.dat', GetApplicationPathPrivate());
    --mLocalTempPath                                  = string.format('%sTactics/GameData.tmp', GetApplicationPathPrivate());
    --mLocalBackupPath                                = string.format('%sTactics/GameData.bak', GetApplicationPathPrivate());	
    
    -- Action请求状态, 用于显示请求延迟转圈动画
    --mActionState              =  Game.KingsMan.Ui.UIActionState.UIAS_NO_ACTION;    -- 初始为没有任何Action发送
    mDelyTime                 = 2.0;
    
    SysNoticeVer = 0;
    DynamicVers = {};    
    --EntityTypeToEnumTable = {};
    
  mEntityChangedCB                                = {};    
};


function GameData:Init()
--[[  
    self:CreateEntity(Game.KingsMan.MessagePack.EntityType.GT_GUILD, Game.KingsMan.Model.GuildEntity);
    self:CreateEntity(Game.KingsMan.MessagePack.EntityType.GT_GUILDMUTABLE, Game.KingsMan.Model.GuildMutableEntity);
    self:CreateEntity(Game.KingsMan.MessagePack.EntityType.GT_GUILDMEMBER, Game.KingsMan.Model.GuildMemberEntity);
    self:CreateEntity(Game.KingsMan.MessagePack.EntityType.GT_GUILDLEAVE, Game.KingsMan.Model.GuildLeaveEntity);
    self:CreateEntity(Game.KingsMan.MessagePack.EntityType.GT_GUILDDONATE, Game.KingsMan.Model.GuildDonateEntity);
    self:CreateEntity(Game.KingsMan.MessagePack.EntityType.GT_GUILDACTIVITYDONATE, Game.KingsMan.Model.GuildActivityDonateEntity);
    self:CreateEntity(Game.KingsMan.MessagePack.EntityType.GT_GUILDCOMBAT, Game.KingsMan.Model.GuildCombatEntity);
    self:CreateEntity(Game.KingsMan.MessagePack.EntityType.GT_GUILDLEVELCHANGE, Game.KingsMan.Model.GuildLevelChangeEntity);
    self:CreateEntity(Game.KingsMan.MessagePack.EntityType.GT_GUILDUSERINFO, Game.KingsMan.Model.GuildUserInfoEntity);
 --]]
 logWarn('GameData:Init')
    self:CreateEntity(Game.KingsMan.MessagePack.EntityType.ET_PLAYER, Game.KingsMan.Model.Player);
    --self.EntityTypeToEnumTable['ET_PLAYER'] = Game.KingsMan.MessagePack.EntityType.ET_PLAYER
    self:CreateEntity(Game.KingsMan.MessagePack.EntityType.ET_TINVENTORYSET, Game.KingsMan.Model.TInventorySet)
    --self.EntityTypeToEnumTable['ET_TINVENTORYSET'] = Game.KingsMan.MessagePack.EntityType.ET_TINVENTORYSET
    InventoryDefTableDB:Init()
    --[[
    self:CreateEntity(Game.KingsMan.MessagePack.EntityType.ET_CARDMANAGER, Game.KingsMan.Model.CardManager);
    self:CreateEntity(Game.KingsMan.MessagePack.EntityType.ET_HEROCARDBAG, Game.KingsMan.Model.HeroCardBag);
    self:CreateEntity(Game.KingsMan.MessagePack.EntityType.ET_SKILLCARDBAG, Game.KingsMan.Model.SkillCardBag);
    self:CreateEntity(Game.KingsMan.MessagePack.EntityType.ET_ITEMBAG, Game.KingsMan.Model.ItemBag);
    self:CreateEntity(Game.KingsMan.MessagePack.EntityType.ET_LEVELBAG, Game.KingsMan.Model.LevelBag);
    self:CreateEntity(Game.KingsMan.MessagePack.EntityType.ET_DAILYTASKRECORD, Game.KingsMan.Model.DailyTaskRecord);
    self:CreateEntity(Game.KingsMan.MessagePack.EntityType.ET_ACTIVITYRECORD, Game.KingsMan.Model.ActivityRecord);
    self:CreateEntity(Game.KingsMan.MessagePack.EntityType.ET_HELPRECORD, Game.KingsMan.Model.HelpRecord);
    self:CreateEntity(Game.KingsMan.MessagePack.EntityType.ET_GUIDERECORD, Game.KingsMan.Model.GuideRecord";
    --self:CreateEntity(Game.KingsMan.MessagePack.EntityType.ET_GUIDERECORDNEW, "Game.KingsMan.Model.GuideRecordNew");
    self:CreateEntity(Game.KingsMan.MessagePack.EntityType.ET_PRODUCE, Game.KingsMan.Model.Produce);
    self:CreateEntity(Game.KingsMan.MessagePack.EntityType.ET_BLACKMARKET, Game.KingsMan.Model.BlackMarketRecord);
    self:CreateEntity(Game.KingsMan.MessagePack.EntityType.ET_MAILBOX2, Game.KingsMan.Model.MailboxEntity2);
    self:CreateEntity(Game.KingsMan.MessagePack.EntityType.ET_GUILD, Game.KingsMan.Model.GuildRecord);
    self:CreateEntity(Game.KingsMan.MessagePack.EntityType.ET_PVPLEVELBAG, Game.KingsMan.Model.PvpLevelBag);
    self:CreateEntity(Game.KingsMan.MessagePack.EntityType.ET_GUARDCARDBAG, Game.KingsMan.Model.GuardCardBag);
    self:CreateEntity(Game.KingsMan.MessagePack.EntityType.ET_GUARDCARDMAKERS, Game.KingsMan.Model.GuardCardMakers);
    self:CreateEntity(Game.KingsMan.MessagePack.EntityType.ET_SPECIALHUNTING, Game.KingsMan.Model.SpecialHuntingRecord);
    self:CreateEntity(Game.KingsMan.MessagePack.EntityType.ET_PROGRESSQUEUE, Game.KingsMan.Model.ProgressQueue);
    self:CreateEntity(Game.KingsMan.MessagePack.EntityType.ET_BUILDINGS, Game.KingsMan.Model.Buildings);
    self:CreateEntity(Game.KingsMan.MessagePack.EntityType.ET_PVPBATTLE, Game.KingsMan.Model.PvpBattleEntity);
    self:CreateEntity(Game.KingsMan.MessagePack.EntityType.ET_TREASUREBOXMGR, Game.KingsMan.Model.TreasureBoxMgr);
    self:CreateEntity(Game.KingsMan.MessagePack.EntityType.ET_MUNITIONSBOXMGR, Game.KingsMan.Model.MunitionsBoxMgr);
    self:CreateEntity(Game.KingsMan.MessagePack.EntityType.ET_VIP, Game.KingsMan.Model.VipRecord);
    self:CreateEntity(Game.KingsMan.MessagePack.EntityType.ET_RTBATTLECARDBAG, Game.KingsMan.Model.RTBattleCardBag);
    
    self:CreateEntity(Game.KingsMan.MessagePack.EntityType.ET_RTBATTLE, Game.KingsMan.Model.RTBattleEntity);
    self:CreateEntity(Game.KingsMan.MessagePack.EntityType.ET_GAMESYSTEMMASK, Game.KingsMan.Model.GameSystemMaskRecord);
    --]]
end

function GameData:CreateEntity(nEntityType, strEntityType)
    local pEntity = strEntityType();
    --assert(pEntity ~= nil, "failed to create message type of " .. strEntityType);
    --pEntity.Version = 2;
    pEntity.Version = -1;
    self.mUserEntitys[nEntityType] = pEntity;
end

function GameData:GetUserEntity(nEntityType)
    --assert(nEntityType >= Game.KingsMan.MessagePack.EntityType.ET_PLAYER and nEntityType < Game.KingsMan.MessagePack.EntityType.GT_GUILDENTITYCOUNT);
    return self.mUserEntitys[nEntityType];
end

local function dump_fields(unknown)
  print('dump_fields')
	for i,v in ipairs(unknown) do
		print(i, v.tag, v.wire, v.value)
		if type(v.value) == 'table' then
			dump_fields(v.value)
		end
	end
end

function GameData:CallServer(nActionId, serverId, onRequestFn, onResponseFn)
  --print(serverId);
	local action = ActionMgr:CreateActionEx(nActionId);
	onRequestFn(action.request);
	self:HandlerRequestPack(action, action.request);
  --self.mActionState = Game.KingsMan.Ui.UIActionState.UIAS_ACTION_JUST_SENDED;  --action状态为刚刚发送出去
  --self.mDelyTime = 2.0 --重置delyTime
  ActionMgr:SendAction(serverId, action, function(errorCode, actionResult)
      --[[
    --save action id, it should be a arg of onResponseFn, but for simple and old code, use global var here!!!
    GameEnv.ActionId = actionResult.actionid
    --print(GameEnv.ActionId)
    if GameEnv.IsPerfromanceTest then
      if errorCode == 0 then self:HandlerResponsePackBeforeCB(actionResult.response, nActionId); end
      onResponseFn(errorCode, actionResult.response)
      if errorCode == 0 then self:HandlerResponsePackAfterCB(actionResult.response, nActionId); end      
      return
    end
    
    --if self.mActionState == Game.KingsMan.Ui.UIActionState.UIAS_ACTION_DELAY then --如果action状态是UIAS_ACTION_DELAY状态
      --UIManager.ClosePopup()  --关闭弹框
    --end
    UIEnv:CloseWaitingUI()
    UIEnv:SetInputEnable(true);
    
    if self.mActionState ~= Game.KingsMan.Ui.UIActionState.UIAS_NO_ACTION then  --action状态重置
      self.mActionState = Game.KingsMan.Ui.UIActionState.UIAS_NO_ACTION;  --action状态重置
    end
    local params = {errorcode = errorCode,actionID =nActionId, ResponseFn = onResponseFn, Result =actionResult, ActionId = nActionId}    
    UI_Base:ScheduleActionEx("common", GameData_DelayCallResponse, params)
    --GameData_DelayCallResponse(params)
    --]]		
    --响应了回调函数
    
    --dump_fields(actionResult.unknown_fields);
    if errorCode == 0 then self:HandlerResponsePackBeforeCB(actionResult); end
    onResponseFn(errorCode, actionResult);
    if errorCode == 0 then self:HandlerResponsePackAfterCB(actionResult); end    
	end);
end

function GameData:HandlerRequestPack(action, requestPack)
	local extensionPack = Game.KingsMan.MessagePack.RequestExtensionPack();
  extensionPack.SysNoticeVer = 1;--self.SysNoticeVer;
  --print('HandlerRequestPack');
  extensionPack.DynamicVers = {};
  requestPack.unknown_fields:clearField();
  for i=1, #self.DynamicVers, 1 do
    extensionPack.DynamicVers[i] = self.DynamicVers[i];
  end
	--if (action.flags & Game.KingsMan.MessagePack.ActionFlag.AF_SYNCUD) ~= 0 then
    extensionPack.UserEntityVers = {};
    for nEntityType, pEntity in pairs(self.mUserEntitys) do
      --local entityVer = extensionPack.UserEntityVers:add();
      --print(nEntityType);
      local entityVer = {};
      --print(nEntityType)
      --print(pEntity.Version)
      entityVer.Type = nEntityType; entityVer.Version = pEntity.Version;
      extensionPack.UserEntityVers:Add(entityVer);
    end
	--end
	--pb.setUnknownField(requestPack, kRequestExtensionFieldNumber, extensionPack);
  --requestPack.unknown_fields = {};
  local buf = extensionPack:Serialize();
  local extension, off = pb.decode_raw(buf)
  requestPack.unknown_fields:addField(kRequestExtensionFieldNumber, 2, extension.unknown_fields);  
  --dump_fields(extension.unknown_fields);
end

function GameData:HandlerResponsePackBeforeCB(actionResult)  
  if (#actionResult.unknown_fields > 0) then
    --print(#actionResult.unknown_fields[1].value);
    --dump_fields(actionResult.unknown_fields);
    
    local fpack = require '3rd.pb.standard.pack'
    local buf = {}
    local off, len = fpack.pack_unknown_fields(buf, 0, 0, actionResult.unknown_fields[1].value);
    --print(#buf, off, len);
    local c = table.concat(buf, '', 1, off)
    
    local extensionPack = Game.KingsMan.MessagePack.ResponseExtensionPack();
    local extension = extensionPack:Parse(c);
    --local extension = Game.KingsMan.MessagePack.ResponseExtensionPack(actionResult.unknown_fields[1].value);
    if (extension) then
      --print('got extension');
      --print(extension);
      
      local notice = extension.SysNoticeMsg;
      --print(notice);
      if (notice ~= nil and notice.Version ~= 0) then
        self.SysNoticeVer = notice.Version;
        local msg = notice.Msg;
        print(msg);
        --TODO: sysnotice
        --UIEnv:CheckNoticeMsg(msg)
      end
    
      self:ReceiveResponsePackUD(extension);     
      self:ReceiveResponsePackDynamicFile(extension);      
    end
  end
end

function GameData:HandlerResponsePackAfterCB(actionResult)
end

function GameData:ReceiveResponsePackDynamicFile(extensionPack)
  --print(extensionPack.DynamicFiles);
  if extensionPack.DynamicFiles == nil then return end
  
	for i = 1, #extensionPack.DynamicFiles, 1 do
		local dynamicFile = extensionPack.DynamicFiles[i];        
    self.DynamicVers[dynamicFile.ID+1] = dynamicFile.Version;

    --GameEnv:UpdateDynamicFile(dynamicFile.Type, dynamicFile.Content);
	end
end

function GameData:ReceiveResponsePackUD(extensionPack)
	local entitysArray = {};
	for i = 1, #extensionPack.UserEntityPacks, 1 do
		local entityPack = extensionPack.UserEntityPacks[i];
		--local pEntity = self:GetUserEntity(self.EntityTypeToEnumTable[entityPack.Type]);
    local pEntity = self:GetUserEntity(entityPack.Type);
    --print(entityPack.Type); 
		if pEntity ~= nil and self:ReceiveEntityPack(pEntity, entityPack) then
      --print(entityPack.Type);
			--table.insert(entitysArray, {type = entityPack.Type, entity = pEntity});
      entitysArray[entityPack.Type] = pEntity;
		--else
			--assert(false, 'failed to receive response entity package of type ' .. Game.KingsMan.MessagePack.EntityType.NAME_TABLE[entityPack.Type]);
		end
	end
	self:DispatchEntityChangedEvent(entitysArray)
end

function GameData:ReceiveEntityPack(pEntity, entityPack)
	if entityPack.CompletePack then
    print('CompletePack')
		return pEntity:Parse(entityPack.CompletePack);
	elseif entityPack.ChangedPack then
    print('ChangedPack')
		return pEntity:Merge(entityPack.ChangedPack);
	end
  print('false')
	return false;
end

function GameData:RegisterEntityChangedCB(entityType, fnCallback)
  if self.mEntityChangedCB[entityType] == nil then self.mEntityChangedCB[entityType] = {}; end
  table.insert(self.mEntityChangedCB[entityType], fnCallback);
end

function GameData:DispatchEntityChangedEvent(entityArray)
  for entityType, pEntity in pairs(entityArray) do
    --local name = pb.fullname(pEntity);
    print('DispatchEntityChangedEvent ' .. entityType)
    local fnCallbacks = self.mEntityChangedCB[entityType];
    if type(fnCallbacks) == 'table' then
      for i = 1, #fnCallbacks, 1 do fnCallbacks[i](pEntity); end
    end
  end
end

