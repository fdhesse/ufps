
--输出日志--
function log(str)
    Util.Log(str);
end

--错误日志--
function logError(str) 
	Util.LogError(str);
end

--警告日志--
function logWarn(str) 
	Util.LogWarning(str);
end

--查找对象--
function find(str)
	return GameObject.Find(str);
end

function destroy(obj)
	GameObject.Destroy(obj);
end

function newObject(prefab)
	return GameObject.Instantiate(prefab);
end

--创建面板--
function createPanel(name)
	PanelManager:CreatePanel(name);
end

function child(str)
	return transform:FindChild(str);
end

function subGet(childNode, typeName)		
	return child(childNode):GetComponent(typeName);
end

function findPanel(str) 
	local obj = find(str);
	if obj == nil then
		error(str.." is null");
		return nil;
	end
	return obj:GetComponent("BaseLua");
end

function split(s, delimiter)
    result = {};
    local start = 1;
    for i=1, #s, 1 do
      --print(string.sub(s, i, i))
      if string.sub(s, i, i) == delimiter then
        local match = string.sub(s, start, i-1)
        start = i+1;
        table.insert(result, match);
        --print(match);
      end
    end
    local match = string.sub(s, start)
    --print(match);
    table.insert(result, match);
    return result;
end

function FindChild(go,dir)
  if go.transform:FindChild(dir) == nil then
    print(dir..' is not exsit!')
    return
  end
  
  return go.transform:FindChild(dir).gameObject
end
