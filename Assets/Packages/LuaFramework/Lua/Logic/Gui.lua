require "ViewModel/LoginPanel"
require "ViewModel/MessagePanel"
require "ViewModel/CombatPanel"
require "ViewModel/MainMenu"
require "ViewModel/LobbyPanel"
require "ViewModel/BackPackPanel"
require "ViewModel/LobbyUserPanel"
require "ViewModel/HeroAttributePanel"
require "ViewModel/LobbyLoadingPanel"

local panels = {};

function PreloadPanel(name)
  local panel = panels[name];
  if (panel) then
    panel:SetActive(false);
  else
    panelMgr:CreatePanel(name, function(ob)
       panels[name] = ob;       
       ob:SetActive(false);
    end);    
  end
end

function ShowPanel(name)
  local panel = panels[name];
  if (panel) then
    panel:SetActive(true);
  else
    panelMgr:CreatePanel(name, function(ob)
       panels[name] = ob;       
    end);    
  end
end
  
function ClosePanel(name, destroy)
  if panels[name] == nil then return; end
  
  if (destroy) then
    panels[name] = nil;
    panelMgr:ClosePanel(name);
  else
    panels[name]:SetActive(false);
  end
end

