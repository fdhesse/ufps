local pb = require "3rd/pb"
require "3rd/pbc/protobuf"
InventoryDefTableDB = { 
  mFindTable = {};
  };

--启动事件--
function InventoryDefTableDB:Init()
  logWarn('InventoryDefTableDB:Init')
  --print(Application.dataPath)
  local cfgtype = Game.KingsMan.Cfg.InventoryDefTable
  local cfgData = cfgtype()
  --local path = 'Assets/Packages/LuaFramework/Lua/Data/cfg/InventoryDefTable.pb'
  --local addr = io.open(path, "rb")
  --local buffer = addr:read("*a")
  --addr:close()
  local buffer = readfile('cfg/InventoryDefTable.pb');
  --protobuf.register(buffer)

  --l-ocal t = protobuf.decode("Game.KingsMan.Cfg.InventoryDefTable", buffer)
  --local  data = pb.decode_raw(buffer)
  --print(#t.InventorysData)
  print(#buffer)
  local bytebuffer = ByteBuffer.New(buffer)
  local headData = bytebuffer:ReadBuffer(4 * 4 + 1 * 16)
  --print(#bytebuffer)
  local databuffer = bytebuffer:ReadBuffer(bytebuffer.Len - 4 * 4 - 1 * 16)
  bytebuffer:Close()
  cfgData:Parse(databuffer)
  print(#cfgData.InventorysData)
  
  --[[path = 'Assets/Package/LuaFramework/Lua/3rd';
  addr = io.open(path.."/pbc/addressbook.pb", "rb")
  buffer = addr:read "*a"
  addr:close()

  protobuf.register(buffer)

  t = protobuf.decode("google.protobuf.FileDescriptorSet", buffer)]]
  self:CreateFindTable(cfgData)
end

function InventoryDefTableDB:CreateFindTable(cfgData)
  print('InventoryDefTableDB:CreateFindTable')
  for i = 1,#cfgData.InventorysData do
    local data = cfgData.InventorysData[i]
    self.mFindTable[data.InventoryId] = data
  end
end

function InventoryDefTableDB:FindData(inventoryId)
  return self.mFindTable[inventoryId]
end
