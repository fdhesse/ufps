require "3rd/pbc/protobuf"
local pb = require "3rd/pb"

ActionMgr = 
{
  action={};
  serveraction = {};
  callback = {};
  uid = 1;
  session = '';
};

function ActionMgr:CreateAction(_actionId)		
  return self:CreateActionEx(_actionId, -1)
end

function ActionMgr:CreateActionEx(_actionId)		
    local action_def = self.action[_actionId]
		
		if (action_def ~= nil)	then
      --[[
			local action_data = {}
			
      if action_def._resquestPackName ~= nil then
        --action_data.request  = self:GetPB(action_def._resquestPackName)
        action_data.request = action_def._resquestPackName();
        if (action_data.request == nil) then
          logWarn('ActionMgr:CreateAction, requestPack:'..tostring(action_def._resquestPackName)..' is invalid!')
          return nil
        end
      end
			
      if action_def._responsePackName ~= nil then
        --action_data.response = self:GetPB(action_def._responsePackName)		
        action_data.response = action_def._responsePackName();
        if (action_data.response == nil) then
          logWarn('ActionMgr:CreateAction, responsePack:'..tostring(action_def._responsePackName)..' is invalid!')
          return nil
        end			
      end
    
			action_data.actionid = action_def._actionid
			action_data.servertype = action_def._servertype 
			action_data.flags = action_def._flags
      action_data.serverid = serverId;
			return action_data;
      --]]
      --direct use action_def for performance
      return action_def;
		end
		
		logWarn('ActionMgr:CreateAction, action:'..tostring(_actionId)..' is invalid!')
		return nil
end

function ActionMgr:RegisterAction(actionid, servertype, flags, requestPackName, responsePackName)
	local action_def = {actionid = actionid, servertype = servertype, flags=flags, request=requestPackName(), response=responsePackName(), --[[responsePack=responsePackName--]]}
	self.action[actionid] =  action_def;
end

function ActionMgr:SendAction(serverid, action_data, callback)
	if (action_data ~= nil) then			
		  local head = Game.KingsMan.MessagePack.RequestHead();
      head.MsgId = self.uid;
      self.uid = self.uid + 1;
      head.ActionId = action_data.actionid;
      head.Session = self.session;
      head.ServerId = serverid;
      head.EncodeMode = 0;
      local headData = head:Serialize();
      local data = action_data.request:Serialize();
      if (data == false) then
        logWarn('Serialize action_data ' .. action_data.actionid .. ' failed');
        return;
      end
 
      local buffer = ByteBuffer.New();
      buffer:WriteInt(headData:len());
      buffer:WriteBuffer(headData);
      buffer:WriteBuffer(data);
      
      --here use head.ActionId instead of head.MsgId for performance, this should be enough as for same actionid we should use same callback
      self.callback[head.ActionId] = callback;
      networkMgr:SendMessage(buffer);
	end
end

function ActionMgr:OnMessage(buffer)		
  local headLen = buffer:ReadInt();
  local headData = buffer:ReadBuffer(headLen);
  
  local head = Game.KingsMan.MessagePack.ResponseHead():Parse(headData);
  local action_def = self.action[head.ActionId];
  
  --print(head.ActionId);
  --print(head.MsgId);
  if (action_def ~= nil)	then
    local callback = self.callback[head.ActionId];
    if callback ~= nil then      
      local msgLen = buffer.Len - headLen - 4;
      local msgData = buffer:ReadBuffer(msgLen);
            
      --print(msgLen);
      --local response = action_def.responsePack();
      local response = action_def.response;
      local msg = response:Parse(msgData);
      callback(head.ErrorCode, msg)
    end
  end  
end
